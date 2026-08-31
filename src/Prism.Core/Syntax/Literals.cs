using System.Numerics;
using System.Text;
using Singulink.Numerics;

namespace Prism.Core.Syntax;

public interface ISyntaxData
{
    SyntaxKind Kind { get; }
    string Text { get; }
}

public readonly record struct BoolLiteralData(bool Value) : ISyntaxData
{
    public SyntaxKind Kind => Value ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword;

    public string Text => Value ? "true" : "false";

    public static BoolLiteralData True => new(true);
    public static BoolLiteralData False => new(false);
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

public readonly record struct IntegerLiteralData(
    string Text,
    BigInteger Value,
    IntegerBase Base = IntegerBase.Decimal,
    IntegerSuffix Suffix = IntegerSuffix.None
) : ISyntaxData
{
    public SyntaxKind Kind => SyntaxKind.IntegerLiteralToken;
}

public enum FloatSuffix : byte
{
    None,
    F32,
    F64,
}

public readonly record struct FloatLiteralData(
    string Text,
    BigInteger Significand,
    int Exponent10 = 0,
    FloatSuffix Suffix = FloatSuffix.None
) : ISyntaxData
{
    public SyntaxKind Kind => SyntaxKind.FloatingPointLiteralToken;
}

public enum CharacterEncoding : byte
{
    Utf8,
    Utf16,
    Utf32,
}

public readonly record struct CharacterLiteralData(
    string Text,
    Rune Value,
    CharacterEncoding Encoding = CharacterEncoding.Utf8
) : ISyntaxData
{
    public SyntaxKind Kind => SyntaxKind.CharacterLiteralToken;
}

public readonly record struct StringLiteralData(
    string Text,
    string Value,
    CharacterEncoding Encoding = CharacterEncoding.Utf8
) : ISyntaxData
{
    public SyntaxKind Kind => SyntaxKind.StringLiteralToken;
}

public readonly record struct IdentifierData : ISyntaxData
{
    public string Text { get; }
    public string Value { get; }
    public bool IsEscaped { get; }
    public SyntaxKind Kind => SyntaxKind.IdentifierToken;

    public IdentifierData(string text)
    {
        Text = text;
        IsEscaped = text.StartsWith('@');
        Value = IsEscaped ? text[1..] : text;
    }
}
