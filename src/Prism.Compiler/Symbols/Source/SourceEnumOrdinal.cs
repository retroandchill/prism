// @file SourceEnumOrdinal.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Prism.Compiler.Binding;
using Prism.Compiler.Mappers;
using Prism.Constants;
using Prism.Diagnostics;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Syntax;
using Prism.Utils;

namespace Prism.Compiler.Symbols.Source;

internal sealed class SourceEnumOrdinal : SourceSymbol, IEnumOrdinalSymbolData
{
    private SymbolCompletionState _completionState;
    private readonly EnumOrdinalDeclarationSyntax _syntax;
    private readonly SourceEnumOrdinal? _previous;
    private ConstantValue _constantValue;
    private bool _constantValueComputed;

    public SourceEnumOrdinal(
        Compilation declaringCompilation,
        EnumOrdinalDeclarationSyntax syntax,
        NamedTypeSymbol containingType,
        SourceEnumOrdinal? previous
    )
        : base(declaringCompilation)
    {
        _syntax = syntax;
        ContainingType = containingType;
        _previous = previous;
        PublicSymbol = new EnumOrdinalSymbol(this);
    }

    public new EnumOrdinalSymbol PublicSymbol
    {
        get => Unsafe.As<EnumOrdinalSymbol>(base.PublicSymbol);
        private init => base.PublicSymbol = value;
    }

    public override string Name => _syntax.Identifier.IdentifierName;
    public override Symbol ContainingSymbol => ContainingType;

    public override MemberContainerSymbol ContainingMemberContainer => ContainingType;

    public override NamedTypeSymbol ContainingType { get; }

    public override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedCompareExchange(
                ref field,
                [_syntax.Identifier.Location],
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
                [new SyntaxReference(_syntax)],
                default
            );
            return field;
        }
    }

    public ConstantValue Value
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

    private (ConstantValue, bool) TryComputeConstantValue(BindingContext context)
    {
        var value = ComputeConstantValue(context);
        return !Interlocked.CompareExchange(ref _constantValueComputed, true, false)
            ? (value, true)
            : (_constantValue, false);
    }

    private ConstantValue ComputeConstantValue(BindingContext context)
    {
        var underlying = ContainingType.EnumUnderlyingType;
        Debug.Assert(underlying is not null);

        var integerKind = underlying.SpecialType.ToIntegerKind(
            DeclaringCompilation.Settings.PointerWidth
        );

        if (_syntax.Value is not null)
        {
            var factory = DeclaringCompilation.GetBinderFactory(_syntax.SyntaxTree);
            var binder = factory.GetBinder(_syntax);

            var boundValue = binder.BindInitializer(
                underlying,
                _syntax.Value,
                context,
                CancellationToken.None
            );

            if (boundValue.ConstantValue is { } constant)
            {
                return constant;
            }

            // If we can't compute a constant, then we need to fall back to what we had before
            context.ReportDiagnostic(
                Diagnostic.NotConstantExpression(
                    _syntax.Value.Value.Location,
                    _syntax.Value.Value.ToString()
                )
            );
        }

        if (_previous is null)
        {
            return IntegerConstant.Zero(integerKind);
        }

        return _previous
            .Value.TryBinary(ConstantBinaryOperation.Addition, IntegerConstant.One(integerKind))
            .RequireNonNull();
    }
}
