using System.Collections.Immutable;
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
}
