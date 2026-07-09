using System.Numerics;
using System.Text;
using Singulink.Numerics;

namespace Prism.Core.Syntax;

public interface ISyntaxLiteral
{
    SyntaxKind Kind { get; }
}

public readonly record struct BoolLiteral(bool Value) : ISyntaxLiteral
{
    public SyntaxKind Kind => Value ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword;

    public static BoolLiteral True => new(true);
    public static BoolLiteral False => new(false);
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

public readonly record struct IntegerLiteralValue(
    BigInteger Value,
    IntegerBase Base,
    IntegerSuffix Suffix
) : ISyntaxLiteral
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

public readonly record struct FloatLiteralValue(BigDecimal Value, FloatSuffix Suffix)
    : ISyntaxLiteral
{
    public SyntaxKind Kind => SyntaxKind.FloatingPointLiteralToken;
}

public enum CharacterEncoding : byte
{
    Utf8,
    Utf16,
    Utf32,
}

public readonly record struct CharacterLiteralValue(Rune Value, CharacterEncoding Encoding)
    : ISyntaxLiteral
{
    public SyntaxKind Kind => SyntaxKind.CharacterLiteralToken;
}

public readonly record struct StringLiteralValue(string Value, CharacterEncoding Encoding)
    : ISyntaxLiteral
{
    public SyntaxKind Kind => SyntaxKind.StringLiteralToken;
}
