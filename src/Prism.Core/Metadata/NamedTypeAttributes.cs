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

    private const int SpecialTypeShift = VisibilityShift + 3;
    private const uint SpecialTypeMask = 0x3F << SpecialTypeShift;

    [ArchiveInclude]
    private readonly uint _value;

    private NamedTypeAttributes(uint value)
    {
        _value = value;
    }

    public static NamedTypeAttributes Create(
        NamedTypeKind kind,
        DeclaredVisibility visibility,
        SpecialType specialType
    )
    {
        return new NamedTypeAttributes(
            (uint)kind << KindShift
                | (uint)visibility << VisibilityShift
                | (uint)specialType << SpecialTypeShift
        );
    }

    [ArchiveIgnore]
    public NamedTypeKind Kind
    {
        get => (NamedTypeKind)((_value & KindMask) >> KindShift);
        init => _value = (_value & ~KindMask) | (uint)value << KindShift;
    }

    [ArchiveIgnore]
    public DeclaredVisibility Visibility
    {
        get => (DeclaredVisibility)((_value & VisibilityMask) >> VisibilityShift);
        init => _value = (_value & ~VisibilityMask) | (uint)value << VisibilityShift;
    }

    [ArchiveIgnore]
    public SpecialType SpecialType
    {
        get => (SpecialType)((_value & SpecialTypeMask) >> SpecialTypeShift);
        init => _value = (_value & ~SpecialTypeMask) | (uint)value << SpecialTypeShift;
    }
}
