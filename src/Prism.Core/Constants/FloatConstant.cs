// @file FloatConstant.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Runtime.CompilerServices;
using MagicArchive;

namespace Prism.Core.Constants;

public enum FloatKind : byte
{
    F32,
    F64,
}

public static class FloatKindExtensions
{
    extension(FloatKind)
    {
        public static FloatKind FindCommon(FloatKind a, FloatKind b)
        {
            if (a == FloatKind.F64 || b == FloatKind.F64)
                return FloatKind.F64;

            return FloatKind.F32;
        }
    }
}

[Archivable(GenerateType.Custom)]
[Union]
public readonly partial record struct FloatConstant : IUnion
{
    internal FloatKind Kind { get; }
    internal double RawValue { get; }

    private FloatConstant(FloatKind kind, double value)
    {
        Kind = kind;
        RawValue = value;
    }

    internal static FloatConstant From(FloatKind kind, double value) => new(kind, value);

    public FloatConstant(float value)
        : this(FloatKind.F32, value) { }

    public FloatConstant(double value)
        : this(FloatKind.F64, value) { }

    public object Value
    {
        get
        {
            return Kind switch
            {
                FloatKind.F32 => (float)RawValue,
                FloatKind.F64 => RawValue,
                _ => throw new InvalidOperationException("Invalid float kind"),
            };
        }
    }

    public bool TryGetValue(out float value)
    {
        if (Kind == FloatKind.F32)
        {
            value = (float)RawValue;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out double value)
    {
        if (Kind == FloatKind.F64)
        {
            value = RawValue;
            return true;
        }

        value = 0;
        return false;
    }

    public static FloatConstant operator -(FloatConstant value)
    {
        return new FloatConstant(value.Kind, -value.RawValue);
    }

    public static FloatConstant operator +(FloatConstant a, FloatConstant b)
    {
        return a.BinaryOp(b, static (a, b) => a + b);
    }

    public static FloatConstant operator -(FloatConstant a, FloatConstant b)
    {
        return a.BinaryOp(b, static (a, b) => a - b);
    }

    public static FloatConstant operator *(FloatConstant a, FloatConstant b)
    {
        return a.BinaryOp(b, static (a, b) => a * b);
    }

    public static FloatConstant operator /(FloatConstant a, FloatConstant b)
    {
        return a.BinaryOp(b, static (a, b) => a / b);
    }

    public static FloatConstant operator %(FloatConstant a, FloatConstant b)
    {
        return a.BinaryOp(b, static (a, b) => a % b);
    }

    private FloatConstant BinaryOp(FloatConstant other, Func<double, double, double> operation)
    {
        var common = FloatKind.FindCommon(Kind, other.Kind);
        return new FloatConstant(
            common,
            TruncateIfNecessary(operation(RawValue, other.RawValue), common)
        );
    }

    private static double TruncateIfNecessary(double value, FloatKind kind)
    {
        return kind switch
        {
            FloatKind.F32 => (float)value,
            FloatKind.F64 => value,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    public FloatConstant ConvertTo(FloatKind kind)
    {
        return kind switch
        {
            FloatKind.F32 => (float)RawValue,
            FloatKind.F64 => RawValue,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    static void IArchivable<FloatConstant>.Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in FloatConstant value
    )
    {
        SerializeWithHeader(ref writer, value.Kind, value.RawValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SerializeWithHeader<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        FloatKind kind,
        double value
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        SerializeWithHeader(ref writer, (byte)kind, kind, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SerializeWithHeader<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        byte header,
        FloatKind kind,
        double value
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        switch (kind)
        {
            case FloatKind.F32:
                writer.WriteBlittable(header, (float)value);
                break;
            case FloatKind.F64:
                writer.WriteBlittable(header, value);
                break;
            default:
                throw new ArchiveSerializationException("Invalid float kind");
        }
    }

    static void IArchivable<FloatConstant>.Deserialize(
        ref ArchiveReader reader,
        scoped ref FloatConstant value
    )
    {
        var kind = reader.ReadBlittable<FloatKind>();
        value = new FloatConstant(kind, ReadValue(ref reader, kind));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double ReadValue(ref ArchiveReader reader, FloatKind kind)
    {
        return kind switch
        {
            FloatKind.F32 => reader.ReadBlittable<float>(),
            FloatKind.F64 => reader.ReadBlittable<double>(),
            _ => throw new ArchiveSerializationException("Invalid float kind"),
        };
    }
}
