using System.Collections.Immutable;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Prism.Core.Configuration;
using Prism.Core.Mappers;
using Prism.Core.Symbols;

namespace Prism.Core.Semantic;

internal enum ConstantKind : byte
{
    Null,
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
    Array,
}

public readonly record struct BoolConstant(bool Value);

public readonly record struct Char8Constant(byte Value);

public readonly record struct Char16Constant(char Value);

public readonly record struct RuneConstant(Rune Value);

public readonly record struct I8Constant(sbyte Value);

public readonly record struct I16Constant(short Value);

public readonly record struct I32Constant(int Value);

public readonly record struct I64Constant(long Value);

public readonly record struct I128Constant(Int128 Value);

public readonly record struct ISizeConstant(long Value);

public readonly record struct U8Constant(byte Value);

public readonly record struct U16Constant(ushort Value);

public readonly record struct U32Constant(uint Value);

public readonly record struct U64Constant(ulong Value);

public readonly record struct U128Constant(UInt128 Value);

public readonly record struct USizeConstant(ulong Value);

public readonly record struct F32Constant(float Value);

public readonly record struct F64Constant(double Value);

public readonly record struct StringConstant(string Value);

public readonly record struct ArrayConstant(ImmutableArray<ConstantValue> Value);

public readonly record struct NullConstant;

[Union]
public readonly struct ConstantValue : IUnion
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

    private readonly ConstantKind _kind;
    private readonly object? _referenceValue;
    private readonly BlittableStorage _blittableStorage;

    private ConstantValue(ConstantKind kind, BlittableStorage blittableStorage)
    {
        _kind = kind;
        _blittableStorage = blittableStorage;
    }

    private ConstantValue(string referenceValue)
    {
        _kind = ConstantKind.Str;
        _referenceValue = referenceValue;
    }

    private ConstantValue(ImmutableArray<ConstantValue> elements)
    {
        if (elements.Length > 1)
        {
            var elementType = elements[0]._kind;
            for (var i = 1; i < elements.Length; i++)
            {
                if (elements[i]._kind != elementType)
                    throw new ArgumentException(
                        "All elements of an array constant must have the same type",
                        nameof(elements)
                    );
            }
        }
        _kind = ConstantKind.Array;
        _referenceValue = ImmutableCollectionsMarshal.AsArray(elements);
    }

    // ReSharper disable once UnusedParameter.Local
    public ConstantValue(NullConstant value)
    {
        _kind = ConstantKind.Null;
    }

    public ConstantValue(BoolConstant value)
        : this(ConstantKind.Bool, new BlittableStorage { BoolValue = value.Value }) { }

    public ConstantValue(Char8Constant value)
        : this(ConstantKind.Char, new BlittableStorage { CharacterValue = new Rune(value.Value) })
    { }

    public ConstantValue(Char16Constant value)
        : this(ConstantKind.Char16, new BlittableStorage { CharacterValue = new Rune(value.Value) })
    { }

    public ConstantValue(RuneConstant value)
        : this(ConstantKind.Rune, new BlittableStorage { CharacterValue = value.Value }) { }

    public ConstantValue(I8Constant value)
        : this(ConstantKind.I8, new BlittableStorage { I64Value = value.Value }) { }

    public ConstantValue(I16Constant value)
        : this(ConstantKind.I16, new BlittableStorage { I64Value = value.Value }) { }

    public ConstantValue(I32Constant value)
        : this(ConstantKind.I32, new BlittableStorage { I64Value = value.Value }) { }

    public ConstantValue(I64Constant value)
        : this(ConstantKind.I64, new BlittableStorage { I64Value = value.Value }) { }

    public ConstantValue(I128Constant value)
        : this(ConstantKind.I128, new BlittableStorage { I128Value = value.Value }) { }

    public ConstantValue(ISizeConstant value)
        : this(ConstantKind.ISize, new BlittableStorage { I64Value = value.Value }) { }

    public ConstantValue(U8Constant value)
        : this(ConstantKind.U8, new BlittableStorage { U64Value = value.Value }) { }

    public ConstantValue(U16Constant value)
        : this(ConstantKind.U16, new BlittableStorage { U64Value = value.Value }) { }

    public ConstantValue(U32Constant value)
        : this(ConstantKind.U32, new BlittableStorage { U64Value = value.Value }) { }

    public ConstantValue(U64Constant value)
        : this(ConstantKind.U64, new BlittableStorage { U64Value = value.Value }) { }

    public ConstantValue(U128Constant value)
        : this(ConstantKind.U128, new BlittableStorage { U128Value = value.Value }) { }

    public ConstantValue(USizeConstant value)
        : this(ConstantKind.USize, new BlittableStorage { U64Value = value.Value }) { }

    public ConstantValue(F32Constant value)
        : this(ConstantKind.F32, new BlittableStorage { F32Value = value.Value }) { }

    public ConstantValue(F64Constant value)
        : this(ConstantKind.F64, new BlittableStorage { F64Value = value.Value }) { }

    public ConstantValue(StringConstant value)
        : this(value.Value) { }

    public ConstantValue(ArrayConstant value)
        : this(value.Value) { }

    public bool IsNull => _kind == ConstantKind.Null;

    public SpecialType SpecialType => _kind.ToSpecialType();

    public bool IsNumeric => IsSignedInteger || IsUnsignedInteger || IsFloat;

    public bool IsSignedInteger =>
        _kind
            is ConstantKind.I8
                or ConstantKind.I16
                or ConstantKind.I32
                or ConstantKind.I64
                or ConstantKind.I128
                or ConstantKind.ISize;

    public bool IsUnsignedInteger =>
        _kind
            is ConstantKind.U8
                or ConstantKind.U16
                or ConstantKind.U32
                or ConstantKind.U64
                or ConstantKind.U128
                or ConstantKind.USize;

    public bool IsFloat => _kind is ConstantKind.F32 or ConstantKind.F64;

    public bool CanBeNegative => IsSignedInteger || IsFloat;

    public bool IsCharacter =>
        _kind is ConstantKind.Char or ConstantKind.Char16 or ConstantKind.Rune;

    public bool IsString => _kind == ConstantKind.Str;

    public object Value
    {
        get
        {
            return _kind switch
            {
                ConstantKind.Null => new NullConstant(),
                ConstantKind.Bool => new BoolConstant(_blittableStorage.BoolValue),
                ConstantKind.Char => new Char8Constant(
                    (byte)_blittableStorage.CharacterValue.Value
                ),
                ConstantKind.Char16 => new Char16Constant(
                    (char)_blittableStorage.CharacterValue.Value
                ),
                ConstantKind.Rune => new RuneConstant(_blittableStorage.CharacterValue),
                ConstantKind.I8 => new I8Constant((sbyte)_blittableStorage.I64Value),
                ConstantKind.I16 => new I16Constant((short)_blittableStorage.I64Value),
                ConstantKind.I32 => new I32Constant((int)_blittableStorage.I64Value),
                ConstantKind.I64 => new I64Constant(_blittableStorage.I64Value),
                ConstantKind.I128 => new I128Constant(_blittableStorage.I128Value),
                ConstantKind.ISize => new ISizeConstant(_blittableStorage.I64Value),
                ConstantKind.U8 => new U8Constant((byte)_blittableStorage.U64Value),
                ConstantKind.U16 => new U16Constant((ushort)_blittableStorage.U64Value),
                ConstantKind.U32 => new U32Constant((uint)_blittableStorage.U64Value),
                ConstantKind.U64 => new U64Constant(_blittableStorage.U64Value),
                ConstantKind.U128 => new U128Constant(_blittableStorage.U128Value),
                ConstantKind.USize => new USizeConstant(_blittableStorage.U64Value),
                ConstantKind.F32 => new F32Constant(_blittableStorage.F32Value),
                ConstantKind.F64 => new F64Constant(_blittableStorage.F64Value),
                ConstantKind.Str => new StringConstant((string)_referenceValue!),
                ConstantKind.Array => new ArrayConstant(
                    ImmutableCollectionsMarshal.AsImmutableArray((ConstantValue[])_referenceValue!)
                ),
                _ => throw new InvalidOperationException("Invalid constant value state"),
            };
        }
    }

    public bool TryGetValue(out NullConstant value)
    {
        if (_kind == ConstantKind.Null)
        {
            value = new NullConstant();
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out BoolConstant value)
    {
        if (_kind == ConstantKind.Bool)
        {
            value = new BoolConstant(_blittableStorage.BoolValue);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out Char8Constant value)
    {
        if (_kind == ConstantKind.Char)
        {
            value = new Char8Constant((byte)_blittableStorage.CharacterValue.Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out Char16Constant value)
    {
        if (_kind == ConstantKind.Char16)
        {
            value = new Char16Constant((char)_blittableStorage.CharacterValue.Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out RuneConstant value)
    {
        if (_kind == ConstantKind.Rune)
        {
            value = new RuneConstant(_blittableStorage.CharacterValue);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out I8Constant value)
    {
        if (_kind == ConstantKind.I8)
        {
            value = new I8Constant((sbyte)_blittableStorage.I64Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out I16Constant value)
    {
        if (_kind == ConstantKind.I16)
        {
            value = new I16Constant((short)_blittableStorage.I64Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out I32Constant value)
    {
        if (_kind == ConstantKind.I32)
        {
            value = new I32Constant((int)_blittableStorage.I64Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out I64Constant value)
    {
        if (_kind == ConstantKind.I64)
        {
            value = new I64Constant(_blittableStorage.I64Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out I128Constant value)
    {
        if (_kind == ConstantKind.I128)
        {
            value = new I128Constant(_blittableStorage.I128Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out ISizeConstant value)
    {
        if (_kind == ConstantKind.ISize)
        {
            value = new ISizeConstant(_blittableStorage.I64Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out U8Constant value)
    {
        if (_kind == ConstantKind.U8)
        {
            value = new U8Constant((byte)_blittableStorage.U64Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out U16Constant value)
    {
        if (_kind == ConstantKind.U16)
        {
            value = new U16Constant((ushort)_blittableStorage.U64Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out U32Constant value)
    {
        if (_kind == ConstantKind.U32)
        {
            value = new U32Constant((uint)_blittableStorage.U64Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out U64Constant value)
    {
        if (_kind == ConstantKind.U64)
        {
            value = new U64Constant(_blittableStorage.U64Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out U128Constant value)
    {
        if (_kind == ConstantKind.U128)
        {
            value = new U128Constant(_blittableStorage.U128Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out USizeConstant value)
    {
        if (_kind == ConstantKind.USize)
        {
            value = new USizeConstant(_blittableStorage.U64Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out F32Constant value)
    {
        if (_kind == ConstantKind.F32)
        {
            value = new F32Constant(_blittableStorage.F32Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out F64Constant value)
    {
        if (_kind == ConstantKind.F64)
        {
            value = new F64Constant(_blittableStorage.F64Value);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out StringConstant value)
    {
        if (_kind == ConstantKind.Str)
        {
            value = new StringConstant((string)_referenceValue!);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out ArrayConstant value)
    {
        if (_kind == ConstantKind.Array)
        {
            value = new ArrayConstant(
                ImmutableCollectionsMarshal.AsImmutableArray((ConstantValue[])_referenceValue!)
            );
            return true;
        }

        value = default;
        return false;
    }

    private bool AsBoolean()
    {
        return _blittableStorage.BoolValue;
    }

    private Rune AsCharacter()
    {
        return _blittableStorage.CharacterValue;
    }

    private long AsInt64()
    {
        return _blittableStorage.I64Value;
    }

    private Int128 AsInt128()
    {
        return _blittableStorage.I128Value;
    }

    private ulong AsUInt64()
    {
        return _blittableStorage.U64Value;
    }

    private UInt128 AsUInt128()
    {
        return _blittableStorage.U128Value;
    }

    private float AsFloat32()
    {
        return _blittableStorage.F32Value;
    }

    private double AsFloat64()
    {
        return _blittableStorage.F64Value;
    }

    private string AsString()
    {
        return (string)_referenceValue!;
    }

    private ImmutableArray<ConstantValue> AsArray()
    {
        return ImmutableCollectionsMarshal.AsImmutableArray((ConstantValue[]?)_referenceValue);
    }

    public ConstantValue? TryNegate(CompilationSettings settings)
    {
        switch (_kind)
        {
            case ConstantKind.I8:
            case ConstantKind.I16:
            case ConstantKind.I32:
            case ConstantKind.I64:
            case ConstantKind.ISize:
                return new ConstantValue(
                    _kind,
                    new BlittableStorage { I64Value = -_blittableStorage.I64Value }
                );
            case ConstantKind.I128:
                return new ConstantValue(
                    _kind,
                    new BlittableStorage { I128Value = -_blittableStorage.I128Value }
                );
            case ConstantKind.U8:
            case ConstantKind.U16:
                return new I32Constant(unchecked(-(ushort)_blittableStorage.U64Value));
            case ConstantKind.U32:
                return new I64Constant(unchecked(-(uint)_blittableStorage.U64Value));
            case ConstantKind.U64:
                return new I128Constant(-(Int128)_blittableStorage.U64Value);
            case ConstantKind.USize:
                return settings.PointerWidth switch
                {
                    PointerWidth.X32 => new I64Constant(
                        unchecked(-(uint)_blittableStorage.U64Value)
                    ),
                    PointerWidth.X64 => new I128Constant(
                        unchecked(-(Int128)_blittableStorage.U64Value)
                    ),
                    _ => throw new InvalidOperationException("Invalid pointer width"),
                };
            case ConstantKind.F32:
                return new F32Constant(-_blittableStorage.F32Value);
            case ConstantKind.F64:
                return new F64Constant(-_blittableStorage.F64Value);
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

    public ConstantValue? TryLogicalNot()
    {
        return this switch
        {
            BoolConstant(var value) => new BoolConstant(!value),
            _ => null,
        };
    }

    public ConstantValue? TryBitwiseNot(CompilationSettings settings)
    {
        return this switch
        {
            I8Constant(var value) => new I8Constant((sbyte)~value),
            I16Constant(var value) => new I16Constant((short)~value),
            I32Constant(var value) => new I32Constant(~value),
            I64Constant(var value) => new I64Constant(~value),
            I128Constant(var value) => new I128Constant(~value),
            ISizeConstant(var value) => settings.PointerWidth switch
            {
                PointerWidth.X32 => new ISizeConstant(~(int)value),
                PointerWidth.X64 => new ISizeConstant(~value),
                _ => throw new InvalidOperationException("Invalid pointer width"),
            },
            U8Constant(var value) => new U8Constant((byte)~value),
            U16Constant(var value) => new U16Constant((ushort)~value),
            U32Constant(var value) => new U32Constant(~value),
            U64Constant(var value) => new U64Constant(~value),
            U128Constant(var value) => new U128Constant(~value),
            USizeConstant(var value) => settings.PointerWidth switch
            {
                PointerWidth.X32 => new USizeConstant(~(uint)value),
                PointerWidth.X64 => new USizeConstant(~value),
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
        if (_kind != right._kind)
            return null;

        return _kind switch
        {
            ConstantKind.I8 => EvalSigned(
                op,
                (sbyte)AsInt64(),
                (sbyte)right.AsInt64(),
                i => new I8Constant(i)
            ),
            ConstantKind.I16 => EvalSigned(
                op,
                (short)AsInt64(),
                (short)right.AsInt64(),
                i => new I16Constant(i)
            ),
            ConstantKind.I32 => EvalSigned(
                op,
                (int)AsInt64(),
                (int)right.AsInt64(),
                i => new I32Constant(i)
            ),
            ConstantKind.I64 => EvalSigned(op, AsInt64(), right.AsInt64(), i => new I64Constant(i)),
            ConstantKind.I128 => EvalSigned(op, AsInt128(), right.AsInt128(), i => new I128Constant(
                i
            )),

            ConstantKind.U8 => EvalUnsigned(
                op,
                (byte)AsUInt64(),
                (byte)right.AsUInt64(),
                i => new U8Constant(i)
            ),
            ConstantKind.U16 => EvalUnsigned(
                op,
                (ushort)AsUInt64(),
                (ushort)right.AsUInt64(),
                i => new U16Constant(i)
            ),
            ConstantKind.U32 => EvalUnsigned(
                op,
                (uint)AsUInt64(),
                (uint)right.AsUInt64(),
                i => new U32Constant(i)
            ),
            ConstantKind.U64 => EvalUnsigned(op, AsUInt64(), right.AsUInt64(), i => new U64Constant(
                i
            )),
            ConstantKind.U128 => EvalUnsigned(
                op,
                AsUInt128(),
                right.AsUInt128(),
                i => new U128Constant(i)
            ),

            ConstantKind.F32 => EvalFloat(op, AsFloat32(), right.AsFloat32(), f => new F32Constant(
                f
            )),
            ConstantKind.F64 => EvalFloat(op, AsFloat64(), right.AsFloat64(), f => new F64Constant(
                f
            )),

            ConstantKind.Bool => EvalBool(op, AsBoolean(), right.AsBoolean()),
            ConstantKind.ISize => settings.PointerWidth switch
            {
                PointerWidth.X32 => EvalSigned(
                    op,
                    (int)AsInt64(),
                    (int)right.AsInt64(),
                    i => new ISizeConstant(i)
                ),
                PointerWidth.X64 => EvalSigned(
                    op,
                    AsInt64(),
                    right.AsInt64(),
                    i => new ISizeConstant(i)
                ),
                _ => null,
            },
            ConstantKind.USize => settings.PointerWidth switch
            {
                PointerWidth.X32 => EvalUnsigned(
                    op,
                    (uint)AsUInt64(),
                    (uint)right.AsUInt64(),
                    i => new USizeConstant(i)
                ),
                PointerWidth.X64 => EvalUnsigned(
                    op,
                    AsUInt64(),
                    right.AsUInt64(),
                    i => new USizeConstant(i)
                ),
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
            BinaryOperation.Equality => new BoolConstant(left == right),
            BinaryOperation.NotEquals => new BoolConstant(left != right),
            BinaryOperation.LessThan => new BoolConstant(left < right),
            BinaryOperation.LessThanOrEquals => new BoolConstant(left <= right),
            BinaryOperation.GreaterThan => new BoolConstant(left > right),
            BinaryOperation.GreaterThanOrEquals => new BoolConstant(left >= right),
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
            BinaryOperation.Equality => new BoolConstant(left == right),
            BinaryOperation.NotEquals => new BoolConstant(left != right),
            BinaryOperation.LessThan => new BoolConstant(left < right),
            BinaryOperation.LessThanOrEquals => new BoolConstant(left <= right),
            BinaryOperation.GreaterThan => new BoolConstant(left > right),
            BinaryOperation.GreaterThanOrEquals => new BoolConstant(left >= right),
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
            BinaryOperation.Equality => new BoolConstant(left == right),
            BinaryOperation.NotEquals => new BoolConstant(left != right),
            BinaryOperation.LessThan => new BoolConstant(left < right),
            BinaryOperation.LessThanOrEquals => new BoolConstant(left <= right),
            BinaryOperation.GreaterThan => new BoolConstant(left > right),
            BinaryOperation.GreaterThanOrEquals => new BoolConstant(left >= right),
            _ => null,
        };
    }

    private static ConstantValue? EvalBool(BinaryOperation op, bool left, bool right)
    {
        return op switch
        {
            BinaryOperation.LogicalAnd => new BoolConstant(left && right),
            BinaryOperation.LogicalOr => new BoolConstant(left || right),
            BinaryOperation.Equality => new BoolConstant(left == right),
            BinaryOperation.NotEquals => new BoolConstant(left != right),
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

            return typeSymbol.SpecialType switch
            {
                SpecialType.Bool => _kind == ConstantKind.Bool ? this : null,
                SpecialType.Char => TryConvertToChar(),
                SpecialType.Char16 => TryConvertToChar16(),
                SpecialType.Rune => TryConvertToRune(),

                SpecialType.I8 => TryConvertToSigned(v => new I8Constant(unchecked((sbyte)v))),
                SpecialType.I16 => TryConvertToSigned(v => new I16Constant(unchecked((short)v))),
                SpecialType.I32 => TryConvertToSigned(v => new I32Constant(unchecked((int)v))),
                SpecialType.I64 => TryConvertToSigned(v => new I64Constant(unchecked((long)v))),
                SpecialType.I128 => TryConvertToSigned(v => new I128Constant(v)),
                SpecialType.ISize => TryConvertToSigned(v => new ISizeConstant(unchecked((long)v))),

                SpecialType.U8 => TryConvertToUnsigned(v => new U8Constant(unchecked((byte)v))),
                SpecialType.U16 => TryConvertToUnsigned(v => new U16Constant(unchecked((ushort)v))),
                SpecialType.U32 => TryConvertToUnsigned(v => new U32Constant(unchecked((uint)v))),
                SpecialType.U64 => TryConvertToUnsigned(v => new U64Constant(unchecked((ulong)v))),
                SpecialType.U128 => TryConvertToUnsigned(v => new U128Constant(v)),
                SpecialType.USize => TryConvertToUnsigned(v => new USizeConstant(
                    unchecked((ulong)v)
                )),

                SpecialType.F32 => TryConvertToFloat(v => new F32Constant((float)v)),
                SpecialType.F64 => TryConvertToFloat(v => new F64Constant(v)),

                SpecialType.Str => _kind == ConstantKind.Str ? this : null,

                _ => null,
            };
        }
    }

    private ConstantValue? TryConvertToChar()
    {
        if (!IsCharacter)
            return null;

        var value = AsCharacter().Value;
        return value <= byte.MaxValue ? new Char8Constant((byte)value) : null;
    }

    private ConstantValue? TryConvertToChar16()
    {
        if (!IsCharacter)
            return null;

        var value = AsCharacter().Value;
        return value <= char.MaxValue ? new Char16Constant((char)value) : null;
    }

    private ConstantValue? TryConvertToRune()
    {
        return IsCharacter ? new RuneConstant(AsCharacter()) : null;
    }

    private ConstantValue? TryConvertToSigned(Func<Int128, ConstantValue> factory)
    {
        if (IsSignedInteger)
        {
            return _kind == ConstantKind.I128 ? factory(AsInt128()) : factory(AsInt64());
        }

        if (IsUnsignedInteger)
        {
            return _kind == ConstantKind.U128
                ? factory(unchecked((Int128)AsUInt128()))
                : factory(AsUInt64());
        }

        if (IsFloat)
        {
            return _kind == ConstantKind.F32
                ? factory(unchecked((Int128)AsFloat32()))
                : factory(unchecked((Int128)AsFloat64()));
        }

        return null;
    }

    private ConstantValue? TryConvertToUnsigned(Func<UInt128, ConstantValue> factory)
    {
        if (IsUnsignedInteger)
        {
            return _kind == ConstantKind.U128 ? factory(AsUInt128()) : factory(AsUInt64());
        }

        if (IsSignedInteger)
        {
            return _kind == ConstantKind.I128
                ? factory(unchecked((UInt128)AsInt128()))
                : factory(unchecked((UInt128)AsInt64()));
        }

        if (IsFloat)
        {
            return _kind == ConstantKind.F32
                ? factory(unchecked((UInt128)AsFloat32()))
                : factory(unchecked((UInt128)AsFloat64()));
        }

        return null;
    }

    private ConstantValue? TryConvertToFloat(Func<double, ConstantValue> factory)
    {
        if (IsFloat)
        {
            return _kind == ConstantKind.F32 ? factory(AsFloat32()) : factory(AsFloat64());
        }

        if (IsSignedInteger)
        {
            return _kind == ConstantKind.I128 ? factory((double)AsInt128()) : factory(AsInt64());
        }

        if (IsUnsignedInteger)
        {
            return _kind == ConstantKind.U128 ? factory((double)AsUInt128()) : factory(AsUInt64());
        }

        return null;
    }

    public override string ToString()
    {
        return _kind switch
        {
            ConstantKind.Null => "null",
            ConstantKind.Bool => AsBoolean().ToString(),
            ConstantKind.Char or ConstantKind.Char16 or ConstantKind.Rune => AsCharacter()
                .ToString(),
            ConstantKind.I8
            or ConstantKind.I16
            or ConstantKind.I32
            or ConstantKind.I64
            or ConstantKind.ISize => AsInt64().ToString(),
            ConstantKind.I128 => AsInt128().ToString(),
            ConstantKind.U8
            or ConstantKind.U16
            or ConstantKind.U32
            or ConstantKind.U64
            or ConstantKind.USize => AsUInt64().ToString(),
            ConstantKind.U128 => AsUInt128().ToString(),
            ConstantKind.F32 => AsFloat32().ToString(CultureInfo.InvariantCulture),
            ConstantKind.F64 => AsFloat64().ToString(CultureInfo.InvariantCulture),
            ConstantKind.Str => AsString(),
            ConstantKind.Array => $"[{string.Join(", ", AsArray().Select(x => x.ToString()))}]",
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}
