using System.Collections.Immutable;
using System.Diagnostics;
using JetBrains.Annotations;
using Prism.Core.Binding;
using Prism.Core.BoundTree;
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
    public VariableDeclarationSyntax Syntax { get; }

    private ConstantValue? _constantValue;
    private bool _constantValueComputed;
    private TypeSymbol? _type;

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
            if (_type is not null)
                return _type;

            using var context = CreateBindingContext();
            if (Interlocked.CompareExchange(ref _type, ComputeType(context), null) is not null)
                return _type;

            AddDeclarationDiagnostics(context);
            _completionState.MarkPartComplete(CompletionPart.Type);
            return _type;
        }
    }

    [MustDisposeResource]
    protected abstract BindingContext CreateBindingContext();

    protected abstract TypeSymbol ComputeType(BindingContext context);

    internal void ForceSetType(TypeSymbol type)
    {
        Debug.Assert(IsLocal);

        if (Interlocked.CompareExchange(ref _type, type, null) is not null)
            return;

        _completionState.MarkPartComplete(CompletionPart.Type);
    }

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
            if (_constantValueComputed)
                return _constantValue;

            using var context = BindingContext.Create();
            var (value, computed) = TryComputeConstantValue(context);
            if (!computed)
                return _constantValue;

            _constantValue = value;
            AddDeclarationDiagnostics(context);
            _completionState.MarkPartComplete(CompletionPart.ConstantValue);
            return _constantValue;
        }
    }

    private (ConstantValue?, bool) TryComputeConstantValue(BindingContext context)
    {
        var value = ComputeConstantValue(context);
        return !Interlocked.CompareExchange(ref _constantValueComputed, true, false)
            ? (value, true)
            : (_constantValue, false);
    }

    protected abstract ConstantValue? ComputeConstantValue(BindingContext context);

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

    protected override BindingContext CreateBindingContext()
    {
        return BindingContext.Discarded;
    }

    protected override TypeSymbol ComputeType(BindingContext context)
    {
        if (Syntax.Type is not null)
        {
            return _scopeBinder.ResolveType(Syntax.Type.Type, context);
        }

        if (Syntax.Initializer is null)
        {
            context.ReportDiagnostic(Diagnostic.ExpectedTypeSpecifier(Syntax.Identifier.Location));
            return ErrorTypeSymbol.Unnamed;
        }

        var initializer = GetInitializer(context);
        return initializer.Type;
    }

    protected override ConstantValue? ComputeConstantValue(BindingContext context)
    {
        if (Syntax.Initializer is null)
            return null;

        var initializer = GetInitializer(context);
        return initializer.ConstantValue;
    }

    private BoundExpression GetInitializer(BindingContext context)
    {
        Debug.Assert(_initializerBinder is not null);
        Debug.Assert(Syntax.Initializer is not null);
        return _initializerBinder.BindExpression(Syntax.Initializer.Value, context);
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

    protected override BindingContext CreateBindingContext()
    {
        return BindingContext.Create();
    }

    protected override TypeSymbol ComputeType(BindingContext context)
    {
        if (Syntax.Type is null)
        {
            context.ReportDiagnostic(Diagnostic.ExpectedTypeSpecifier(Syntax.Identifier.Location));
            return ErrorTypeSymbol.Unnamed;
        }

        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        var factory = compilation.GetBinderFactory(Syntax.SyntaxTree);
        var binder = factory.GetBinder(Syntax);
        return binder.ResolveType(Syntax.Type.Type, context);
    }

    protected override ConstantValue? ComputeConstantValue(BindingContext context)
    {
        if (Syntax.Initializer is null)
            return null;

        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        var initializer = compilation.GetBoundInitializer(this);
        return initializer?.ConstantValue;
    }
}
