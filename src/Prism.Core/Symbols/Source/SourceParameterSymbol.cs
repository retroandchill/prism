// @file SourceParameterSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Binding;
using Prism.Core.BoundTree;
using Prism.Core.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Source;

internal closed class SourceParameterSymbol : ParameterSymbol
{
    protected ParameterSyntax Syntax { get; }
    private SymbolCompletionState _completionState;

    internal SourceParameterSymbol(string name, Symbol containingSymbol, ParameterSyntax syntax)
        : base(name, containingSymbol)
    {
        Syntax = syntax;
    }

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

    public sealed override TypeSymbol Type
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

    public sealed override ParameterDefault? DefaultValue
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
                return new ConstantParameterDefault(literal.Value, Syntax.DefaultValue.Value);
            default:
                // Use null for a non-constant default value
                context.ReportDiagnostic(
                    Diagnostic.DefaultParameterValueMustBeConstant(
                        Syntax.DefaultValue.Value.Location
                    )
                );
                return new ConstantParameterDefault(
                    ConstantValue.Null(),
                    Syntax.DefaultValue.Value
                );
        }
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

    internal sealed override bool IsComplete(CompletionPart part)
    {
        return _completionState.IsComplete(part);
    }
}

internal sealed class SourceFunctionParameterSymbol : SourceParameterSymbol
{
    internal SourceFunctionParameterSymbol(
        string name,
        FunctionSymbol containingSymbol,
        ParameterSyntax syntax
    )
        : base(name, containingSymbol, syntax)
    {
        ContainingFunction = containingSymbol;

        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        compilation.CacheSymbol(Syntax, this);
    }

    public override FunctionSymbol ContainingFunction { get; }

    public override bool IsMutable => Syntax.MutableKeyword is not null;
}

internal sealed class SourceAttributeParameterSymbol : SourceParameterSymbol
{
    public SourceAttributeParameterSymbol(
        string name,
        Symbol containingSymbol,
        ParameterSyntax syntax
    )
        : base(name, containingSymbol, syntax)
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        compilation.CacheSymbol(Syntax, this);
    }

    public override bool IsMutable => false;
    public override FunctionSymbol? ContainingFunction => null;
}
