// @file LabelSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Symbols;

public abstract class LabelSymbol(string name, FunctionSymbol containingFunction)
    : Symbol(name, containingFunction)
{
    public FunctionSymbol ContainingFunction => (FunctionSymbol)ContainingSymbol!;

    public sealed override void WriteDisplayString(TextWriter writer)
    {
        writer.Write(Name);
    }
}
