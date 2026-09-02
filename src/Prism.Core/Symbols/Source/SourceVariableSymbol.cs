using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Binding;
using Prism.Core.BoundTree;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Symbols.Error;
using Prism.Core.Syntax;
using Prism.Core.Text;
using ZLinq;

namespace Prism.Core.Symbols.Source;

internal abstract class SourceVariableSymbol : VariableSymbol
{
    private SymbolCompletionState _completionState;
    protected VariableDeclarationSyntax Syntax { get; }

    private Lazy<ConstantValue?>? _constantValue;

    protected SourceVariableSymbol(
        string name,
        Symbol? containingSymbol,
        VariableDeclarationSyntax syntax
    )
        : base(name, containingSymbol)
    {
        Syntax = syntax;
        IsMutable = syntax
            .Modifiers.AsValueEnumerable()
            .Any(t => t.Kind == SyntaxKind.MutableKeyword);
    }

    public sealed override ImmutableArray<Location> Locations
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

    public sealed override TypeSymbol Type
    {
        get
        {
            if (field is not null)
                return field;

            var diagnostics = DiagnosticBag.Create();
            if (Interlocked.CompareExchange(ref field, ComputeType(diagnostics), null) is not null)
                return field;

            AddDeclarationDiagnostics(diagnostics);
            _completionState.MarkPartComplete(CompletionPart.Type);
            return field;
        }
    }

    protected abstract TypeSymbol ComputeType(DiagnosticBag diagnostics);

    public sealed override bool IsMutable { get; }

    public sealed override bool HasInitializer => Syntax.Initializer is not null;

    public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
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

    public sealed override ConstantValue? ConstantValue
    {
        get
        {
            if (_constantValue is not null)
                return _constantValue.Value;

            var diagnostics = DiagnosticBag.Create();
            if (
                Interlocked.CompareExchange(
                    ref _constantValue,
                    new Lazy<ConstantValue?>(
                        () => ComputeConstantValue(diagnostics),
                        LazyThreadSafetyMode.PublicationOnly
                    ),
                    null
                )
                is not null
            )
                return _constantValue.Value;

            AddDeclarationDiagnostics(diagnostics);
            _completionState.MarkPartComplete(CompletionPart.ConstantValue);
            return _constantValue.Value;
        }
    }

    protected abstract ConstantValue? ComputeConstantValue(DiagnosticBag diagnostics);

    public sealed override bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithin)
    {
        return IsDefinedInSourceTree(new SyntaxReference(Syntax), tree, definedWithin);
    }

    internal sealed override bool NeedsCompletion => false;

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
                case CompletionPart.Type:
                    _ = Type;
                    break;
                case CompletionPart.ConstantValue:
                    _ = ConstantValue;
                    break;
                case CompletionPart.None:
                    return;
                default:
                    _completionState.MarkPartComplete(
                        CompletionPart.All & ~CompletionPart.VariableAll
                    );
                    break;
            }

            _completionState.WaitPartComplete(incompletePart, cancellationToken);
        }
    }

    internal sealed override bool IsComplete(CompletionPart part)
    {
        return _completionState.IsComplete(part);
    }
}

internal sealed class SourceLocalVariableSymbol : SourceVariableSymbol
{
    private readonly Binder _scopeBinder;
    private readonly Binder? _initializerBinder;

    public SourceLocalVariableSymbol(
        string name,
        Symbol? containingSymbol,
        VariableDeclarationSyntax syntax,
        Binder scopeBinder,
        Binder? initializerBinder
    )
        : base(name, containingSymbol, syntax)
    {
        _scopeBinder = scopeBinder;
        _initializerBinder = initializerBinder;

        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        compilation.CacheSymbol(Syntax, this);
    }

    public override bool IsGlobal => false;

    protected override TypeSymbol ComputeType(DiagnosticBag diagnostics)
    {
        var context = LookupContext.Create(diagnostics);
        if (Syntax.Type is not null)
        {
            return _scopeBinder.ResolveType(Syntax.Type.Type, context);
        }

        if (Syntax.Initializer is null)
        {
            diagnostics.Add(Diagnostic.ExpectedTypeSpecifier(Syntax.Identifier.Location));
            return ErrorTypeSymbol.Unnamed;
        }

        var initializer = GetInitializer(context);
        return initializer.Type;
    }

    protected override ConstantValue? ComputeConstantValue(DiagnosticBag diagnostics)
    {
        if (Syntax.Initializer is null)
            return null;

        var context = LookupContext.Create(diagnostics);
        var initializer = GetInitializer(context);
        return initializer.ConstantValue;
    }

    private BoundExpression GetInitializer(LookupContext context)
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        var semanticModel = compilation.GetSemanticModel(Syntax.SyntaxTree);
        Debug.Assert(_initializerBinder is not null);
        var initializer = semanticModel.GetBoundVariableInitializer(
            Syntax,
            _initializerBinder,
            context
        );
        return initializer;
    }
}

internal sealed class SourceGlobalVariableSymbol : SourceVariableSymbol
{
    public SourceGlobalVariableSymbol(
        string name,
        Symbol? containingSymbol,
        VariableDeclarationSyntax syntax
    )
        : base(name, containingSymbol, syntax)
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        compilation.CacheSymbol(Syntax, this);
    }

    public override bool IsGlobal => true;

    protected override TypeSymbol ComputeType(DiagnosticBag diagnostics)
    {
        if (Syntax.Type is null)
        {
            diagnostics.Add(Diagnostic.ExpectedTypeSpecifier(Syntax.Identifier.Location));
            return ErrorTypeSymbol.Unnamed;
        }

        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        var factory = compilation.GetBinderFactory(Syntax.SyntaxTree);
        var binder = factory.GetBinder(Syntax);
        var context = LookupContext.Create(diagnostics);
        return binder.ResolveType(Syntax.Type.Type, context);
    }

    protected override ConstantValue? ComputeConstantValue(DiagnosticBag diagnostics)
    {
        if (Syntax.Initializer is null)
            return null;

        var context = LookupContext.Create(diagnostics);
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        var semanticModel = compilation.GetSemanticModel(Syntax.SyntaxTree);
        var initializer = semanticModel.GetBoundVariableInitializer(Syntax, context);
        return initializer.ConstantValue;
    }
}
