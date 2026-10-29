// @file IntegerConstant.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using MagicArchive;

namespace Prism.Core.Constants;

internal enum IntegerWidth : byte
{
    W8,
    W16,
    W32,
    W64,
    W128,
}

internal enum IntegerSign : byte
{
    Unsigned,
    Signed,
}

[Archivable]
internal readonly partial struct IntegerHeader
{
    [ArchiveInclude]
    private readonly byte _value;

    private const byte WidthMask = 0b0000_0111;
    private const byte SignMask = 0b0000_1000;
    private const byte SignShift = 3;

    private IntegerHeader(byte value)
    {
        _value = value;
    }

    public static IntegerHeader Create(IntegerWidth width, IntegerSign sign)
    {
        return new IntegerHeader((byte)((byte)width | (byte)((byte)sign << SignShift)));
    }

    [ArchiveIgnore]
    public IntegerWidth Width => (IntegerWidth)(_value & WidthMask);

    [ArchiveIgnore]
    public IntegerSign Sign => (IntegerSign)((_value & SignMask) >> SignShift);
}

[Archivable(GenerateType.Custom)]
[Union]
public readonly partial struct IntegerConstant : IUnion
{
    private readonly IntegerHeader _header;
    private readonly UInt128 _rawValue;

    private IntegerConstant(IntegerHeader header, UInt128 value)
    {
        _header = header;
        _rawValue = value;
    }

    public IntegerConstant(sbyte value)
    {
        _header = IntegerHeader.Create(IntegerWidth.W8, IntegerSign.Signed);
        _rawValue = unchecked((byte)value);
    }

    public IntegerConstant(byte value)
    {
        _header = IntegerHeader.Create(IntegerWidth.W8, IntegerSign.Unsigned);
        _rawValue = value;
    }

    public IntegerConstant(short value)
    {
        _header = IntegerHeader.Create(IntegerWidth.W16, IntegerSign.Signed);
        _rawValue = unchecked((ushort)value);
    }

    public IntegerConstant(ushort value)
    {
        _header = IntegerHeader.Create(IntegerWidth.W16, IntegerSign.Unsigned);
        _rawValue = value;
    }

    public IntegerConstant(int value)
    {
        _header = IntegerHeader.Create(IntegerWidth.W32, IntegerSign.Signed);
        _rawValue = unchecked((uint)value);
    }

    public IntegerConstant(uint value)
    {
        _header = IntegerHeader.Create(IntegerWidth.W32, IntegerSign.Unsigned);
        _rawValue = value;
    }

    public IntegerConstant(long value)
    {
        _header = IntegerHeader.Create(IntegerWidth.W64, IntegerSign.Signed);
        _rawValue = unchecked((ulong)value);
    }

    public IntegerConstant(ulong value)
    {
        _header = IntegerHeader.Create(IntegerWidth.W64, IntegerSign.Unsigned);
        _rawValue = value;
    }

    public IntegerConstant(Int128 value)
    {
        _header = IntegerHeader.Create(IntegerWidth.W128, IntegerSign.Signed);
        _rawValue = unchecked((UInt128)value);
    }

    public IntegerConstant(UInt128 value)
    {
        _header = IntegerHeader.Create(IntegerWidth.W128, IntegerSign.Unsigned);
        _rawValue = value;
    }

    public object Value
    {
        get
        {
            return _header.Sign switch
            {
                IntegerSign.Unsigned => _header.Width switch
                {
                    IntegerWidth.W8 => (byte)_rawValue,
                    IntegerWidth.W16 => (ushort)_rawValue,
                    IntegerWidth.W32 => (uint)_rawValue,
                    IntegerWidth.W64 => (ulong)_rawValue,
                    IntegerWidth.W128 => _rawValue,
                    _ => throw new ArgumentOutOfRangeException(),
                },
                IntegerSign.Signed => _header.Width switch
                {
                    IntegerWidth.W8 => unchecked((sbyte)(byte)_rawValue),
                    IntegerWidth.W16 => unchecked((short)(ushort)_rawValue),
                    IntegerWidth.W32 => unchecked((int)(uint)_rawValue),
                    IntegerWidth.W64 => unchecked((long)(ulong)_rawValue),
                    IntegerWidth.W128 => unchecked((Int128)_rawValue),
                    _ => throw new ArgumentOutOfRangeException(),
                },
                _ => throw new InvalidOperationException("Invalid sign"),
            };
        }
    }

    public bool TryGetValue(out sbyte value)
    {
        if (_header is { Sign: IntegerSign.Signed, Width: IntegerWidth.W8 })
        {
            value = unchecked((sbyte)(byte)_rawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out byte value)
    {
        if (_header is { Sign: IntegerSign.Unsigned, Width: IntegerWidth.W8 })
        {
            value = unchecked((byte)_rawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out short value)
    {
        if (_header is { Sign: IntegerSign.Signed, Width: IntegerWidth.W16 })
        {
            value = unchecked((short)(ushort)_rawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out ushort value)
    {
        if (_header is { Sign: IntegerSign.Unsigned, Width: IntegerWidth.W16 })
        {
            value = unchecked((ushort)_rawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out int value)
    {
        if (_header is { Sign: IntegerSign.Signed, Width: IntegerWidth.W32 })
        {
            value = unchecked((int)(uint)_rawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out uint value)
    {
        if (_header is { Sign: IntegerSign.Unsigned, Width: IntegerWidth.W32 })
        {
            value = unchecked((uint)_rawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out long value)
    {
        if (_header is { Sign: IntegerSign.Signed, Width: IntegerWidth.W64 })
        {
            value = unchecked((long)(ulong)_rawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out ulong value)
    {
        if (_header is { Sign: IntegerSign.Unsigned, Width: IntegerWidth.W64 })
        {
            value = unchecked((ulong)_rawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out Int128 value)
    {
        if (_header is { Sign: IntegerSign.Signed, Width: IntegerWidth.W128 })
        {
            value = unchecked((Int128)_rawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out UInt128 value)
    {
        if (_header is { Sign: IntegerSign.Unsigned, Width: IntegerWidth.W128 })
        {
            value = _rawValue;
            return true;
        }

        value = 0;
        return false;
    }

    static void IArchivable<IntegerConstant>.Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in IntegerConstant value
    )
    {
        switch (value._header.Width)
        {
            case IntegerWidth.W8:
                writer.WriteBlittable(value._header, unchecked((byte)value._rawValue));
                break;
            case IntegerWidth.W16:
                writer.WriteBlittable(value._header, unchecked((ushort)value._rawValue));
                break;
            case IntegerWidth.W32:
                writer.WriteBlittable(value._header, unchecked((uint)value._rawValue));
                break;
            case IntegerWidth.W64:
                writer.WriteBlittable(value._header, unchecked((ulong)value._rawValue));
                break;
            case IntegerWidth.W128:
                writer.WriteBlittable(value._header, value._rawValue);
                break;
            default:
                throw new ArchiveSerializationException("Unknown integer width");
        }
    }

    static void IArchivable<IntegerConstant>.Deserialize(
        ref ArchiveReader reader,
        scoped ref IntegerConstant value
    )
    {
        var header = reader.ReadBlittable<IntegerHeader>();
        var readValue = header.Width switch
        {
            IntegerWidth.W8 => reader.ReadBlittable<byte>(),
            IntegerWidth.W16 => reader.ReadBlittable<ushort>(),
            IntegerWidth.W32 => reader.ReadBlittable<uint>(),
            IntegerWidth.W64 => reader.ReadBlittable<ulong>(),
            IntegerWidth.W128 => reader.ReadBlittable<UInt128>(),
            _ => throw new ArchiveSerializationException("Unknown integer width"),
        };

        value = new IntegerConstant(header, readValue);
    }
}
