// @file ValueSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Prism.Semantic.Symbols.Data;

namespace Prism.Semantic.Symbols;

public closed class ValueSymbol : Symbol
{
    private protected ValueSymbol(IValueSymbolData data)
        : base(data) { }

    internal new IValueSymbolData Data => Unsafe.As<IValueSymbolData>(base.Data);

    public TypeSymbol Type => Data.Type;

    public bool IsMutable => Data.IsMutable;
}
