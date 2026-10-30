using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using MagicArchive;
using Prism.Core.Configuration;
using Prism.Core.Constants;
using Prism.Core.Symbols;

namespace Prism.Core.Semantic;

public readonly record struct NullConstant;

[Union]
[Archivable(GenerateType.Custom)]
public readonly partial struct ConstantValue
    : IUnion,
        IEquatable<ConstantValue>,
        IEqualityOperators<ConstantValue, ConstantValue, bool>
{
    private readonly ConstantHeader _header;
    private readonly ConstantStorage _storage;

    private ConstantValue(ConstantHeader header, ConstantStorage blittableStorage)
    {
        _header = header;
        _storage = blittableStorage;
    }

    // ReSharper disable once UnusedParameter.Local
    public ConstantValue(NullConstant value)
    {
        _header = ConstantHeader.Null;
    }

    public ConstantValue(bool value)
        : this(ConstantHeader.Bool, new ConstantStorage(value)) { }

    public ConstantValue(CharacterConstant value)
        : this(ConstantHeader.Character(value.Kind), new ConstantStorage(value.RawValue)) { }

    public ConstantValue(IntegerConstant value)
        : this(ConstantHeader.Integer(value.Kind), new ConstantStorage(value.RawValue)) { }

    public ConstantValue(FloatConstant value)
        : this(
            ConstantHeader.Float(value.Kind),
            value switch
            {
                float f => new ConstantStorage(f),
                double d => new ConstantStorage(d),
            }
        ) { }

    public ConstantValue(string value)
    {
        _header = ConstantHeader.String;
        _storage = new ConstantStorage(value);
    }

    public bool IsNull => _header.Kind == ConstantKind.Null;

    public bool IsNumeric => IsSignedInteger || IsUnsignedInteger || IsFloat;

    public bool IsSignedInteger =>
        _header is { Kind: ConstantKind.Integer, IntegerKind.Sign: IntegerSign.Signed };

    public bool IsUnsignedInteger =>
        _header is { Kind: ConstantKind.Integer, IntegerKind.Sign: IntegerSign.Unsigned };

    public bool IsFloat => _header.Kind == ConstantKind.Float;

    public bool CanBeNegative => IsSignedInteger || IsFloat;

    public bool IsCharacter => _header.Kind == ConstantKind.Character;

    public bool IsString => _header.Kind == ConstantKind.String;

    private static readonly object BoxedNull = new NullConstant();
    private static readonly object BoxedFalse = false;
    private static readonly object BoxedTrue = true;

    public object Value
    {
        get
        {
            return _header.Kind switch
            {
                ConstantKind.Null => BoxedNull,
                ConstantKind.Bool => AsBoolean() ? BoxedTrue : BoxedFalse,
                ConstantKind.Character => AsCharacter(),
                ConstantKind.Integer => AsInteger(),
                ConstantKind.Float => AsFloat(),
                ConstantKind.String => AsString(),
                ConstantKind.Array => throw new NotImplementedException(),
                _ => throw new InvalidOperationException("Invalid constant value state"),
            };
        }
    }

    public bool TryGetValue(out NullConstant value)
    {
        if (IsNull)
        {
            value = new NullConstant();
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out bool value)
    {
        if (_header.Kind == ConstantKind.Bool)
        {
            value = _storage.BoolValue;
            return true;
        }

        value = false;
        return false;
    }

    public bool TryGetValue(out CharacterConstant value)
    {
        if (_header.Kind == ConstantKind.Character)
        {
            value = AsCharacter();
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out IntegerConstant value)
    {
        if (_header.Kind == ConstantKind.Integer)
        {
            value = AsInteger();
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out FloatConstant value)
    {
        if (_header.Kind == ConstantKind.Float)
        {
            value = AsFloat();
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue([NotNullWhen(true)] out string? value)
    {
        if (_header.Kind == ConstantKind.String)
        {
            value = AsString();
            return true;
        }

        value = null;
        return false;
    }

    private bool AsBoolean()
    {
        return _storage.BoolValue;
    }

    private CharacterConstant AsCharacter()
    {
        return CharacterConstant.From(_header.CharacterKind, _storage.CharacterValue);
    }

    private IntegerConstant AsInteger()
    {
        return IntegerConstant.From(_header.IntegerKind, _storage.IntegerValue);
    }

    private FloatConstant AsFloat()
    {
        return _header.FloatKind switch
        {
            FloatKind.F32 => _storage.FloatValue,
            FloatKind.F64 => _storage.DoubleValue,
            _ => throw new InvalidOperationException("Invalid float kind"),
        };
    }

    private string AsString()
    {
        return Unsafe.As<string>(_storage.ReferenceValue);
    }

    private static ConstantValue? FromNullable(bool? value)
    {
        // ReSharper disable once MergeConditionalExpression
        return value.HasValue ? value.Value : null;
    }

    private static ConstantValue? FromNullable(IntegerConstant? value)
    {
        // ReSharper disable once MergeConditionalExpression
        return value.HasValue ? value.Value : null;
    }

    public ConstantValue? TryNegate(CompilationSettings settings)
    {
        return this switch
        {
            IntegerConstant i => FromNullable(i.TryNegate()),
            FloatConstant f => -f,
            _ => null,
        };
    }

    public ConstantValue? TryLogicalNot()
    {
        return this switch
        {
            bool value => !value,
            _ => null,
        };
    }

    public ConstantValue? TryBitwiseNot(CompilationSettings settings)
    {
        return this switch
        {
            IntegerConstant i => ~i,
            _ => null,
        };
    }

    public ConstantValue? TryBinary(
        BinaryOperation op,
        in ConstantValue right,
        CompilationSettings settings
    )
    {
        if (_header.Kind != right._header.Kind)
            return null;

        return this switch
        {
            NullConstant => new NullConstant(),
            bool b => EvalBool(op, b, right.AsBoolean()),
            IntegerConstant i => EvalInteger(op, i, right.AsInteger()),
            FloatConstant f => EvalFloat(op, f, right.AsFloat()),
            CharacterConstant c => EvalCharacter(op, c, right.AsCharacter()),
            _ => null,
        };
    }

    private static ConstantValue? EvalInteger(
        BinaryOperation op,
        IntegerConstant left,
        IntegerConstant right
    )
    {
        return op switch
        {
            BinaryOperation.Addition => FromNullable(left.TryAdd(right)),
            BinaryOperation.Subtraction => FromNullable(left.TrySubtract(right)),
            BinaryOperation.Multiplication => FromNullable(left.TryMultiply(right)),
            BinaryOperation.Division => FromNullable(left.TryDivide(right)),
            BinaryOperation.Modulo => FromNullable(left.TryMod(right)),
            BinaryOperation.BitwiseAnd => FromNullable(left.TryBitwiseAnd(left)),
            BinaryOperation.BitwiseOr => FromNullable(left.TryBitwiseOr(right)),
            BinaryOperation.BitwiseXor => FromNullable(left.TryBitwiseXor(right)),
            BinaryOperation.ShiftLeft => FromNullable(left.TryShiftLeft(right)),
            BinaryOperation.ShiftRight => FromNullable(left.TryShiftRight(right)),
            BinaryOperation.UnsignedShiftRight => FromNullable(left.TryUnsignedShiftRight(right)),
            BinaryOperation.Equality => FromNullable(left.TryEqual(right)),
            BinaryOperation.NotEquals => FromNullable(left.TryNotEqual(right)),
            BinaryOperation.LessThan => FromNullable(left.TryLessThan(right)),
            BinaryOperation.LessThanOrEquals => FromNullable(left.TryLessThanOrEqual(right)),
            BinaryOperation.GreaterThan => FromNullable(left.TryGreaterThan(right)),
            BinaryOperation.GreaterThanOrEquals => FromNullable(left.TryGreaterThanOrEqual(right)),
            _ => null,
        };
    }

    private static ConstantValue? EvalFloat(
        BinaryOperation op,
        FloatConstant left,
        FloatConstant right
    )
    {
        return op switch
        {
            BinaryOperation.Addition => left + right,
            BinaryOperation.Subtraction => left - right,
            BinaryOperation.Multiplication => left * right,
            BinaryOperation.Division => left / right,
            BinaryOperation.Modulo => left % right,
            BinaryOperation.Equality => left.Equivalent(right),
            BinaryOperation.NotEquals => left.NotEquivalent(right),
            BinaryOperation.LessThan => left.LessThan(right),
            BinaryOperation.LessThanOrEquals => left.LessThanOrEqual(right),
            BinaryOperation.GreaterThan => left.GreaterThan(right),
            BinaryOperation.GreaterThanOrEquals => left.GreaterThanOrEqual(right),
            _ => null,
        };
    }

    private static ConstantValue? EvalCharacter(
        BinaryOperation op,
        CharacterConstant left,
        CharacterConstant right
    )
    {
        return op switch
        {
            BinaryOperation.Equality => left.EquivalentTo(right),
            BinaryOperation.NotEquals => left.NotEquivalentTo(right),
            _ => null,
        };
    }

    private static ConstantValue? EvalBool(BinaryOperation op, bool left, bool right)
    {
        return op switch
        {
            BinaryOperation.LogicalAnd => left && right,
            BinaryOperation.LogicalOr => left || right,
            BinaryOperation.Equality => left == right,
            BinaryOperation.NotEquals => left != right,
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

            if (typeSymbol.SpecialType.IsCharacter)
            {
                return TryConvertToCharacter(typeSymbol.SpecialType);
            }

            return typeSymbol.SpecialType switch
            {
                SpecialType.Bool => _header.Kind == ConstantKind.Bool ? this : (ConstantValue?)null,
                { IsCharacter: true } => TryConvertToCharacter(typeSymbol.SpecialType),

                { IsInteger: true } => TryConvertToInteger(
                    typeSymbol.SpecialType,
                    settings.PointerWidth
                ),

                { IsFloatingPoint: true } => TryConvertToFloat(typeSymbol.SpecialType),

                SpecialType.Str => _header.Kind == ConstantKind.String
                    ? this
                    : (ConstantValue?)null,

                _ => null,
            };
        }
    }

    private static CharacterKind ToCharacterKind(SpecialType type)
    {
        return type switch
        {
            SpecialType.Char => CharacterKind.C8,
            SpecialType.Char16 => CharacterKind.C16,
            SpecialType.Rune => CharacterKind.C32,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    private ConstantValue? TryConvertToCharacter(SpecialType type)
    {
        return this switch
        {
            CharacterConstant ch => ch.ConvertTo(ToCharacterKind(type)),
            _ => null,
        };
    }

    private IntegerKind ToIntegerKind(SpecialType type, PointerWidth width)
    {
        return type switch
        {
            SpecialType.I8 => IntegerKind.I8,
            SpecialType.I16 => IntegerKind.I16,
            SpecialType.I32 => IntegerKind.I32,
            SpecialType.I64 => IntegerKind.I64,
            SpecialType.I128 => IntegerKind.I128,
            SpecialType.ISize => width switch
            {
                PointerWidth.X32 => IntegerKind.I32,
                PointerWidth.X64 => IntegerKind.I64,
                _ => throw new ArgumentOutOfRangeException(nameof(width), width, null),
            },
            SpecialType.U8 => IntegerKind.U8,
            SpecialType.U16 => IntegerKind.U16,
            SpecialType.U32 => IntegerKind.U32,
            SpecialType.U64 => IntegerKind.U64,
            SpecialType.U128 => IntegerKind.U128,
            SpecialType.USize => width switch
            {
                PointerWidth.X32 => IntegerKind.U32,
                PointerWidth.X64 => IntegerKind.U64,
                _ => throw new ArgumentOutOfRangeException(nameof(width), width, null),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    private ConstantValue? TryConvertToInteger(SpecialType type, PointerWidth pointerWidth)
    {
        var integerKind = ToIntegerKind(type, pointerWidth);
        return this switch
        {
            IntegerConstant i => i.ConvertTo(integerKind),
            FloatConstant f => f.ConvertTo(integerKind),
            _ => null,
        };
    }

    private static FloatKind ToFloatKind(SpecialType type)
    {
        return type switch
        {
            SpecialType.F32 => FloatKind.F32,
            SpecialType.F64 => FloatKind.F64,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    private ConstantValue? TryConvertToFloat(SpecialType type)
    {
        var floatKind = ToFloatKind(type);
        return this switch
        {
            IntegerConstant i => i.ConvertTo(floatKind),
            FloatConstant f => f.ConvertTo(floatKind),
            _ => null,
        };
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is ConstantValue other && Equals(other);
    }

    public bool Equals(ConstantValue other)
    {
        return this switch
        {
            NullConstant value => other is NullConstant otherValue && value.Equals(otherValue),
            bool value => other is bool otherValue && value.Equals(otherValue),
            IntegerConstant value => other is IntegerConstant otherValue
                && value.Equals(otherValue),
            FloatConstant value => other is FloatConstant otherValue && value.Equals(otherValue),
            CharacterConstant value => other is CharacterConstant otherValue
                && value.Equals(otherValue),
            string value => other is string otherValue && value.Equals(otherValue),
        };
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            _header.GetHashCode(),
            this switch
            {
                NullConstant value => value.GetHashCode(),
                bool value => value.GetHashCode(),
                IntegerConstant value => value.GetHashCode(),
                FloatConstant value => value.GetHashCode(),
                CharacterConstant value => value.GetHashCode(),
                string value => value.GetHashCode(),
            }
        );
    }

    public override string ToString()
    {
        return this switch
        {
            NullConstant => "null",
            bool b => b.ToString(),
            IntegerConstant i => i.ToString(),
            FloatConstant f => f.ToString(),
            CharacterConstant c => c.ToString(),
            string s => s,
        };
    }

    public static bool operator ==(ConstantValue left, ConstantValue right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ConstantValue left, ConstantValue right)
    {
        return !left.Equals(right);
    }

    static void IArchivable<ConstantValue>.Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ConstantValue value
    )
    {
        switch (value)
        {
            case NullConstant:
                writer.WriteBlittable(value._header);
                break;
            case bool b:
            {
                var rawHeaderByte = value._header.AsRaw();
                if (b)
                {
                    rawHeaderByte |= 1;
                }
                writer.WriteBlittable(rawHeaderByte);
                break;
            }
            case CharacterConstant characterConstant:
                CharacterConstant.SerializeWithHeader(
                    ref writer,
                    value._header.AsRaw(),
                    characterConstant.Kind,
                    characterConstant.RawValue
                );
                break;
            case IntegerConstant integerConstant:
                IntegerConstant.SerializeWithHeader(
                    ref writer,
                    value._header.AsRaw(),
                    integerConstant.Kind,
                    integerConstant.RawValue
                );
                break;
            case FloatConstant floatConstant:
                floatConstant.SerializeWithHeader(ref writer, value._header.AsRaw());
                break;
            case string s:
                writer.WriteBlittable(value._header);
                writer.WriteString(s);
                break;
        }
    }

    static void IArchivable<ConstantValue>.Deserialize(
        ref ArchiveReader reader,
        scoped ref ConstantValue value
    )
    {
        var header = reader.ReadBlittable<ConstantHeader>();
        switch (header.Kind)
        {
            case ConstantKind.Null:
                value = new NullConstant();
                break;
            case ConstantKind.Bool:
                value = (header.AsRaw() & 1) != 0;
                break;
            case ConstantKind.Integer:
                value = new ConstantValue(
                    header,
                    new ConstantStorage(IntegerConstant.ReadValue(ref reader, header.IntegerKind))
                );
                break;
            case ConstantKind.Float:
                value = FloatConstant.ReadValue(ref reader, header.FloatKind);
                break;
            case ConstantKind.Character:
                value = new ConstantValue(
                    header,
                    new ConstantStorage(
                        CharacterConstant.ReadValue(ref reader, header.CharacterKind)
                    )
                );
                break;
            case ConstantKind.String:
                value =
                    reader.ReadString()
                    ?? throw new ArchiveSerializationException("Invalid string constant");
                break;
            case ConstantKind.Array:
                throw new NotImplementedException("Array constants are not ready yet");
            default:
                throw new ArchiveSerializationException("Invalid constant kind");
        }
    }
}
