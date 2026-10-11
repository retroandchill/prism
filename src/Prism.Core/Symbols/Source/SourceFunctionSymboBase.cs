using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Prism.Core.Abi;
using Prism.Core.Binding;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Symbols.Source;

internal closed class SourceFunctionSymbolBase : FunctionSymbol
{
    private SymbolCompletionState _completionState;
    private readonly Lock _functionChecksLock = new();
    private readonly DeclarationModifiers _modifiers;
    private ImmutableArray<AttributeData> _attributes;

    internal SourceFunctionSymbolBase(
        string name,
        MemberContainerSymbol containingSymbol,
        DeclarationSyntax syntax
    )
        : base(name, containingSymbol)
    {
        _modifiers = DeclarationModifiers.MakeModifiers(containingSymbol, syntax.Modifiers);
    }

    protected abstract DeclarationSyntax AbstractSyntax { get; }

    public sealed override DeclaredVisibility DeclaredVisibility =>
        DeclaredVisibility.FromDeclarationModifiers(_modifiers);

    public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedCompareExchange(
                ref field,
                [new SyntaxReference(AbstractSyntax)],
                default
            );
            return field;
        }
    }

    public sealed override TypeSymbol ReturnType
    {
        get
        {
            if (field is not null)
                return field;

            using var context = BindingContext.Create();
            if (
                Interlocked.CompareExchange(ref field, ComputeReturnType(context), null) is not null
            )
                return field;

            AddDeclarationDiagnostics(context);
            _completionState.MarkPartComplete(CompletionPart.Type);
            return field;
        }
    }

    protected abstract TypeSymbol ComputeReturnType(BindingContext context);

    public sealed override TypeSymbol? ReceiverType
    {
        get
        {
            if (field is not null)
                return field;

            var containerType = ContainingType;
            if (containerType is null || _modifiers.HasFlag(DeclarationModifiers.Static))
                return null;

            Interlocked.CompareExchange(ref field, ComputeReceiverType(containerType), null);
            return field;
        }
    }

    protected abstract TypeSymbol ComputeReceiverType(TypeSymbol containerType);

    public sealed override ImmutableArray<ParameterSymbol> Parameters
    {
        get
        {
            if (!field.IsDefault)
                return field;

            using var context = BindingContext.Create();
            if (!ImmutableInterlocked.InterlockedInitialize(ref field, ComputeParameters(context)))
                return field;

            AddDeclarationDiagnostics(context);
            return field;
        }
    }

    internal override AbiKind AbiKind => AbiKind.Internal;
    public sealed override bool IsExtern => _modifiers.HasFlag(DeclarationModifiers.Extern);

    protected abstract ImmutableArray<ParameterSymbol> ComputeParameters(BindingContext context);

    protected ImmutableArray<ParameterSymbol> CreateParametersFromSource(
        SeparatedSyntaxList<ParameterSyntax> source,
        BindingContext context
    )
    {
        if (source.Count == 0)
            return [];

        var parameters = new ParameterSymbol[source.Count];
        var parametersByName = new Dictionary<string, List<ParameterSyntax>>();
        var seenDefaultValue = false;
        foreach (var (i, syntax) in source.AsValueEnumerable().Index())
        {
            var name = syntax.Name.IdentifierName;
            var list = parametersByName.GetOrAdd(name, () => []);
            list.Add(syntax);
            var param = new SourceFunctionParameterSymbol(name, this, syntax);
            parameters[i] = param;

            if (param.HasDefaultValue)
            {
                seenDefaultValue = true;
            }
            else if (seenDefaultValue)
            {
                context.ReportDiagnostic(
                    Diagnostic.DefaultValueAfterNonDefaultValue(syntax.Location, name)
                );
            }
        }

        foreach (var (name, syntax) in parametersByName)
        {
            if (syntax.Count == 1)
                continue;

            context.ReportDiagnostic(
                Diagnostic.DuplicateDeclaration(
                    syntax[0].Location,
                    [.. syntax[1..].Select(x => x.Location)],
                    name
                )
            );
        }

        return ImmutableCollectionsMarshal.AsImmutableArray(parameters);
    }

    public sealed override ImmutableArray<AttributeData> GetAttributes()
    {
        if (!_attributes.IsDefault)
            return _attributes;

        using var context = BindingContext.Create();
        if (
            !ImmutableInterlocked.InterlockedInitialize(ref _attributes, ComputeAttributes(context))
        )
            return _attributes;

        AddDeclarationDiagnostics(context);
        _completionState.MarkPartComplete(CompletionPart.Attributes);
        return _attributes;
    }

    private ImmutableArray<AttributeData> ComputeAttributes(BindingContext context)
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        var semanticModel = compilation.GetSemanticModel(AbstractSyntax.SyntaxTree);
        var binder = semanticModel.GetBinder(AbstractSyntax);
        return binder.BindAttributes(AbstractSyntax.Attributes, context);
    }

    internal sealed override bool NeedsCompletion => true;

    internal sealed override void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    )
    {
        if (filter?.Invoke(this) == false)
            return;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var incompletePart = _completionState.NextIncompletePart;
            switch (incompletePart)
            {
                case CompletionPart.Attributes:
                    _ = GetAttributes();
                    break;
                case CompletionPart.Type:
                    _ = ReturnType;
                    break;
                case CompletionPart.Parameters:
                    foreach (var parameter in Parameters)
                    {
                        parameter.ForceComplete(location, null, cancellationToken);
                    }

                    _completionState.MarkPartComplete(CompletionPart.Parameters);
                    break;
                case CompletionPart.StartChecks or CompletionPart.FinishChecks:
                    LazyFunctionChecks();
                    break;
                case CompletionPart.None:
                    return;
                default:
                    _completionState.MarkPartComplete(
                        CompletionPart.All & ~CompletionPart.FunctionAll
                    );
                    break;
            }

            _completionState.WaitPartComplete(incompletePart, cancellationToken);
        }
    }

    private void LazyFunctionChecks()
    {
        if (_completionState.IsComplete(CompletionPart.FinishChecks))
            return;

        using var scope = _functionChecksLock.EnterScope();
        if (!_completionState.MarkPartComplete(CompletionPart.StartChecks))
            return;

        using var context = BindingContext.Create();
        try
        {
            FunctionChecks(context);
            AddDeclarationDiagnostics(context);
        }
        finally
        {
            _completionState.MarkPartComplete(CompletionPart.FinishChecks);
        }
    }

    protected abstract void FunctionChecks(BindingContext context);

    internal sealed override bool IsComplete(CompletionPart part)
    {
        return _completionState.IsComplete(part);
    }
}
