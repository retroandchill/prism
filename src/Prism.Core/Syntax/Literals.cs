using System.Numerics;
using System.Text;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public interface ISyntaxData<TSelf>
    where TSelf : struct, ISyntaxData<TSelf>
{
    SyntaxKind Kind { get; }
    string Text { get; }

    internal static virtual TSelf? TryGetFrom(GreenToken token)
    {
        return token is GreenValueToken<TSelf> valueToken ? valueToken.Value : null;
    }
}

public readonly record struct BoolLiteralData(bool Value) : ISyntaxData<BoolLiteralData>
{
    public SyntaxKind Kind => Value ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword;

    public string Text => Value ? "true" : "false";

    private static BoolLiteralData True => new(true);
    private static BoolLiteralData False => new(false);

    static BoolLiteralData? ISyntaxData<BoolLiteralData>.TryGetFrom(GreenToken token)
    {
        return token.Kind switch
        {
            SyntaxKind.TrueKeyword => True,
            SyntaxKind.FalseKeyword => False,
            _ => null,
        };
    }
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
) : ISyntaxData<IntegerLiteralData>
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
) : ISyntaxData<FloatLiteralData>
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
) : ISyntaxData<CharacterLiteralData>
{
    public SyntaxKind Kind => SyntaxKind.CharacterLiteralToken;
}

public readonly record struct StringLiteralData(
    string Text,
    string Value,
    CharacterEncoding Encoding = CharacterEncoding.Utf8
) : ISyntaxData<StringLiteralData>
{
    public SyntaxKind Kind => SyntaxKind.StringLiteralToken;
}

public readonly record struct IdentifierData : ISyntaxData<IdentifierData>
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
