// @file TypeFlags.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;

namespace Prism.Metadata;

[Archivable]
internal readonly partial record struct TypeAliasAttributes
{
    private const uint VisibilityMask = 0x7;

    [ArchiveInclude]
    private readonly uint _value;

    private TypeAliasAttributes(uint value)
    {
        _value = value;
    }

    public static TypeAliasAttributes Create(DeclaredVisibility visibility)
    {
        return new TypeAliasAttributes((uint)visibility);
    }

    [ArchiveIgnore]
    public DeclaredVisibility Visibility
    {
        get => (DeclaredVisibility)((_value & VisibilityMask));
        init => _value = (_value & ~VisibilityMask);
    }
}
