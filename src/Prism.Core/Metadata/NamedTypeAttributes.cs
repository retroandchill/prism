// @file TypeFlags.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;
using Prism.Core.Symbols;

namespace Prism.Core.Metadata;

[Archivable]
internal readonly partial record struct NamedTypeAttributes
{
    private const int KindShift = 0;
    private const uint KindMask = 0x7;

    private const int VisibilityShift = 3;
    private const uint VisibilityMask = 0x7 << VisibilityShift;

    [ArchiveInclude]
    private readonly uint _value;

    private NamedTypeAttributes(uint value)
    {
        _value = value;
    }

    public static NamedTypeAttributes Create(NamedTypeKind kind, DeclaredVisibility visibility)
    {
        return new NamedTypeAttributes(
            (uint)kind << KindShift | (uint)visibility << VisibilityShift
        );
    }

    [ArchiveIgnore]
    public NamedTypeKind Kind => (NamedTypeKind)((_value & KindMask) >> KindShift);

    [ArchiveIgnore]
    public DeclaredVisibility Visibility =>
        (DeclaredVisibility)((_value & VisibilityMask) >> VisibilityShift);
}
