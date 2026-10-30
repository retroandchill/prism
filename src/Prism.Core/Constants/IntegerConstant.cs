// @file IntegerConstant.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MagicArchive;

namespace Prism.Core.Constants;

public enum IntegerWidth : byte
{
    W8,
    W16,
    W32,
    W64,
    W128,
}

public enum IntegerSign : byte
{
    Unsigned,
    Signed,
}

[Archivable]
public readonly partial record struct IntegerKind
{
    [ArchiveInclude]
    private readonly byte _value;

    internal const byte WidthMask = 0b0000_0111;
    internal const byte SignMask = 0b0000_1000;
    internal const byte SignShift = 3;

    public static IntegerKind I8 => Create(IntegerWidth.W8, IntegerSign.Signed);
    public static IntegerKind I16 => Create(IntegerWidth.W16, IntegerSign.Signed);
    public static IntegerKind I32 => Create(IntegerWidth.W32, IntegerSign.Signed);
    public static IntegerKind I64 => Create(IntegerWidth.W64, IntegerSign.Signed);
    public static IntegerKind I128 => Create(IntegerWidth.W128, IntegerSign.Signed);

    public static IntegerKind U8 => Create(IntegerWidth.W8, IntegerSign.Signed);
    public static IntegerKind U16 => Create(IntegerWidth.W16, IntegerSign.Unsigned);
    public static IntegerKind U32 => Create(IntegerWidth.W32, IntegerSign.Unsigned);
    public static IntegerKind U64 => Create(IntegerWidth.W64, IntegerSign.Unsigned);
    public static IntegerKind U128 => Create(IntegerWidth.W128, IntegerSign.Unsigned);

    private IntegerKind(byte value)
    {
        _value = value;
    }

    public static IntegerKind Create(IntegerWidth width, IntegerSign sign)
    {
        return new IntegerKind((byte)((byte)width | (byte)((byte)sign << SignShift)));
    }

    internal static IntegerKind FromRaw(byte value) => new(value);

    internal byte ToRaw() => _value;

    [ArchiveIgnore]
    public IntegerWidth Width => (IntegerWidth)(_value & WidthMask);

    [ArchiveIgnore]
    public IntegerSign Sign => (IntegerSign)((_value & SignMask) >> SignShift);

    public static IntegerKind? TryFindCommon(IntegerKind left, IntegerKind right)
    {
        if (left.Sign == right.Sign)
        {
            return left.Width > right.Width ? left : right;
        }

        var largerWidth = left.Width > right.Width ? left.Width : right.Width;
        if (largerWidth == IntegerWidth.W128)
            return null;

        return Create(largerWidth + 1, IntegerSign.Signed);
    }
}

[Archivable(GenerateType.Custom)]
[Union]
public readonly partial record struct IntegerConstant : IUnion
{
    internal IntegerKind Kind { get; }
    internal UInt128 RawValue { get; }

    private IntegerConstant(IntegerKind kind, UInt128 value)
    {
        Kind = kind;
        RawValue = value;
    }

    internal static IntegerConstant From(IntegerKind kind, UInt128 value) => new(kind, value);

    public IntegerConstant(sbyte value)
    {
        Kind = IntegerKind.Create(IntegerWidth.W8, IntegerSign.Signed);
        RawValue = unchecked((byte)value);
    }

    public IntegerConstant(byte value)
    {
        Kind = IntegerKind.Create(IntegerWidth.W8, IntegerSign.Unsigned);
        RawValue = value;
    }

    public IntegerConstant(short value)
    {
        Kind = IntegerKind.Create(IntegerWidth.W16, IntegerSign.Signed);
        RawValue = unchecked((ushort)value);
    }

    public IntegerConstant(ushort value)
    {
        Kind = IntegerKind.Create(IntegerWidth.W16, IntegerSign.Unsigned);
        RawValue = value;
    }

    public IntegerConstant(int value)
    {
        Kind = IntegerKind.Create(IntegerWidth.W32, IntegerSign.Signed);
        RawValue = unchecked((uint)value);
    }

    public IntegerConstant(uint value)
    {
        Kind = IntegerKind.Create(IntegerWidth.W32, IntegerSign.Unsigned);
        RawValue = value;
    }

    public IntegerConstant(long value)
    {
        Kind = IntegerKind.Create(IntegerWidth.W64, IntegerSign.Signed);
        RawValue = unchecked((ulong)value);
    }

    public IntegerConstant(ulong value)
    {
        Kind = IntegerKind.Create(IntegerWidth.W64, IntegerSign.Unsigned);
        RawValue = value;
    }

    public IntegerConstant(Int128 value)
    {
        Kind = IntegerKind.Create(IntegerWidth.W128, IntegerSign.Signed);
        RawValue = unchecked((UInt128)value);
    }

    public IntegerConstant(UInt128 value)
    {
        Kind = IntegerKind.Create(IntegerWidth.W128, IntegerSign.Unsigned);
        RawValue = value;
    }

    public object Value
    {
        get
        {
            return Kind.Sign switch
            {
                IntegerSign.Unsigned => Kind.Width switch
                {
                    IntegerWidth.W8 => (byte)RawValue,
                    IntegerWidth.W16 => (ushort)RawValue,
                    IntegerWidth.W32 => (uint)RawValue,
                    IntegerWidth.W64 => (ulong)RawValue,
                    IntegerWidth.W128 => RawValue,
                    _ => throw new ArgumentOutOfRangeException(),
                },
                IntegerSign.Signed => Kind.Width switch
                {
                    IntegerWidth.W8 => unchecked((sbyte)(byte)RawValue),
                    IntegerWidth.W16 => unchecked((short)(ushort)RawValue),
                    IntegerWidth.W32 => unchecked((int)(uint)RawValue),
                    IntegerWidth.W64 => unchecked((long)(ulong)RawValue),
                    IntegerWidth.W128 => unchecked((Int128)RawValue),
                    _ => throw new ArgumentOutOfRangeException(),
                },
                _ => throw new InvalidOperationException("Invalid sign"),
            };
        }
    }

    public bool TryGetValue(out sbyte value)
    {
        if (Kind is { Sign: IntegerSign.Signed, Width: IntegerWidth.W8 })
        {
            value = unchecked((sbyte)(byte)RawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out byte value)
    {
        if (Kind is { Sign: IntegerSign.Unsigned, Width: IntegerWidth.W8 })
        {
            value = unchecked((byte)RawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out short value)
    {
        if (Kind is { Sign: IntegerSign.Signed, Width: IntegerWidth.W16 })
        {
            value = unchecked((short)(ushort)RawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out ushort value)
    {
        if (Kind is { Sign: IntegerSign.Unsigned, Width: IntegerWidth.W16 })
        {
            value = unchecked((ushort)RawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out int value)
    {
        if (Kind is { Sign: IntegerSign.Signed, Width: IntegerWidth.W32 })
        {
            value = unchecked((int)(uint)RawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out uint value)
    {
        if (Kind is { Sign: IntegerSign.Unsigned, Width: IntegerWidth.W32 })
        {
            value = unchecked((uint)RawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out long value)
    {
        if (Kind is { Sign: IntegerSign.Signed, Width: IntegerWidth.W64 })
        {
            value = unchecked((long)(ulong)RawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out ulong value)
    {
        if (Kind is { Sign: IntegerSign.Unsigned, Width: IntegerWidth.W64 })
        {
            value = unchecked((ulong)RawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out Int128 value)
    {
        if (Kind is { Sign: IntegerSign.Signed, Width: IntegerWidth.W128 })
        {
            value = unchecked((Int128)RawValue);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out UInt128 value)
    {
        if (Kind is { Sign: IntegerSign.Unsigned, Width: IntegerWidth.W128 })
        {
            value = RawValue;
            return true;
        }

        value = 0;
        return false;
    }

    private static UInt128 Truncate(IntegerWidth width, UInt128 value)
    {
        return width switch
        {
            IntegerWidth.W8 => value & byte.MaxValue,
            IntegerWidth.W16 => value & ushort.MaxValue,
            IntegerWidth.W32 => value & uint.MaxValue,
            IntegerWidth.W64 => value & ulong.MaxValue,
            IntegerWidth.W128 => value,
            _ => throw new ArgumentOutOfRangeException(nameof(width), width, null),
        };
    }

    public IntegerConstant? TryNegate()
    {
        IntegerWidth width;
        switch (Kind.Sign)
        {
            case IntegerSign.Unsigned:
                if (Kind.Width == IntegerWidth.W128)
                    return null;

                width = Kind.Width + 1;
                break;
            case IntegerSign.Signed:
                width = Kind.Width;
                break;
            default:
                throw new InvalidOperationException("Unknown sign");
        }

        return new IntegerConstant(
            IntegerKind.Create(width, IntegerSign.Signed),
            Truncate(width, unchecked(~RawValue + 1))
        );
    }

    public static IntegerConstant operator ~(IntegerConstant value)
    {
        return new IntegerConstant(value.Kind, Truncate(value.Kind.Width, ~value.RawValue));
    }

    public IntegerConstant? TryAdd(IntegerConstant other)
    {
        if (IntegerKind.TryFindCommon(Kind, other.Kind) is not { } commonKind)
            return null;

        throw new NotImplementedException();
    }

    public IntegerConstant ConvertTo(IntegerKind kind)
    {
        if (Kind == kind)
            return this;

        return kind.Sign switch
        {
            IntegerSign.Unsigned => new IntegerConstant(kind, Truncate(kind.Width, RawValue)),
            IntegerSign.Signed => SignedConvert(kind),
            _ => throw new InvalidOperationException("Unknown sign"),
        };
    }

    private IntegerConstant SignedConvert(IntegerKind kind)
    {
        Debug.Assert(kind.Sign == Kind.Sign);
        if (Kind.Width > kind.Width)
        {
            return new IntegerConstant(kind, Truncate(kind.Width, RawValue));
        }

        return Kind.Sign switch
        {
            IntegerSign.Unsigned => new IntegerConstant(kind, Truncate(kind.Width, RawValue)),
            IntegerSign.Signed => new IntegerConstant(kind, ComputeSignExtendedValue(kind.Width)),
            _ => throw new InvalidOperationException("Unknown sign"),
        };
    }

    private UInt128 ComputeSignExtendedValue(IntegerWidth width)
    {
        var signBit = GetSignBit(Kind.Width);
        if ((RawValue & signBit) == 0)
        {
            return RawValue;
        }

        var mask = ~GetSignExtendMask(Kind.Width);
        return Truncate(width, RawValue | mask);
    }

    private static UInt128 GetSignExtendMask(IntegerWidth width)
    {
        return width switch
        {
            IntegerWidth.W8 => byte.MaxValue,
            IntegerWidth.W16 => ushort.MaxValue,
            IntegerWidth.W32 => uint.MaxValue,
            IntegerWidth.W64 => ulong.MaxValue,
            IntegerWidth.W128 => UInt128.MaxValue,
            _ => throw new ArgumentOutOfRangeException(nameof(width), width, null),
        };
    }

    private static UInt128 GetSignBit(IntegerWidth width)
    {
        return width switch
        {
            IntegerWidth.W8 => (UInt128)1 << 7,
            IntegerWidth.W16 => (UInt128)1 << 15,
            IntegerWidth.W32 => (UInt128)1 << 31,
            IntegerWidth.W64 => (UInt128)1 << 63,
            IntegerWidth.W128 => (UInt128)1 << 127,
            _ => throw new ArgumentOutOfRangeException(nameof(width), width, null),
        };
    }

    static void IArchivable<IntegerConstant>.Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in IntegerConstant value
    )
    {
        SerializeWithHeader(ref writer, value.Kind, value.RawValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SerializeWithHeader<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        IntegerKind kind,
        UInt128 value
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        SerializeWithHeader(ref writer, kind.ToRaw(), kind, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SerializeWithHeader<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        byte header,
        IntegerKind kind,
        UInt128 value
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        switch (kind.Width)
        {
            case IntegerWidth.W8:
                writer.WriteBlittable(header, unchecked((byte)value));
                break;
            case IntegerWidth.W16:
                writer.WriteBlittable(header, unchecked((ushort)value));
                break;
            case IntegerWidth.W32:
                writer.WriteBlittable(header, unchecked((uint)value));
                break;
            case IntegerWidth.W64:
                writer.WriteBlittable(header, unchecked((ulong)value));
                break;
            case IntegerWidth.W128:
                writer.WriteBlittable(header, value);
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
        var header = reader.ReadBlittable<IntegerKind>();
        value = new IntegerConstant(header, ReadValue(ref reader, header));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static UInt128 ReadValue(ref ArchiveReader reader, IntegerKind kind)
    {
        return kind.Width switch
        {
            IntegerWidth.W8 => reader.ReadBlittable<byte>(),
            IntegerWidth.W16 => reader.ReadBlittable<ushort>(),
            IntegerWidth.W32 => reader.ReadBlittable<uint>(),
            IntegerWidth.W64 => reader.ReadBlittable<ulong>(),
            IntegerWidth.W128 => reader.ReadBlittable<UInt128>(),
            _ => throw new ArchiveSerializationException("Unknown integer width"),
        };
    }
}
