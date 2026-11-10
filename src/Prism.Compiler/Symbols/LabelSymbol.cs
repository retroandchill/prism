// @file LabelSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Compiler.Symbols;

public closed class LabelSymbol : Symbol
{
    public sealed override Symbol ContainingSymbol => ContainingFunction;

    public abstract FunctionSymbol ContainingFunction { get; }

    public sealed override void WriteDisplayString(TextWriter writer)
    {
        writer.Write(Name);
    }
}
