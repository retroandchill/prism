// @file ArrayTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Prism.Semantic;

public sealed class ArrayTypeSymbol : TypeSymbol
{
    internal ArrayTypeSymbol(TypeSymbol elementType, ulong? size = null)
        : base(new DataInternal(elementType, size)) { }

    private new DataInternal Data => Unsafe.As<DataInternal>(base.Data);

    public TypeSymbol ElementType => Data.ElementType;

    public ulong? Size => Data.Size;

    private sealed record DataInternal(TypeSymbol ElementType, ulong? Size) : ITypeSymbolData
    {
        public string Name => Size is { } s ? $"{ElementType.Name}[{s}]" : $"{ElementType.Name}[]";
        public Symbol? ContainingSymbol => null;
        public bool IsDynamicallySized => Size is null;
        public bool IsDefaultConstructible =>
            Size is not null && (Size == 0 || ElementType.IsDefaultConstructible);
    }
}
