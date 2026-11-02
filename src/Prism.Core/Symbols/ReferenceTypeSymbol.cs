// @file ReferenceTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols;

public sealed class ReferenceTypeSymbol : TypeSymbol
{
    internal ReferenceTypeSymbol(TypeSymbol referencedType, bool isMutable)
        : base($"{referencedType.Name}{(isMutable ? " mutable" : "")}&")
    {
        ReferencedType = referencedType;
        IsMutable = isMutable;
    }

    public TypeSymbol ReferencedType { get; }

    public bool IsMutable { get; }

    public override bool IsDynamicallySized => false;
    public override bool IsDefaultConstructible => false;

    public override ImmutableArray<Location> Locations => [];
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override ImmutableArray<Symbol> GetMembers() => [];

    public override ImmutableArray<Symbol> GetMembers(string name) => [];

    public override void WriteDisplayString(TextWriter writer)
    {
        ReferencedType.WriteDisplayString(writer);
        writer.Write(IsMutable ? " mutable&" : "&");
    }

    public override bool Equals(TypeSymbol other, TypeCompareKind kind)
    {
        return Equals(other as ReferenceTypeSymbol, kind);
    }

    private bool Equals(ReferenceTypeSymbol? other, TypeCompareKind kind)
    {
        if (ReferenceEquals(this, other))
            return true;

        if (other is null || ReferencedType.Equals(other.ReferencedType, kind))
        {
            return false;
        }

        if (kind.HasFlag(TypeCompareKind.IgnoreMutability))
            return true;

        return IsMutable == other.IsMutable;
    }

    public override int GetHashCode(TypeCompareKind kind)
    {
        var hashCode = new HashCode();

        hashCode.Add(ReferencedType.GetHashCode(kind));

        if (!kind.HasFlag(TypeCompareKind.IgnoreMutability))
        {
            hashCode.Add(IsMutable);
        }

        return hashCode.ToHashCode();
    }
}
