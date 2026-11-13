using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Prism.Compiler.Binding;
using Prism.Compiler.BoundTree;
using Prism.Compiler.Semantic;
using Prism.Compiler.Symbols.Error;
using Prism.Diagnostics;
using Prism.Metadata;
using Prism.Semantic;
using Prism.Syntax;
using Prism.Text;
using ZLinq;
using ConstantValue = Prism.Constants.ConstantValue;

namespace Prism.Compiler.Symbols.Source;

internal closed class SourceVariableSymbol
    : SourceSymbol,
        IVariableSymbolData,
        IAttributeHolderSymbolData,
        ICompletableSymbolData
{
    private SymbolCompletionState _completionState;
    public VariableDeclarationSyntax Syntax { get; }

    private ConstantValue? _constantValue;
    private bool _constantValueComputed;
    private TypeSymbol? _type;
    private ImmutableArray<AttributeData> _attributes;

    protected SourceVariableSymbol(Compilation compilation, VariableDeclarationSyntax syntax)
        : base(compilation)
    {
        Syntax = syntax;
        IsMutable = syntax
            .Modifiers.AsValueEnumerable()
            .Any(t => t.Kind == SyntaxKind.MutableKeyword);
        PublicSymbol = new VariableSymbol(this);
        DeclaringCompilation.CacheSymbol(Syntax, PublicSymbol);
    }

    public new VariableSymbol PublicSymbol
    {
        get => Unsafe.As<VariableSymbol>(base.PublicSymbol);
        private init => base.PublicSymbol = value;
    }

    public sealed override string Name => Syntax.Identifier.IdentifierName;

    public abstract VariableKind Kind { get; }

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

    public TypeSymbol Type
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
        Debug.Assert(PublicSymbol.IsLocal);

        if (Interlocked.CompareExchange(ref _type, type, null) is not null)
            return;

        _completionState.MarkPartComplete(CompletionPart.Type);
    }

    public bool IsMutable { get; }

    public bool HasInitializer => Syntax.Initializer is not null;

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

    public ConstantValue? ConstantValue
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

    private ConstantValue? ComputeConstantValue(BindingContext context)
    {
        if (Syntax.Initializer is null || !Syntax.IsConst)
            return null;

        var initializer = GetInitializer(context);
        var value = initializer.ConstantValue;

        if (value is null)
        {
            context.ReportDiagnostic(
                Diagnostic.NotConstantExpression(
                    Syntax.Initializer.Location,
                    Syntax.Initializer.ToString()
                )
            );
        }

        return value;
    }

    protected abstract BoundExpression GetInitializer(BindingContext context);

    public sealed override bool IsDefinedInSourceTree(ISourceDocument tree, TextSpan? definedWithin)
    {
        return IsDefinedInSourceTree(new SyntaxReference(Syntax), tree, definedWithin);
    }

    public ImmutableArray<AttributeData> GetAttributes()
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
        var semanticModel = compilation.GetSemanticModel(Syntax.SyntaxTree);
        var binder = semanticModel.GetBinder(Syntax);
        return binder.BindAttributes(Syntax.Attributes, context);
    }

    public void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    )
    {
        if (filter?.Invoke(PublicSymbol) == false)
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

    public bool IsComplete(CompletionPart part)
    {
        return _completionState.IsComplete(part);
    }
}

internal sealed class SourceLocalVariableSymbol(
    Compilation compilation,
    FunctionSymbol containingSymbol,
    LocalVariableDeclarationSyntax syntax,
    Binder scopeBinder,
    Binder? initializerBinder
) : SourceVariableSymbol(compilation, syntax), IFunctionOwnedSymbolData
{
    public FunctionSymbol ContainingFunction { get; } = containingSymbol;
    public override Symbol ContainingSymbol => ContainingFunction;

    public override VariableKind Kind => VariableKind.Local;

    protected override BindingContext CreateBindingContext()
    {
        return BindingContext.Discarded;
    }

    protected override TypeSymbol ComputeType(BindingContext context)
    {
        if (Syntax.Type is not null)
        {
            var targetType = scopeBinder.ResolveType(Syntax.Type.Type, context);
            if (!targetType.IsDynamicallySized)
                return targetType;
        }

        if (Syntax.Initializer is null)
        {
            context.ReportDiagnostic(Diagnostic.ExpectedTypeSpecifier(Syntax.Identifier.Location));
            return ErrorTypeSymbol.Unnamed.PublicSymbol;
        }

        var initializer = GetInitializer(context);
        return initializer.Type;
    }

    protected override BoundExpression GetInitializer(BindingContext context)
    {
        Debug.Assert(initializerBinder is not null);
        Debug.Assert(Syntax.Initializer is not null);
        return initializerBinder.BindExpression(
            Syntax.Initializer.Value,
            context,
            CancellationToken.None
        );
    }
}

internal sealed class SourceGlobalVariableSymbol(
    Compilation compilation,
    MemberContainerSymbol containingSymbol,
    GlobalVariableDeclarationSyntax syntax
) : SourceVariableSymbol(compilation, syntax), IVisibilityGuardedSymbolData
{
    private readonly DeclarationModifiers _modifiers = DeclarationModifiers.MakeModifiers(
        containingSymbol,
        syntax.Modifiers
    );

    public override MemberContainerSymbol ContainingMemberContainer { get; } = containingSymbol;
    public override Symbol ContainingSymbol => ContainingMemberContainer;

    public override VariableKind Kind => VariableKind.Global;

    public override string MetadataName
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, ProduceMetadataName(Syntax.SyntaxTree), null);
            return field;
        }
    }

    public DeclaredVisibility DeclaredVisibility =>
        DeclaredVisibility.FromDeclarationModifiers(_modifiers);

    protected override BindingContext CreateBindingContext()
    {
        return BindingContext.Create();
    }

    protected override TypeSymbol ComputeType(BindingContext context)
    {
        Debug.Assert(Syntax.Type is not null);
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        var factory = compilation.GetBinderFactory(Syntax.SyntaxTree);
        var binder = factory.GetBinder(Syntax);
        return binder.ResolveType(Syntax.Type.Type, context);
    }

    protected override BoundExpression GetInitializer(BindingContext context)
    {
        Debug.Assert(Syntax.Initializer is not null);

        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        var factory = compilation.GetBinderFactory(Syntax.SyntaxTree);
        var binder = factory.GetBinder(Syntax);
        return binder.BindInitializer(
            PublicSymbol,
            Syntax.Initializer,
            context,
            CancellationToken.None
        );
    }
}

internal sealed class SourceFieldSymbol(
    Compilation compilation,
    NamedTypeSymbol containingSymbol,
    FieldDeclarationSyntax syntax
) : SourceVariableSymbol(compilation, syntax), IVisibilityGuardedSymbolData
{
    private readonly DeclarationModifiers _modifiers = DeclarationModifiers.MakeModifiers(
        containingSymbol,
        syntax.Modifiers
    );

    public override TypeSymbol ContainingType { get; } = containingSymbol;
    public override MemberContainerSymbol ContainingMemberContainer => ContainingType;
    public override Symbol ContainingSymbol => ContainingType;

    public override VariableKind Kind =>
        _modifiers.HasFlag(DeclarationModifiers.Static) || Syntax.IsConst
            ? VariableKind.Class
            : VariableKind.Instance;

    public override string MetadataName
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, ProduceMetadataName(Syntax.SyntaxTree), null);
            return field;
        }
    }

    public DeclaredVisibility DeclaredVisibility =>
        DeclaredVisibility.FromDeclarationModifiers(_modifiers);

    protected override BindingContext CreateBindingContext()
    {
        return BindingContext.Create();
    }

    protected override TypeSymbol ComputeType(BindingContext context)
    {
        Debug.Assert(Syntax.Type is not null);

        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        var factory = compilation.GetBinderFactory(Syntax.SyntaxTree);
        var binder = factory.GetBinder(Syntax);
        return binder.ResolveType(Syntax.Type.Type, context);
    }

    protected override BoundExpression GetInitializer(BindingContext context)
    {
        Debug.Assert(Syntax.Initializer is not null);

        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        var factory = compilation.GetBinderFactory(Syntax.SyntaxTree);
        var binder = factory.GetBinder(Syntax);
        return binder.BindInitializer(
            PublicSymbol,
            Syntax.Initializer,
            context,
            CancellationToken.None
        );
    }
}
