// @file ErrorLabelSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Semantic.Symbols.Data;

namespace Prism.Semantic.Symbols.Error;

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
