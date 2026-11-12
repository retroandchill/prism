// @file ReferenceTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Semantic;

public sealed class ReferenceTypeSymbol : TypeSymbol
{
    internal ReferenceTypeSymbol(TypeSymbol referencedType, bool isMutable)
        : base(new DataInternal(referencedType, isMutable)) { }

    private new DataInternal Data => (DataInternal)base.Data;

    public TypeSymbol ReferencedType => Data.ReferencedType;

    public bool IsMutable => Data.IsMutable;

    private sealed record DataInternal(TypeSymbol ReferencedType, bool IsMutable) : ITypeSymbolData
    {
        public string Name => $"{ReferencedType.Name}{(IsMutable ? " mutable" : "")}&";
        public Symbol? ContainingSymbol => null;
        public bool IsDynamicallySized => false;
        public bool IsDefaultConstructible => false;
    }
}
