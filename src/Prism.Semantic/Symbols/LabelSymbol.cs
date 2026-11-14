// @file LabelSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using Prism.Semantic.Symbols.Data;

namespace Prism.Semantic.Symbols;

public sealed class LabelSymbol : Symbol
{
    internal LabelSymbol(ILabelSymbolData data)
        : base(data)
    {
        Debug.Assert(data.ContainingFunction is not null);
    }

    internal new ILabelSymbolData Data => Unsafe.As<ILabelSymbolData>(base.Data);

    public FunctionSymbol ContainingFunction => Data.ContainingFunction;

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        builder.Append(Name);
    }
}
