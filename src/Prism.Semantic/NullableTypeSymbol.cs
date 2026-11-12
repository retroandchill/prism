// @file NullableTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Prism.Semantic;

public sealed class NullableTypeSymbol : TypeSymbol
{
    internal NullableTypeSymbol(TypeSymbol elementType)
        : base(new DataInternal(elementType)) { }

    private new DataInternal Data => Unsafe.As<DataInternal>(base.Data);

    public TypeSymbol ElementType => Data.ElementType;

    private sealed record DataInternal(TypeSymbol ElementType) : ITypeSymbolData
    {
        public string Name => $"{ElementType.Name}?";
        public Symbol? ContainingSymbol => null;
        public bool IsDynamicallySized => false;
        public bool IsDefaultConstructible => true;
    }
}
