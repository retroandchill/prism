using System.Runtime.InteropServices;
using System.Text;
using Prism.Core.Configuration;
using Prism.Core.Mappers;
using Prism.Core.Symbols;

namespace Prism.Core.Semantic;

public enum ConstantKind : byte
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
    private readonly string? _stringValue;
    private readonly BlittableStorage _blittableStorage;

    private ConstantValue(ConstantKind kind, BlittableStorage blittableStorage)
    {
        Kind = kind;
        _blittableStorage = blittableStorage;
    }

    private ConstantValue(string stringValue)
    {
        Kind = ConstantKind.Str;
        _stringValue = stringValue;
    }

    public static ConstantValue Boolean(bool value)
    {
        return new ConstantValue(ConstantKind.Bool, new BlittableStorage { BoolValue = value });
    }

    public static ConstantValue Character(byte value)
    {
        return new ConstantValue(
            ConstantKind.Char,
            new BlittableStorage { CharacterValue = new Rune(value) }
        );
    }

    public static ConstantValue Character16(char value)
    {
        return new ConstantValue(
            ConstantKind.Char16,
            new BlittableStorage { CharacterValue = new Rune(value) }
        );
    }

    public static ConstantValue Rune(Rune value)
    {
        return new ConstantValue(
            ConstantKind.Rune,
            new BlittableStorage { CharacterValue = value }
        );
    }

    public static ConstantValue I8(sbyte value)
    {
        return new ConstantValue(ConstantKind.I8, new BlittableStorage { I64Value = value });
    }

    public static ConstantValue I16(short value)
    {
        return new ConstantValue(ConstantKind.I16, new BlittableStorage { I64Value = value });
    }

    public static ConstantValue I32(int value)
    {
        return new ConstantValue(ConstantKind.I32, new BlittableStorage { I64Value = value });
    }

    public static ConstantValue I64(long value)
    {
        return new ConstantValue(ConstantKind.I64, new BlittableStorage { I64Value = value });
    }

    public static ConstantValue I128(Int128 value)
    {
        return new ConstantValue(ConstantKind.I128, new BlittableStorage { I128Value = value });
    }

    public static ConstantValue ISize(long value)
    {
        return new ConstantValue(ConstantKind.ISize, new BlittableStorage { I64Value = value });
    }

    public static ConstantValue U8(byte value)
    {
        return new ConstantValue(ConstantKind.U8, new BlittableStorage { U64Value = value });
    }

    public static ConstantValue U16(ushort value)
    {
        return new ConstantValue(ConstantKind.U16, new BlittableStorage { U64Value = value });
    }

    public static ConstantValue U32(uint value)
    {
        return new ConstantValue(ConstantKind.U32, new BlittableStorage { U64Value = value });
    }

    public static ConstantValue U64(ulong value)
    {
        return new ConstantValue(ConstantKind.U64, new BlittableStorage { U64Value = value });
    }

    public static ConstantValue U128(UInt128 value)
    {
        return new ConstantValue(ConstantKind.U128, new BlittableStorage { U128Value = value });
    }

    public static ConstantValue USize(ulong value)
    {
        return new ConstantValue(ConstantKind.USize, new BlittableStorage { U64Value = value });
    }

    public static ConstantValue F32(float value)
    {
        return new ConstantValue(ConstantKind.F32, new BlittableStorage { F32Value = value });
    }

    public static ConstantValue F64(double value)
    {
        return new ConstantValue(ConstantKind.F64, new BlittableStorage { F64Value = value });
    }

    public static ConstantValue Str(string value)
    {
        return new ConstantValue(value);
    }

    public SpecialType SpecialType => Kind.ToSpecialType();

    public bool IsNumeric => IsSignedInteger || IsUnsignedInteger || IsFloat;

    public bool IsSignedInteger =>
        Kind
            is ConstantKind.I8
                or ConstantKind.I16
                or ConstantKind.I32
                or ConstantKind.I64
                or ConstantKind.I128
                or ConstantKind.ISize;

    public bool IsUnsignedInteger =>
        Kind
            is ConstantKind.U8
                or ConstantKind.U16
                or ConstantKind.U32
                or ConstantKind.U64
                or ConstantKind.U128
                or ConstantKind.USize;

    public bool IsFloat => Kind is ConstantKind.F32 or ConstantKind.F64;

    public bool CanBeNegative => IsSignedInteger || IsFloat;

    public bool IsCharacter =>
        Kind is ConstantKind.Char or ConstantKind.Char16 or ConstantKind.Rune;

    private void ThrowIfNotValidType(bool condition)
    {
        if (!condition)
        {
            throw new InvalidOperationException("Invalid type");
        }
    }

    public bool AsBoolean()
    {
        ThrowIfNotValidType(Kind == ConstantKind.Bool);
        return _blittableStorage.BoolValue;
    }

    public Rune AsCharacter()
    {
        ThrowIfNotValidType(IsCharacter);
        return _blittableStorage.CharacterValue;
    }

    public long AsInt64()
    {
        ThrowIfNotValidType(Kind != ConstantKind.I128 && IsSignedInteger);
        return _blittableStorage.I64Value;
    }

    public Int128 AsInt128()
    {
        ThrowIfNotValidType(Kind == ConstantKind.I128);
        return _blittableStorage.I128Value;
    }

    public ulong AsUInt64()
    {
        ThrowIfNotValidType(Kind != ConstantKind.U128 && IsUnsignedInteger);
        return _blittableStorage.U64Value;
    }

    public UInt128 AsUInt128()
    {
        ThrowIfNotValidType(Kind == ConstantKind.U128);
        return _blittableStorage.U128Value;
    }

    public float AsFloat32()
    {
        ThrowIfNotValidType(Kind == ConstantKind.F32);
        return _blittableStorage.F32Value;
    }

    public double AsFloat64()
    {
        ThrowIfNotValidType(Kind == ConstantKind.F64);
        return _blittableStorage.F64Value;
    }

    public string AsString()
    {
        ThrowIfNotValidType(Kind == ConstantKind.Str);
        return _stringValue!;
    }

    public ConstantValue? TryNegate(CompilationSettings settings)
    {
        switch (Kind)
        {
            case ConstantKind.I8:
            case ConstantKind.I16:
            case ConstantKind.I32:
            case ConstantKind.I64:
            case ConstantKind.ISize:
                return new ConstantValue(
                    Kind,
                    new BlittableStorage { I64Value = -_blittableStorage.I64Value }
                );
            case ConstantKind.I128:
                return new ConstantValue(
                    Kind,
                    new BlittableStorage { I128Value = -_blittableStorage.I128Value }
                );
            case ConstantKind.U8:
            case ConstantKind.U16:
                return I32(unchecked(-(ushort)_blittableStorage.U64Value));
            case ConstantKind.U32:
                return I64(unchecked(-(uint)_blittableStorage.U64Value));
            case ConstantKind.U64:
                return I128(-(Int128)_blittableStorage.U64Value);
            case ConstantKind.USize:
                return settings.PointerWidth switch
                {
                    PointerWidth.X32 => I64(unchecked(-(uint)_blittableStorage.U64Value)),
                    PointerWidth.X64 => I128(unchecked(-(Int128)_blittableStorage.U64Value)),
                    _ => throw new InvalidOperationException("Invalid pointer width"),
                };
            case ConstantKind.F32:
                return F32(-_blittableStorage.F32Value);
            case ConstantKind.F64:
                return F64(-_blittableStorage.F64Value);
            case ConstantKind.Bool:
            case ConstantKind.Char:
            case ConstantKind.Char16:
            case ConstantKind.Rune:
            case ConstantKind.Str:
            case ConstantKind.U128:
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
