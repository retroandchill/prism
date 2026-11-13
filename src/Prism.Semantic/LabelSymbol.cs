// @file LabelSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Cysharp.Text;

namespace Prism.Semantic;

public sealed class LabelSymbol : Symbol
{
    internal LabelSymbol(IFunctionOwnedSymbolData data)
        : base(data)
    {
        Debug.Assert(data.ContainingFunction is not null);
    }

    internal new IFunctionOwnedSymbolData Data => Unsafe.As<IFunctionOwnedSymbolData>(base.Data);

    public FunctionSymbol ContainingFunction => Data.ContainingFunction!;

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        builder.Append(Name);
    }
}
