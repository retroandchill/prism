// @file SourceLabelSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Prism.Compiler.Binding;
using Prism.Diagnostics;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Syntax;

namespace Prism.Compiler.Symbols.Source;

internal sealed class SourceLabelSymbol : SourceSymbol, ILabelSymbolData
{
    private readonly LabeledStatementSyntax _syntax;

    public SourceLabelSymbol(
        Compilation compilation,
        FunctionSymbol containingFunction,
        LabeledStatementSyntax syntax
    )
        : base(compilation)
    {
        _syntax = syntax;
        ContainingFunction = containingFunction;
        PublicSymbol = new LabelSymbol(this);
    }

    public new LabelSymbol PublicSymbol
    {
        get => Unsafe.As<LabelSymbol>(base.PublicSymbol);
        private init => base.PublicSymbol = value;
    }

    public override string Name => _syntax.Identifier.IdentifierName;

    public FunctionSymbol ContainingFunction { get; }

    public override Symbol ContainingSymbol => ContainingFunction;

    public override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedInitialize(ref field, [_syntax.Location]);
            return field;
        }
    }

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedInitialize(ref field, [new SyntaxReference(_syntax)]);
            return field;
        }
    }
}
