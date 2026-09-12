// @file ArrayTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Prism.Core.Configuration;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols;

public sealed class ArrayTypeSymbol : TypeSymbol
{
    internal ArrayTypeSymbol(TypeSymbol elementType, ulong? size = null)
        : base(size is { } s ? $"{elementType.Name}[{s}]" : $"{elementType.Name}[]")
    {
        ElementType = elementType;
        Size = size;
    }

    public TypeSymbol ElementType { get; }

    public ulong? Size { get; }

    [MemberNotNullWhen(false, nameof(Size))]
    public override bool IsDynamicallySized => Size is null;

    public override ulong GetSizeInBytes(CompilationSettings settings)
    {
        if (IsDynamicallySized)
            throw new InvalidOperationException("Cannot get size of dynamically sized type");

        return ElementType.GetSizeInBytes(settings) * Size.Value;
    }

    public override ImmutableArray<Location> Locations => [];

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override ImmutableArray<Symbol> GetMembers() => [];

    public override ImmutableArray<Symbol> GetMembers(string name) => [];

    public override void WriteDisplayString(TextWriter writer)
    {
        ElementType.WriteDisplayString(writer);
        writer.Write('[');
        if (Size is { } size)
        {
            writer.Write(size);
        }
        writer.Write(']');
    }
}
