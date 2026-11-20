// @file EnumOrdinalSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Cysharp.Text;
using Prism.Constants;
using Prism.Semantic.Symbols.Data;

namespace Prism.Semantic.Symbols;

public sealed class EnumOrdinalSymbol : Symbol
{
    internal EnumOrdinalSymbol(IEnumOrdinalSymbolData data)
        : base(data) { }

    internal new IEnumOrdinalSymbolData Data => Unsafe.As<IEnumOrdinalSymbolData>(base.Data);

    public ConstantValue Value => Data.Value;

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        if (
            ContainingSymbol
            is not null
                and not AssemblySymbol
                and not FunctionSymbol
                and NamespaceSymbol { IsGlobal: false }
        )
        {
            ContainingSymbol.WriteDisplayString(ref builder);
            builder.Append("::");
        }

        builder.Append(Name);
    }
}
