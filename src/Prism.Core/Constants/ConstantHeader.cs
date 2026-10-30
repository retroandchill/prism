// @file ConstantHeader.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using MagicArchive;

namespace Prism.Core.Constants;

internal enum ConstantKind : byte
{
    Null = 0,
    Bool = 1 << ConstantHeader.KindShift,
    Integer = 2 << ConstantHeader.KindShift,
    Float = 3 << ConstantHeader.KindShift,
    Character = 4 << ConstantHeader.KindShift,
    String = 5 << ConstantHeader.KindShift,
    Array = 6 << ConstantHeader.KindShift,
}

[Archivable]
internal readonly partial record struct ConstantHeader
{
    [ArchiveInclude]
    private readonly byte _value;

    private const byte SubTypeMask = IntegerKind.WidthMask | IntegerKind.SignMask;
    private const byte KindMask = unchecked((byte)~SubTypeMask);
    internal const byte KindShift = IntegerKind.SignShift + 1;

    private ConstantHeader(byte value)
    {
        _value = value;
    }

    internal static ConstantHeader FromRaw(byte raw) => new(raw);

    public static ConstantHeader Null => new((byte)ConstantKind.Null);

    public static ConstantHeader Bool => new((byte)ConstantKind.Bool);

    public static ConstantHeader Integer(IntegerKind kind) =>
        new((byte)((byte)ConstantKind.Integer | kind.ToRaw()));

    public static ConstantHeader Float(FloatKind kind) =>
        new((byte)((byte)ConstantKind.Float | (byte)kind));

    public static ConstantHeader Character(CharacterKind kind) =>
        new((byte)((byte)ConstantKind.Character | (byte)kind));

    public static ConstantHeader String => new((byte)ConstantKind.String);

    public static ConstantHeader Array => new((byte)ConstantKind.Array);

    [ArchiveIgnore]
    public ConstantKind Kind => (ConstantKind)(_value & KindMask);

    [ArchiveIgnore]
    public IntegerKind IntegerKind
    {
        get
        {
            Debug.Assert(Kind == ConstantKind.Integer);
            return IntegerKind.FromRaw((byte)(_value & SubTypeMask));
        }
    }

    [ArchiveIgnore]
    public FloatKind FloatKind
    {
        get
        {
            Debug.Assert(Kind == ConstantKind.Float);
            return (FloatKind)(_value & SubTypeMask);
        }
    }

    [ArchiveIgnore]
    public CharacterKind CharacterKind
    {
        get
        {
            Debug.Assert(Kind == ConstantKind.Character);
            return (CharacterKind)(_value & SubTypeMask);
        }
    }

    internal byte AsRaw() => _value;
}
