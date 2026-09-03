// @file ReferenceTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols;

public sealed class ReferenceTypeSymbol : TypeSymbol
{
    internal ReferenceTypeSymbol(TypeSymbol referencedType, bool isMutable)
        : base(referencedType.Name)
    {
        ReferencedType = referencedType;
        IsMutable = isMutable;
    }

    public TypeSymbol ReferencedType { get; }

    public bool IsMutable { get; }

    public override bool IsDynamicallySized => false;

    public override ImmutableArray<Location> Locations => [];
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override ImmutableArray<Symbol> GetMembers() => [];

    public override ImmutableArray<Symbol> GetMembers(string name) => [];

    public override void WriteDisplayString(TextWriter writer)
    {
        ReferencedType.WriteDisplayString(writer);
        writer.Write(IsMutable ? " mutable&" : "&");
    }
}
