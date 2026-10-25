// @file TypeFlags.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;
using Prism.Core.Symbols;

namespace Prism.Core.Metadata;

[Archivable]
internal readonly partial record struct VariableAttributes
{
    private const int KindShift = 0;
    private const uint KindMask = 0x3;

    private const int VisibilityShift = 2;
    private const uint VisibilityMask = 0x7 << VisibilityShift;

    [ArchiveInclude]
    private readonly uint _value;

    private VariableAttributes(uint value)
    {
        _value = value;
    }

    public static VariableAttributes Create(VariableKind kind, DeclaredVisibility visibility)
    {
        return new VariableAttributes(
            (uint)kind << KindShift | (uint)visibility << VisibilityShift
        );
    }

    [ArchiveIgnore]
    public VariableKind Kind => (VariableKind)((_value & KindMask) >> KindShift);

    [ArchiveIgnore]
    public DeclaredVisibility Visibility =>
        (DeclaredVisibility)((_value & VisibilityMask) >> VisibilityShift);
}
