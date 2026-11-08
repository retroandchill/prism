// @file FunctionAttributes.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;
using Prism.Core.Abi;
using Prism.Core.Symbols;

namespace Prism.Core.Metadata;

[Archivable]
internal readonly partial record struct FunctionAttributes
{
    private const uint VisibilityMask = 0x7;

    private const uint ExternBit = 0x8;

    // Right now we only have two ABI options, but let's reserve some bits for future expansion.
    private const int AbiShift = 4;
    private const uint AbiMask = 0xF << AbiShift;

    [ArchiveInclude]
    private readonly uint _value;

    private FunctionAttributes(uint value)
    {
        _value = value;
    }

    public static FunctionAttributes Create(
        DeclaredVisibility visibility,
        bool isExtern,
        AbiKind abiKind
    )
    {
        return new FunctionAttributes(
            (uint)visibility | (isExtern ? ExternBit : 0) | ((uint)abiKind << AbiShift)
        );
    }

    [ArchiveIgnore]
    public DeclaredVisibility Visibility
    {
        get => (DeclaredVisibility)(_value & VisibilityMask);
        init => _value = (_value & ~VisibilityMask) | (uint)value;
    }

    [ArchiveIgnore]
    public bool IsExtern
    {
        get => (_value & ExternBit) != 0;
        init => _value = (_value & ~ExternBit) & (value ? ExternBit : 0);
    }

    [ArchiveIgnore]
    public AbiKind AbiKind
    {
        get => (AbiKind)((_value & AbiMask) >> AbiShift);
        init => _value = (_value & ~AbiMask) | ((uint)value << AbiShift);
    }
}
