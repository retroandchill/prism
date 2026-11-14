// @file ErrorLabelSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Diagnostics;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Syntax;

namespace Prism.Compiler.Symbols.Error;

internal sealed class ErrorLabelSymbol : ILabelSymbolData
{
    public ErrorLabelSymbol(string name, FunctionSymbol containingFunction)
    {
        Name = name;
        ContainingFunction = containingFunction;
        PublicSymbol = new LabelSymbol(this);
    }

    public LabelSymbol PublicSymbol { get; }
    public string Name { get; }
    public Symbol ContainingSymbol => ContainingFunction;
    public FunctionSymbol ContainingFunction { get; }
}
