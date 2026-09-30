using System.Collections.Immutable;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Prism.Core.Configuration;
using Prism.Core.Mappers;
using Prism.Core.Symbols;

namespace Prism.Core.Semantic;

public enum ConstantKind : byte
{
    Null,
    Primitive,
    Array,
}

public enum PrimitiveKind : byte
{
    Bool,
    Char,
    Char16,
    Rune,
    I8,
    I16,
    I32,
    I64,
    I128,
    ISize,
    U8,
    U16,
    U32,
    U64,
    U128,
    USize,
    F32,
    F64,
    Str,
}

public readonly struct ConstantValue
{
    [StructLayout(LayoutKind.Explicit)]
    private struct BlittableStorage
    {
        [field: FieldOffset(0)]
        public bool BoolValue { get; init; }

        [field: FieldOffset(0)]
        public Rune CharacterValue { get; init; }

        [field: FieldOffset(0)]
        public long I64Value { get; init; }

        [field: FieldOffset(0)]
        public ulong U64Value { get; init; }

        [field: FieldOffset(0)]
        public Int128 I128Value { get; init; }

        [field: FieldOffset(0)]
        public UInt128 U128Value { get; init; }

        [field: FieldOffset(0)]
        public float F32Value { get; init; }

        [field: FieldOffset(0)]
        public double F64Value { get; init; }
    }

    public ConstantKind Kind { get; }

    public PrimitiveKind PrimitiveKind
    {
        get =>
            Kind == ConstantKind.Primitive
                ? field
                : throw new InvalidOperationException("ConstantValue is not primitive");
        private init;
    }

    private readonly object? _referenceValue;
    private readonly BlittableStorage _blittableStorage;

    private ConstantValue(PrimitiveKind primitiveKind, BlittableStorage blittableStorage)
    {
        Kind = ConstantKind.Primitive;
        PrimitiveKind = primitiveKind;
        _blittableStorage = blittableStorage;
    }

    private ConstantValue(string referenceValue)
    {
        Kind = ConstantKind.Primitive;
        PrimitiveKind = PrimitiveKind.Str;
        _referenceValue = referenceValue;
    }

    private ConstantValue(ImmutableArray<ConstantValue> elements)
    {
        Kind = ConstantKind.Array;
        _referenceValue = ImmutableCollectionsMarshal.AsArray(elements);
    }

    public static ConstantValue Null() => default;

    public static ConstantValue Boolean(bool value)
    {
        return new ConstantValue(PrimitiveKind.Bool, new BlittableStorage { BoolValue = value });
    }

    public static ConstantValue Character(byte value)
    {
        return new ConstantValue(
            PrimitiveKind.Char,
            new BlittableStorage { CharacterValue = new Rune(value) }
        );
    }

    public static ConstantValue Character16(char value)
    {
        return new ConstantValue(
            PrimitiveKind.Char16,
            new BlittableStorage { CharacterValue = new Rune(value) }
        );
    }

    public static ConstantValue Rune(Rune value)
    {
        return new ConstantValue(
            PrimitiveKind.Rune,
            new BlittableStorage { CharacterValue = value }
        );
    }

    public static ConstantValue I8(sbyte value)
    {
        return new ConstantValue(PrimitiveKind.I8, new BlittableStorage { I64Value = value });
    }

    public static ConstantValue I16(short value)
    {
        return new ConstantValue(PrimitiveKind.I16, new BlittableStorage { I64Value = value });
    }

    public static ConstantValue I32(int value)
    {
        return new ConstantValue(PrimitiveKind.I32, new BlittableStorage { I64Value = value });
    }

    public static ConstantValue I64(long value)
    {
        return new ConstantValue(PrimitiveKind.I64, new BlittableStorage { I64Value = value });
    }

    public static ConstantValue I128(Int128 value)
    {
        return new ConstantValue(PrimitiveKind.I128, new BlittableStorage { I128Value = value });
    }

    public static ConstantValue ISize(long value)
    {
        return new ConstantValue(PrimitiveKind.ISize, new BlittableStorage { I64Value = value });
    }

    public static ConstantValue U8(byte value)
    {
        return new ConstantValue(PrimitiveKind.U8, new BlittableStorage { U64Value = value });
    }

    public static ConstantValue U16(ushort value)
    {
        return new ConstantValue(PrimitiveKind.U16, new BlittableStorage { U64Value = value });
    }

    public static ConstantValue U32(uint value)
    {
        return new ConstantValue(PrimitiveKind.U32, new BlittableStorage { U64Value = value });
    }

    public static ConstantValue U64(ulong value)
    {
        return new ConstantValue(PrimitiveKind.U64, new BlittableStorage { U64Value = value });
    }

    public static ConstantValue U128(UInt128 value)
    {
        return new ConstantValue(PrimitiveKind.U128, new BlittableStorage { U128Value = value });
    }

    public static ConstantValue USize(ulong value)
    {
        return new ConstantValue(PrimitiveKind.USize, new BlittableStorage { U64Value = value });
    }

    public static ConstantValue F32(float value)
    {
        return new ConstantValue(PrimitiveKind.F32, new BlittableStorage { F32Value = value });
    }

    public static ConstantValue F64(double value)
    {
        return new ConstantValue(PrimitiveKind.F64, new BlittableStorage { F64Value = value });
    }

    public static ConstantValue Str(string value)
    {
        return new ConstantValue(value);
    }

    public bool IsNull => Kind == ConstantKind.Null;

    public SpecialType SpecialType => PrimitiveKind.ToSpecialType();

    public bool IsNumeric => IsSignedInteger || IsUnsignedInteger || IsFloat;

    public bool IsSignedInteger =>
        PrimitiveKind
            is PrimitiveKind.I8
                or PrimitiveKind.I16
                or PrimitiveKind.I32
                or PrimitiveKind.I64
                or PrimitiveKind.I128
                or PrimitiveKind.ISize;

    public bool IsUnsignedInteger =>
        PrimitiveKind
            is PrimitiveKind.U8
                or PrimitiveKind.U16
                or PrimitiveKind.U32
                or PrimitiveKind.U64
                or PrimitiveKind.U128
                or PrimitiveKind.USize;

    public bool IsFloat => PrimitiveKind is PrimitiveKind.F32 or PrimitiveKind.F64;

    public bool CanBeNegative => IsSignedInteger || IsFloat;

    public bool IsCharacter =>
        PrimitiveKind is PrimitiveKind.Char or PrimitiveKind.Char16 or PrimitiveKind.Rune;

    public bool IsString => PrimitiveKind == PrimitiveKind.Str;

    private void ThrowIfNotValidType(bool condition)
    {
        if (!condition)
        {
            throw new InvalidOperationException("Invalid type");
        }
    }

    public bool AsBoolean()
    {
        ThrowIfNotValidType(PrimitiveKind == PrimitiveKind.Bool);
        return _blittableStorage.BoolValue;
    }

    public Rune AsCharacter()
    {
        ThrowIfNotValidType(IsCharacter);
        return _blittableStorage.CharacterValue;
    }

    public long AsInt64()
    {
        ThrowIfNotValidType(PrimitiveKind != PrimitiveKind.I128 && IsSignedInteger);
        return _blittableStorage.I64Value;
    }

    public Int128 AsInt128()
    {
        ThrowIfNotValidType(PrimitiveKind == PrimitiveKind.I128);
        return _blittableStorage.I128Value;
    }

    public ulong AsUInt64()
    {
        ThrowIfNotValidType(PrimitiveKind != PrimitiveKind.U128 && IsUnsignedInteger);
        return _blittableStorage.U64Value;
    }

    public UInt128 AsUInt128()
    {
        ThrowIfNotValidType(PrimitiveKind == PrimitiveKind.U128);
        return _blittableStorage.U128Value;
    }

    public float AsFloat32()
    {
        ThrowIfNotValidType(PrimitiveKind == PrimitiveKind.F32);
        return _blittableStorage.F32Value;
    }

    public double AsFloat64()
    {
        ThrowIfNotValidType(PrimitiveKind == PrimitiveKind.F64);
        return _blittableStorage.F64Value;
    }

    public string AsString()
    {
        ThrowIfNotValidType(PrimitiveKind == PrimitiveKind.Str);
        return (string)_referenceValue!;
    }

    public ImmutableArray<ConstantValue> AsArray()
    {
        ThrowIfNotValidType(Kind == ConstantKind.Array);
        return ImmutableCollectionsMarshal.AsImmutableArray((ConstantValue[]?)_referenceValue);
    }

    public ConstantValue? TryNegate(CompilationSettings settings)
    {
        switch (PrimitiveKind)
        {
            case PrimitiveKind.I8:
            case PrimitiveKind.I16:
            case PrimitiveKind.I32:
            case PrimitiveKind.I64:
            case PrimitiveKind.ISize:
                return new ConstantValue(
                    PrimitiveKind,
                    new BlittableStorage { I64Value = -_blittableStorage.I64Value }
                );
            case PrimitiveKind.I128:
                return new ConstantValue(
                    PrimitiveKind,
                    new BlittableStorage { I128Value = -_blittableStorage.I128Value }
                );
            case PrimitiveKind.U8:
            case PrimitiveKind.U16:
                return I32(unchecked(-(ushort)_blittableStorage.U64Value));
            case PrimitiveKind.U32:
                return I64(unchecked(-(uint)_blittableStorage.U64Value));
            case PrimitiveKind.U64:
                return I128(-(Int128)_blittableStorage.U64Value);
            case PrimitiveKind.USize:
                return settings.PointerWidth switch
                {
                    PointerWidth.X32 => I64(unchecked(-(uint)_blittableStorage.U64Value)),
                    PointerWidth.X64 => I128(unchecked(-(Int128)_blittableStorage.U64Value)),
                    _ => throw new InvalidOperationException("Invalid pointer width"),
                };
            case PrimitiveKind.F32:
                return F32(-_blittableStorage.F32Value);
            case PrimitiveKind.F64:
                return F64(-_blittableStorage.F64Value);
            case PrimitiveKind.Bool:
            case PrimitiveKind.Char:
            case PrimitiveKind.Char16:
            case PrimitiveKind.Rune:
            case PrimitiveKind.Str:
            case PrimitiveKind.U128:
            default:
                return null;
        }
    }

    public ConstantValue Negate(CompilationSettings settings)
    {
        return TryNegate(settings)
            ?? throw new InvalidOperationException("Cannot negate constant value");
    }

    public ConstantValue? TryLogicalNot()
    {
        return Kind == ConstantKind.Primitive && PrimitiveKind == PrimitiveKind.Bool
            ? Boolean(!AsBoolean())
            : null;
    }

    public ConstantValue? TryBitwiseNot(CompilationSettings settings)
    {
        if (Kind != ConstantKind.Primitive)
        {
            return null;
        }

        return PrimitiveKind switch
        {
            PrimitiveKind.I8 => I8((sbyte)~(sbyte)AsInt64()),
            PrimitiveKind.I16 => I16((short)~(short)AsInt64()),
            PrimitiveKind.I32 => I32(~(int)AsInt64()),
            PrimitiveKind.I64 => I64(~AsInt64()),
            PrimitiveKind.I128 => I128(~AsInt128()),
            PrimitiveKind.ISize => settings.PointerWidth switch
            {
                PointerWidth.X32 => ISize(~(int)AsInt64()),
                PointerWidth.X64 => ISize(~AsInt64()),
                _ => throw new InvalidOperationException("Invalid pointer width"),
            },
            PrimitiveKind.U8 => U8((byte)~(byte)AsUInt64()),
            PrimitiveKind.U16 => U16((ushort)~(ushort)AsUInt64()),
            PrimitiveKind.U32 => U32(~(uint)AsUInt64()),
            PrimitiveKind.U64 => U64(~AsUInt64()),
            PrimitiveKind.U128 => U128(~AsUInt128()),
            PrimitiveKind.USize => settings.PointerWidth switch
            {
                PointerWidth.X32 => USize(~(uint)AsUInt64()),
                PointerWidth.X64 => USize(~AsUInt64()),
                _ => throw new InvalidOperationException("Invalid pointer width"),
            },
            _ => null,
        };
    }

    public ConstantValue? TryBinary(
        BinaryOperation op,
        in ConstantValue right,
        CompilationSettings settings
    )
    {
        if (Kind != ConstantKind.Primitive)
            return null;

        if (PrimitiveKind != right.PrimitiveKind)
            return null;

        return PrimitiveKind switch
        {
            PrimitiveKind.I8 => EvalSigned(op, (sbyte)AsInt64(), (sbyte)right.AsInt64(), I8),
            PrimitiveKind.I16 => EvalSigned(op, (short)AsInt64(), (short)right.AsInt64(), I16),
            PrimitiveKind.I32 => EvalSigned(op, (int)AsInt64(), (int)right.AsInt64(), I32),
            PrimitiveKind.I64 => EvalSigned(op, AsInt64(), right.AsInt64(), I64),
            PrimitiveKind.I128 => EvalSigned(op, AsInt128(), right.AsInt128(), I128),

            PrimitiveKind.U8 => EvalUnsigned(op, (byte)AsUInt64(), (byte)right.AsUInt64(), U8),
            PrimitiveKind.U16 => EvalUnsigned(
                op,
                (ushort)AsUInt64(),
                (ushort)right.AsUInt64(),
                U16
            ),
            PrimitiveKind.U32 => EvalUnsigned(op, (uint)AsUInt64(), (uint)right.AsUInt64(), U32),
            PrimitiveKind.U64 => EvalUnsigned(op, AsUInt64(), right.AsUInt64(), U64),
            PrimitiveKind.U128 => EvalUnsigned(op, AsUInt128(), right.AsUInt128(), U128),

            PrimitiveKind.F32 => EvalFloat(op, AsFloat32(), right.AsFloat32(), F32),
            PrimitiveKind.F64 => EvalFloat(op, AsFloat64(), right.AsFloat64(), F64),

            PrimitiveKind.Bool => EvalBool(op, AsBoolean(), right.AsBoolean()),
            PrimitiveKind.ISize => settings.PointerWidth switch
            {
                PointerWidth.X32 => EvalSigned(op, (int)AsInt64(), (int)right.AsInt64(), i =>
                    ISize(i)
                ),
                PointerWidth.X64 => EvalSigned(op, AsInt64(), right.AsInt64(), ISize),
                _ => null,
            },
            PrimitiveKind.USize => settings.PointerWidth switch
            {
                PointerWidth.X32 => EvalUnsigned(op, (uint)AsUInt64(), (uint)right.AsUInt64(), i =>
                    USize(i)
                ),
                PointerWidth.X64 => EvalUnsigned(op, AsUInt64(), right.AsUInt64(), USize),
                _ => null,
            },

            _ => null,
        };
    }

    private static ConstantValue? EvalSigned<T>(
        BinaryOperation op,
        T left,
        T right,
        Func<T, ConstantValue> wrap
    )
        where T : IBinaryInteger<T>, ISignedNumber<T>
    {
        return op switch
        {
            BinaryOperation.Addition => wrap(left + right),
            BinaryOperation.Subtraction => wrap(left - right),
            BinaryOperation.Multiplication => wrap(left * right),
            BinaryOperation.Division => wrap(left / right),
            BinaryOperation.Modulo => wrap(left % right),
            BinaryOperation.BitwiseAnd => wrap(left & right),
            BinaryOperation.BitwiseOr => wrap(left | right),
            BinaryOperation.BitwiseXor => wrap(left ^ right),
            BinaryOperation.ShiftLeft => wrap(left << int.CreateChecked(right)),
            BinaryOperation.ShiftRight => wrap(left >> int.CreateChecked(right)),
            BinaryOperation.Equality => Boolean(left == right),
            BinaryOperation.NotEquals => Boolean(left != right),
            BinaryOperation.LessThan => Boolean(left < right),
            BinaryOperation.LessThanOrEquals => Boolean(left <= right),
            BinaryOperation.GreaterThan => Boolean(left > right),
            BinaryOperation.GreaterThanOrEquals => Boolean(left >= right),
            _ => null,
        };
    }

    private static ConstantValue? EvalUnsigned<T>(
        BinaryOperation op,
        T left,
        T right,
        Func<T, ConstantValue> wrap
    )
        where T : IBinaryInteger<T>, IUnsignedNumber<T>
    {
        return op switch
        {
            BinaryOperation.Addition => wrap(left + right),
            BinaryOperation.Subtraction => wrap(left - right),
            BinaryOperation.Multiplication => wrap(left * right),
            BinaryOperation.Division => wrap(left / right),
            BinaryOperation.Modulo => wrap(left % right),
            BinaryOperation.BitwiseAnd => wrap(left & right),
            BinaryOperation.BitwiseOr => wrap(left | right),
            BinaryOperation.BitwiseXor => wrap(left ^ right),
            BinaryOperation.ShiftLeft => wrap(left << int.CreateChecked(right)),
            BinaryOperation.ShiftRight => wrap(left >> int.CreateChecked(right)),
            BinaryOperation.Equality => Boolean(left == right),
            BinaryOperation.NotEquals => Boolean(left != right),
            BinaryOperation.LessThan => Boolean(left < right),
            BinaryOperation.LessThanOrEquals => Boolean(left <= right),
            BinaryOperation.GreaterThan => Boolean(left > right),
            BinaryOperation.GreaterThanOrEquals => Boolean(left >= right),
            _ => null,
        };
    }

    private static ConstantValue? EvalFloat<T>(
        BinaryOperation op,
        T left,
        T right,
        Func<T, ConstantValue> wrap
    )
        where T : IBinaryFloatingPointIeee754<T>
    {
        return op switch
        {
            BinaryOperation.Addition => wrap(left + right),
            BinaryOperation.Subtraction => wrap(left - right),
            BinaryOperation.Multiplication => wrap(left * right),
            BinaryOperation.Division => wrap(left / right),
            BinaryOperation.Modulo => wrap(left % right),
            BinaryOperation.Equality => Boolean(left == right),
            BinaryOperation.NotEquals => Boolean(left != right),
            BinaryOperation.LessThan => Boolean(left < right),
            BinaryOperation.LessThanOrEquals => Boolean(left <= right),
            BinaryOperation.GreaterThan => Boolean(left > right),
            BinaryOperation.GreaterThanOrEquals => Boolean(left >= right),
            _ => null,
        };
    }

    private static ConstantValue? EvalBool(BinaryOperation op, bool left, bool right)
    {
        return op switch
        {
            BinaryOperation.LogicalAnd => Boolean(left && right),
            BinaryOperation.LogicalOr => Boolean(left || right),
            BinaryOperation.Equality => Boolean(left == right),
            BinaryOperation.NotEquals => Boolean(left != right),
            _ => null,
        };
    }

    public ConstantValue? TryConvert(TypeSymbol typeSymbol, CompilationSettings settings)
    {
        while (true)
        {
            if (typeSymbol is NullableTypeSymbol nullableType)
            {
                if (IsNull)
                    return this;
                typeSymbol = nullableType.ElementType;
                continue;
            }

            if (IsNull)
            {
                return null;
            }

            if (Kind != ConstantKind.Primitive)
            {
                return null;
            }

            return typeSymbol.SpecialType switch
            {
                SpecialType.Bool => PrimitiveKind == PrimitiveKind.Bool ? this : null,
                SpecialType.Char => TryConvertToChar(),
                SpecialType.Char16 => TryConvertToChar16(),
                SpecialType.Rune => TryConvertToRune(),

                SpecialType.I8 => TryConvertToSigned(v => I8(unchecked((sbyte)v))),
                SpecialType.I16 => TryConvertToSigned(v => I16(unchecked((short)v))),
                SpecialType.I32 => TryConvertToSigned(v => I32(unchecked((int)v))),
                SpecialType.I64 => TryConvertToSigned(v => I64(unchecked((long)v))),
                SpecialType.I128 => TryConvertToSigned(I128),
                SpecialType.ISize => TryConvertToSigned(v => ISize(unchecked((long)v))),

                SpecialType.U8 => TryConvertToUnsigned(v => U8(unchecked((byte)v))),
                SpecialType.U16 => TryConvertToUnsigned(v => U16(unchecked((ushort)v))),
                SpecialType.U32 => TryConvertToUnsigned(v => U32(unchecked((uint)v))),
                SpecialType.U64 => TryConvertToUnsigned(v => U64(unchecked((ulong)v))),
                SpecialType.U128 => TryConvertToUnsigned(U128),
                SpecialType.USize => TryConvertToUnsigned(v => USize(unchecked((ulong)v))),

                SpecialType.F32 => TryConvertToFloat(v => F32((float)v)),
                SpecialType.F64 => TryConvertToFloat(F64),

                SpecialType.Str => PrimitiveKind == PrimitiveKind.Str ? this : null,

                _ => null,
            };
        }
    }

    private ConstantValue? TryConvertToChar()
    {
        if (!IsCharacter)
            return null;

        var value = AsCharacter().Value;
        return value <= byte.MaxValue ? Character((byte)value) : null;
    }

    private ConstantValue? TryConvertToChar16()
    {
        if (!IsCharacter)
            return null;

        var value = AsCharacter().Value;
        return value <= char.MaxValue ? Character16((char)value) : null;
    }

    private ConstantValue? TryConvertToRune()
    {
        return IsCharacter ? Rune(AsCharacter()) : null;
    }

    private ConstantValue? TryConvertToSigned(Func<Int128, ConstantValue> factory)
    {
        if (IsSignedInteger)
        {
            return PrimitiveKind == PrimitiveKind.I128 ? factory(AsInt128()) : factory(AsInt64());
        }

        if (IsUnsignedInteger)
        {
            return PrimitiveKind == PrimitiveKind.U128
                ? factory(unchecked((Int128)AsUInt128()))
                : factory(AsUInt64());
        }

        if (IsFloat)
        {
            return PrimitiveKind == PrimitiveKind.F32
                ? factory(unchecked((Int128)AsFloat32()))
                : factory(unchecked((Int128)AsFloat64()));
        }

        return null;
    }

    private ConstantValue? TryConvertToUnsigned(Func<UInt128, ConstantValue> factory)
    {
        if (IsUnsignedInteger)
        {
            return PrimitiveKind == PrimitiveKind.U128 ? factory(AsUInt128()) : factory(AsUInt64());
        }

        if (IsSignedInteger)
        {
            return PrimitiveKind == PrimitiveKind.I128
                ? factory(unchecked((UInt128)AsInt128()))
                : factory(unchecked((UInt128)AsInt64()));
        }

        if (IsFloat)
        {
            return PrimitiveKind == PrimitiveKind.F32
                ? factory(unchecked((UInt128)AsFloat32()))
                : factory(unchecked((UInt128)AsFloat64()));
        }

        return null;
    }

    private ConstantValue? TryConvertToFloat(Func<double, ConstantValue> factory)
    {
        if (IsFloat)
        {
            return PrimitiveKind == PrimitiveKind.F32 ? factory(AsFloat32()) : factory(AsFloat64());
        }

        if (IsSignedInteger)
        {
            return PrimitiveKind == PrimitiveKind.I128
                ? factory((double)AsInt128())
                : factory(AsInt64());
        }

        if (IsUnsignedInteger)
        {
            return PrimitiveKind == PrimitiveKind.U128
                ? factory((double)AsUInt128())
                : factory(AsUInt64());
        }

        return null;
    }

    public override string ToString()
    {
        return Kind switch
        {
            ConstantKind.Null => "null",
            ConstantKind.Primitive => PrimitiveKind switch
            {
                PrimitiveKind.Bool => AsBoolean().ToString(),
                PrimitiveKind.Char or PrimitiveKind.Char16 or PrimitiveKind.Rune => AsCharacter()
                    .ToString(),
                PrimitiveKind.I8
                or PrimitiveKind.I16
                or PrimitiveKind.I32
                or PrimitiveKind.I64
                or PrimitiveKind.ISize => AsInt64().ToString(),
                PrimitiveKind.I128 => AsInt128().ToString(),
                PrimitiveKind.U8
                or PrimitiveKind.U16
                or PrimitiveKind.U32
                or PrimitiveKind.U64
                or PrimitiveKind.USize => AsUInt64().ToString(),
                PrimitiveKind.U128 => AsUInt128().ToString(),
                PrimitiveKind.F32 => AsFloat32().ToString(CultureInfo.InvariantCulture),
                PrimitiveKind.F64 => AsFloat64().ToString(CultureInfo.InvariantCulture),
                PrimitiveKind.Str => AsString(),
                _ => throw new ArgumentOutOfRangeException(),
            },
            ConstantKind.Array => $"[{string.Join(", ", AsArray().Select(x => x.ToString()))}]",
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}
