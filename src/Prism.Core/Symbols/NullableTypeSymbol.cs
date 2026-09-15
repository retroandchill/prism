// @file NullableTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Configuration;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols;

public sealed class NullableTypeSymbol : TypeSymbol
{
    internal NullableTypeSymbol(TypeSymbol elementType)
        : base($"{elementType.Name}?")
    {
        ElementType = elementType;
    }

    public TypeSymbol ElementType { get; }

    public override bool IsDynamicallySized => false;

    public override SizeAndAlignment GetSizeAndAlignment(CompilationSettings settings)
    {
        if (ElementType is ReferenceTypeSymbol refType)
        {
            return refType.GetSizeAndAlignment(settings);
        }

        var elementSize = ElementType.GetSizeAndAlignment(settings);
        if (elementSize.IsZero)
        {
            return new SizeAndAlignment(1, 1);
        }

        // TODO: Check that this logic is correct.
        // For a non-zero sized type, adding a single extra byte should add the alignment's worth in size
        return elementSize with
        {
            Size = elementSize.Size + elementSize.Alignment,
        };
    }

    public override ImmutableArray<Location> Locations => [];
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override ImmutableArray<Symbol> GetMembers() => [];

    public override ImmutableArray<Symbol> GetMembers(string name) => [];

    public override void WriteDisplayString(TextWriter writer)
    {
        ElementType.WriteDisplayString(writer);
        writer.Write('?');
    }
}
