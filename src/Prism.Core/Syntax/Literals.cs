using System.Numerics;
using System.Text;
using Singulink.Numerics;

namespace Prism.Core.Syntax;

public interface ISyntaxData
{
    SyntaxKind Kind { get; }
}

public readonly record struct BoolData(bool Value) : ISyntaxData
{
    public SyntaxKind Kind => Value ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword;

    public static BoolData True => new(true);
    public static BoolData False => new(false);
}

public enum IntegerBase : byte
{
    Decimal,
    Hex,
    Binary,
}

public enum IntegerSuffix : byte
{
    None,
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
}

public readonly record struct IntegerDataValue(
    BigInteger Value,
    IntegerBase Base,
    IntegerSuffix Suffix
) : ISyntaxData
{
    public SyntaxKind Kind => SyntaxKind.IntegerLiteralToken;
}

public enum FloatSuffix : byte
{
    None,
    F16,
    F32,
    F64,
}

public readonly record struct FloatDataValue(BigDecimal Value, FloatSuffix Suffix) : ISyntaxData
{
    public SyntaxKind Kind => SyntaxKind.FloatingPointLiteralToken;
}

public enum CharacterEncoding : byte
{
    Utf8,
    Utf16,
    Utf32,
}

public readonly record struct CharacterDataValue(Rune Value, CharacterEncoding Encoding)
    : ISyntaxData
{
    public SyntaxKind Kind => SyntaxKind.CharacterLiteralToken;
}

public readonly record struct StringDataValue(string Value, CharacterEncoding Encoding)
    : ISyntaxData
{
    public SyntaxKind Kind => SyntaxKind.StringLiteralToken;
}
