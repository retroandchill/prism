using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Prism.Core.Binding;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using ZLinq;

namespace Prism.Core.Symbols.Source;

internal sealed class SourceFunctionSymbol : FunctionSymbol
{
    private SymbolCompletionState _completionState;
    private readonly Lock _functionChecksLock = new();

    internal SourceFunctionSymbol(
        string name,
        Symbol? containingSymbol,
        FunctionDeclarationSyntax syntax
    )
        : base(name, containingSymbol)
    {
        Syntax = syntax;

        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        compilation.CacheSymbol(Syntax, this);
    }

    public FunctionDeclarationSyntax Syntax { get; }

    public override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedCompareExchange(
                ref field,
                [Syntax.Identifier.Location],
                default
            );
            return field;
        }
    }

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedCompareExchange(
                ref field,
                [new SyntaxReference(Syntax)],
                default
            );
            return field;
        }
    }

    public override TypeSymbol ReturnType
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

    private TypeSymbol ComputeReturnType(BindingContext context)
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);

        if (Syntax.ReturnType is null)
        {
            // Omitting the return type just results in void
            return compilation.GetSpecialType(SpecialType.Void);
        }

        var semanticModel = compilation.GetSemanticModel(Syntax.SyntaxTree);
        var binder = semanticModel.GetBinder(Syntax);
        return binder.ResolveType(Syntax.ReturnType.Type, context);
    }

    public override ImmutableArray<ParameterSymbol> Parameters
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedCompareExchange(
                ref field,
                ComputeParameters(),
                default
            );
            return field;
        }
    }

    private ImmutableArray<ParameterSymbol> ComputeParameters()
    {
        var source = Syntax.Parameters.Parameters;
        if (source.Count == 0)
            return [];

        var parameters = new ParameterSymbol[source.Count];
        foreach (var (i, syntax) in source.AsValueEnumerable().Index())
        {
            var name = syntax.Name.IdentifierName;
            parameters[i] = new SourceParameterSymbol(name, this, syntax);
        }

        return ImmutableCollectionsMarshal.AsImmutableArray(parameters);
    }

    internal override bool NeedsCompletion => false;

    internal override void ForceComplete(
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

    private void FunctionChecks(BindingContext context)
    {
        // TODO: Actually validate
    }

    internal override bool IsComplete(CompletionPart part)
    {
        return _completionState.IsComplete(part);
    }
}
