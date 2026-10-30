// @file FloatConstant.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
    private readonly Payload _rawValue;

    [ExtendedLayout(ExtendedLayoutKind.CUnion)]
    private struct Payload
    {
        public float F32;
        public double F64;
    }

    public FloatConstant(float value)
    {
        Kind = FloatKind.F32;
        _rawValue.F32 = value;
    }

    public FloatConstant(double value)
    {
        Kind = FloatKind.F64;
        _rawValue.F64 = value;
    }

    public object Value
    {
        get
        {
            return Kind switch
            {
                FloatKind.F32 => _rawValue.F32,
                FloatKind.F64 => _rawValue.F64,
                _ => throw new InvalidOperationException("Invalid float kind"),
            };
        }
    }

    public bool TryGetValue(out float value)
    {
        if (Kind == FloatKind.F32)
        {
            value = _rawValue.F32;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out double value)
    {
        if (Kind == FloatKind.F64)
        {
            value = _rawValue.F64;
            return true;
        }

        value = 0;
        return false;
    }

    public static FloatConstant operator -(FloatConstant value)
    {
        return value.Kind switch
        {
            FloatKind.F32 => -value._rawValue.F32,
            FloatKind.F64 => -value._rawValue.F64,
            _ => throw new InvalidOperationException("Invalid float kind"),
        };
    }

    public static FloatConstant operator +(FloatConstant a, FloatConstant b)
    {
        return a.BinaryOp(b, BinaryOperation.Add);
    }

    public static FloatConstant operator -(FloatConstant a, FloatConstant b)
    {
        return a.BinaryOp(b, BinaryOperation.Subtract);
    }

    public static FloatConstant operator *(FloatConstant a, FloatConstant b)
    {
        return a.BinaryOp(b, BinaryOperation.Multiply);
    }

    public static FloatConstant operator /(FloatConstant a, FloatConstant b)
    {
        return a.BinaryOp(b, BinaryOperation.Divide);
    }

    public static FloatConstant operator %(FloatConstant a, FloatConstant b)
    {
        return a.BinaryOp(b, BinaryOperation.Modulo);
    }

    private enum BinaryOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
    }

    private FloatConstant BinaryOp(FloatConstant other, BinaryOperation operation)
    {
        var common = FloatKind.FindCommon(Kind, other.Kind);

        var commonThis = ConvertTo(common);
        var commonOther = other.ConvertTo(common);

        return common switch
        {
            FloatKind.F32 => BinaryOp(
                commonThis._rawValue.F32,
                commonOther._rawValue.F64,
                operation
            ),
            FloatKind.F64 => BinaryOp(
                commonThis._rawValue.F64,
                commonOther._rawValue.F64,
                operation
            ),
            _ => throw new InvalidOperationException("Invalid float kind"),
        };
    }

    private static TFloat BinaryOp<TFloat>(TFloat lhs, TFloat rhs, BinaryOperation operation)
        where TFloat : unmanaged, IFloatingPointIeee754<TFloat>
    {
        return operation switch
        {
            BinaryOperation.Add => lhs + rhs,
            BinaryOperation.Subtract => lhs - rhs,
            BinaryOperation.Multiply => lhs * rhs,
            BinaryOperation.Divide => lhs / rhs,
            BinaryOperation.Modulo => lhs % rhs,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
        };
    }

    public bool Equivalent(FloatConstant other)
    {
        return Compare(other, ComparisonOperation.Equal);
    }

    public bool NotEquivalent(FloatConstant other)
    {
        return Compare(other, ComparisonOperation.NotEqual);
    }

    public bool LessThan(FloatConstant other)
    {
        return Compare(other, ComparisonOperation.LessThan);
    }

    public bool LessThanOrEqual(FloatConstant other)
    {
        return Compare(other, ComparisonOperation.LessThanOrEqual);
    }

    public bool GreaterThan(FloatConstant other)
    {
        return Compare(other, ComparisonOperation.GreaterThan);
    }

    public bool GreaterThanOrEqual(FloatConstant other)
    {
        return Compare(other, ComparisonOperation.GreaterThanOrEqual);
    }

    private bool Compare(FloatConstant other, ComparisonOperation operation)
    {
        var common = FloatKind.FindCommon(Kind, other.Kind);

        var commonThis = ConvertTo(common);
        var commonOther = other.ConvertTo(common);

        return common switch
        {
            FloatKind.F32 => ComparisonHelpers.CompareValues(
                commonThis._rawValue.F32,
                commonOther._rawValue.F64,
                operation
            ),
            FloatKind.F64 => ComparisonHelpers.CompareValues(
                commonThis._rawValue.F64,
                commonOther._rawValue.F64,
                operation
            ),
            _ => throw new InvalidOperationException("Invalid float kind"),
        };
    }

    public IntegerConstant ConvertTo(IntegerKind kind)
    {
        return this switch
        {
            float f => ConvertTo(f, kind),
            double d => ConvertTo(d, kind),
        };
    }

    private static IntegerConstant ConvertTo<TFloat>(TFloat value, IntegerKind kind)
        where TFloat : unmanaged, IFloatingPointIeee754<TFloat>
    {
        return (kind.Sign, kind.Width) switch
        {
            (IntegerSign.Signed, IntegerWidth.W8) => sbyte.CreateTruncating(value),
            (IntegerSign.Signed, IntegerWidth.W16) => short.CreateTruncating(value),
            (IntegerSign.Signed, IntegerWidth.W32) => int.CreateTruncating(value),
            (IntegerSign.Signed, IntegerWidth.W64) => long.CreateTruncating(value),
            (IntegerSign.Signed, IntegerWidth.W128) => Int128.CreateTruncating(value),
            (IntegerSign.Unsigned, IntegerWidth.W8) => byte.CreateTruncating(value),
            (IntegerSign.Unsigned, IntegerWidth.W16) => ushort.CreateTruncating(value),
            (IntegerSign.Unsigned, IntegerWidth.W32) => uint.CreateTruncating(value),
            (IntegerSign.Unsigned, IntegerWidth.W64) => ulong.CreateTruncating(value),
            (IntegerSign.Unsigned, IntegerWidth.W128) => UInt128.CreateTruncating(value),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    public FloatConstant ConvertTo(FloatKind kind)
    {
        if (Kind == kind)
            return this;

        return kind switch
        {
            FloatKind.F32 => (float)_rawValue.F64,
            FloatKind.F64 => (double)_rawValue.F32,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    static void IArchivable<FloatConstant>.Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in FloatConstant value
    )
    {
        switch (value.Kind)
        {
            case FloatKind.F32:
                SerializeWithHeader(ref writer, FloatKind.F32, value._rawValue.F32);
                break;
            case FloatKind.F64:
                SerializeWithHeader(ref writer, FloatKind.F32, value._rawValue.F64);
                break;
            default:
                throw new ArchiveSerializationException("Invalid float kind");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SerializeWithHeader<TBufferWriter, TFloat>(
        ref ArchiveWriter<TBufferWriter> writer,
        FloatKind kind,
        TFloat value
    )
        where TBufferWriter : IBufferWriter<byte>
        where TFloat : unmanaged, IFloatingPointIeee754<TFloat>
    {
        SerializeWithHeader(ref writer, (byte)kind, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SerializeWithHeader<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        byte header
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        switch (this)
        {
            case float f:
                SerializeWithHeader(ref writer, header, f);
                break;
            case double d:
                SerializeWithHeader(ref writer, header, d);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SerializeWithHeader<TBufferWriter, TFloat>(
        ref ArchiveWriter<TBufferWriter> writer,
        byte header,
        TFloat value
    )
        where TBufferWriter : IBufferWriter<byte>
        where TFloat : unmanaged, IFloatingPointIeee754<TFloat>
    {
        writer.WriteBlittable(header, value);
    }

    static void IArchivable<FloatConstant>.Deserialize(
        ref ArchiveReader reader,
        scoped ref FloatConstant value
    )
    {
        var kind = reader.ReadBlittable<FloatKind>();
        value = ReadValue(ref reader, kind);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static FloatConstant ReadValue(ref ArchiveReader reader, FloatKind kind)
    {
        return kind switch
        {
            FloatKind.F32 => reader.ReadBlittable<float>(),
            FloatKind.F64 => reader.ReadBlittable<double>(),
            _ => throw new ArchiveSerializationException("Invalid float kind"),
        };
    }

    public override string ToString()
    {
        return this switch
        {
            float v => v.ToString(CultureInfo.InvariantCulture),
            double v => v.ToString(CultureInfo.InvariantCulture),
        };
    }
}
