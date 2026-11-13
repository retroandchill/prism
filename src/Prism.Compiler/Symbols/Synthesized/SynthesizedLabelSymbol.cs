// @file SynthesizedLabelSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Prism.Semantic;

namespace Prism.Compiler.Symbols.Synthesized;

internal sealed class SynthesizedLabelSymbol : SynthesizedSymbol, IFunctionOwnedSymbolData
{
    public SynthesizedLabelSymbol(Compilation compilation, FunctionSymbol containingFunction)
        : base(compilation)
    {
        ContainingFunction = containingFunction;
        PublicSymbol = new LabelSymbol(this);
    }

    public new LabelSymbol PublicSymbol
    {
        get => Unsafe.As<LabelSymbol>(base.PublicSymbol);
        private init => base.PublicSymbol = value;
    }

    public override string Name => "";

    public FunctionSymbol ContainingFunction { get; }

    public override Symbol ContainingSymbol => ContainingFunction;
}
