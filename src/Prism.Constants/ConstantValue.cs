using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using MagicArchive;

namespace Prism.Constants;

public readonly record struct NullConstant;

public enum ConstantBinaryOperation : byte
{
    Addition,
    Subtraction,
    Multiplication,
    Division,
    Modulo,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    LogicalAnd,
    LogicalOr,
    Equality,
    NotEquals,
    LessThan,
    LessThanOrEquals,
    GreaterThan,
    GreaterThanOrEquals,
    ThreeWayComparison,
    ShiftLeft,
    ShiftRight,
    UnsignedShiftRight,
};

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

    public ConstantValue? TryNegate()
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

    public ConstantValue? TryBitwiseNot()
    {
        return this switch
        {
            IntegerConstant i => ~i,
            _ => null,
        };
    }

    public ConstantValue? TryBinary(ConstantBinaryOperation op, in ConstantValue right)
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
        ConstantBinaryOperation op,
        IntegerConstant left,
        IntegerConstant right
    )
    {
        return op switch
        {
            ConstantBinaryOperation.Addition => FromNullable(left.TryAdd(right)),
            ConstantBinaryOperation.Subtraction => FromNullable(left.TrySubtract(right)),
            ConstantBinaryOperation.Multiplication => FromNullable(left.TryMultiply(right)),
            ConstantBinaryOperation.Division => FromNullable(left.TryDivide(right)),
            ConstantBinaryOperation.Modulo => FromNullable(left.TryMod(right)),
            ConstantBinaryOperation.BitwiseAnd => FromNullable(left.TryBitwiseAnd(left)),
            ConstantBinaryOperation.BitwiseOr => FromNullable(left.TryBitwiseOr(right)),
            ConstantBinaryOperation.BitwiseXor => FromNullable(left.TryBitwiseXor(right)),
            ConstantBinaryOperation.ShiftLeft => FromNullable(left.TryShiftLeft(right)),
            ConstantBinaryOperation.ShiftRight => FromNullable(left.TryShiftRight(right)),
            ConstantBinaryOperation.UnsignedShiftRight => FromNullable(
                left.TryUnsignedShiftRight(right)
            ),
            ConstantBinaryOperation.Equality => FromNullable(left.TryEqual(right)),
            ConstantBinaryOperation.NotEquals => FromNullable(left.TryNotEqual(right)),
            ConstantBinaryOperation.LessThan => FromNullable(left.TryLessThan(right)),
            ConstantBinaryOperation.LessThanOrEquals => FromNullable(
                left.TryLessThanOrEqual(right)
            ),
            ConstantBinaryOperation.GreaterThan => FromNullable(left.TryGreaterThan(right)),
            ConstantBinaryOperation.GreaterThanOrEquals => FromNullable(
                left.TryGreaterThanOrEqual(right)
            ),
            _ => null,
        };
    }

    private static ConstantValue? EvalFloat(
        ConstantBinaryOperation op,
        FloatConstant left,
        FloatConstant right
    )
    {
        return op switch
        {
            ConstantBinaryOperation.Addition => left + right,
            ConstantBinaryOperation.Subtraction => left - right,
            ConstantBinaryOperation.Multiplication => left * right,
            ConstantBinaryOperation.Division => left / right,
            ConstantBinaryOperation.Modulo => left % right,
            ConstantBinaryOperation.Equality => left.Equivalent(right),
            ConstantBinaryOperation.NotEquals => left.NotEquivalent(right),
            ConstantBinaryOperation.LessThan => left.LessThan(right),
            ConstantBinaryOperation.LessThanOrEquals => left.LessThanOrEqual(right),
            ConstantBinaryOperation.GreaterThan => left.GreaterThan(right),
            ConstantBinaryOperation.GreaterThanOrEquals => left.GreaterThanOrEqual(right),
            _ => null,
        };
    }

    private static ConstantValue? EvalCharacter(
        ConstantBinaryOperation op,
        CharacterConstant left,
        CharacterConstant right
    )
    {
        return op switch
        {
            ConstantBinaryOperation.Equality => left.EquivalentTo(right),
            ConstantBinaryOperation.NotEquals => left.NotEquivalentTo(right),
            _ => null,
        };
    }

    private static ConstantValue? EvalBool(ConstantBinaryOperation op, bool left, bool right)
    {
        return op switch
        {
            ConstantBinaryOperation.LogicalAnd => left && right,
            ConstantBinaryOperation.LogicalOr => left || right,
            ConstantBinaryOperation.Equality => left == right,
            ConstantBinaryOperation.NotEquals => left != right,
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
