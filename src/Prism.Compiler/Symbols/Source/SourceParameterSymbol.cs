// @file SourceParameterSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Prism.Compiler.Binding;
using Prism.Compiler.BoundTree;
using Prism.Constants;
using Prism.Diagnostics;
using Prism.Semantic;
using Prism.Syntax;

namespace Prism.Compiler.Symbols.Source;

internal closed class SourceParameterSymbol
    : SourceSymbol,
        IParameterSymbolData,
        IAttributeHolderSymbolData,
        ICompletableSymbolData
{
    protected ParameterSyntax Syntax { get; }
    private SymbolCompletionState _completionState;
    private ImmutableArray<AttributeData> _attributes;

    internal SourceParameterSymbol(Compilation compilation, ParameterSyntax syntax)
        : base(compilation)
    {
        Syntax = syntax;
        PublicSymbol = new ParameterSymbol(this);
        DeclaringCompilation.CacheSymbol(Syntax, PublicSymbol);
    }

    public new ParameterSymbol PublicSymbol
    {
        get => Unsafe.As<ParameterSymbol>(base.PublicSymbol);
        private init => base.PublicSymbol = value;
    }

    public sealed override string Name => Syntax.Name.IdentifierName;

    public abstract bool IsMutable { get; }

    public sealed override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedCompareExchange(
                ref field,
                [Syntax.Name.Location],
                default
            );
            return field;
        }
    }

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

    public TypeSymbol Type
    {
        get
        {
            if (field is not null)
                return field;

            using var context = BindingContext.Create();
            if (Interlocked.CompareExchange(ref field, ComputeType(context), null) is not null)
                return field;

            AddDeclarationDiagnostics(context);
            _completionState.MarkPartComplete(CompletionPart.Type);
            return field;
        }
    }

    private TypeSymbol ComputeType(BindingContext context)
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        var factory = compilation.GetBinderFactory(Syntax.SyntaxTree);
        var binder = factory.GetBinder(Syntax);

        Debug.Assert(Syntax.TypeSpecifier is not null);
        return binder.ResolveType(Syntax.TypeSpecifier.Type, context);
    }

    public ParameterDefault? DefaultValue
    {
        get
        {
            if (Syntax.DefaultValue is null)
                return null;

            if (field is not null)
                return field;

            using var context = BindingContext.Create();
            if (
                Interlocked.CompareExchange(ref field, CreateParameterDefault(context), null)
                is not null
            )
                return field;

            AddDeclarationDiagnostics(context);
            _completionState.MarkPartComplete(CompletionPart.ConstantValue);
            return field;
        }
    }

    private ParameterDefault CreateParameterDefault(BindingContext context)
    {
        Debug.Assert(Syntax.DefaultValue is not null);

        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        var factory = compilation.GetBinderFactory(Syntax.SyntaxTree);
        var binder = factory.GetBinder(Syntax);

        var expression = binder.BindExpression(
            Syntax.DefaultValue.Value,
            Type,
            context,
            CancellationToken.None
        );
        switch (expression)
        {
            case BoundLiteral literal:
                return new ConstantParameterDefault(literal.Value);
            default:
                // Use null for a non-constant default value
                context.ReportDiagnostic(
                    Diagnostic.DefaultParameterValueMustBeConstant(
                        Syntax.DefaultValue.Value.Location
                    )
                );
                return new ConstantParameterDefault(new NullConstant());
        }
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
                    _ = DefaultValue;
                    _completionState.MarkPartComplete(CompletionPart.ConstantValue);
                    break;
                case CompletionPart.None:
                    return;
                default:
                    _completionState.MarkPartComplete(
                        CompletionPart.All & ~CompletionPart.ParameterAll
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

internal sealed class SourceFunctionParameterSymbol(
    Compilation compilation,
    FunctionSymbol containingSymbol,
    ParameterSyntax syntax
) : SourceParameterSymbol(compilation, syntax), IFunctionOwnedSymbolData
{
    public FunctionSymbol ContainingFunction { get; } = containingSymbol;

    public override Symbol ContainingSymbol => ContainingFunction;

    public override bool IsMutable => Syntax.MutableKeyword is not null;
}

internal sealed class SourceAttributeParameterSymbol(
    Compilation compilation,
    NamedTypeSymbol containingSymbol,
    ParameterSyntax syntax
) : SourceParameterSymbol(compilation, syntax)
{
    public override TypeSymbol ContainingType { get; } = containingSymbol;

    public override Symbol ContainingSymbol => ContainingType;

    public override bool IsMutable => false;
}
