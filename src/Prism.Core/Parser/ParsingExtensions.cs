using Prism.Core.Syntax;

namespace Prism.Core.Parser;

internal static class ParsingExtensions
{
    extension(SyntaxKind kind)
    {
        public bool IsModifier => kind is SyntaxKind.ExternKeyword or SyntaxKind.MutableKeyword;

        public bool IsBuiltInType =>
            kind
                is SyntaxKind.VoidKeyword
                    or SyntaxKind.BoolKeyword
                    or SyntaxKind.I8Keyword
                    or SyntaxKind.I16Keyword
                    or SyntaxKind.I32Keyword
                    or SyntaxKind.I64Keyword
                    or SyntaxKind.I128Keyword
                    or SyntaxKind.ISizeKeyword
                    or SyntaxKind.U8Keyword
                    or SyntaxKind.U16Keyword
                    or SyntaxKind.U32Keyword
                    or SyntaxKind.U64Keyword
                    or SyntaxKind.U128Keyword
                    or SyntaxKind.USizeKeyword
                    or SyntaxKind.F32Keyword
                    or SyntaxKind.F64Keyword
                    or SyntaxKind.CharKeyword
                    or SyntaxKind.Char16Keyword
                    or SyntaxKind.RuneKeyword
                    or SyntaxKind.StrKeyword;

        public bool IsAssignmentOperator =>
            kind
                is SyntaxKind.EqualToken
                    or SyntaxKind.PlusEqualToken
                    or SyntaxKind.MinusEqualToken
                    or SyntaxKind.StarEqualToken
                    or SyntaxKind.SlashEqualToken
                    or SyntaxKind.PercentEqualToken
                    or SyntaxKind.AmpEqualToken
                    or SyntaxKind.CaretEqualToken
                    or SyntaxKind.PipeEqualToken
                    or SyntaxKind.AmpAmpEqualToken
                    or SyntaxKind.PipePipeEqualToken
                    or SyntaxKind.LessLessEqualToken
                    or SyntaxKind.GreaterGreaterEqualToken
                    or SyntaxKind.GreaterGreaterGreaterEqualToken;

        public bool IsPrefixOperator =>
            kind
                is SyntaxKind.PlusToken
                    or SyntaxKind.MinusToken
                    or SyntaxKind.BangToken
                    or SyntaxKind.TildeToken
                    or SyntaxKind.PlusPlusToken
                    or SyntaxKind.MinusMinusToken
                    or SyntaxKind.AmpToken
                    or SyntaxKind.StarToken;

        public bool IsPostfixOperator =>
            kind is SyntaxKind.PlusPlusToken or SyntaxKind.MinusMinusToken;

        public int OperatorPrecedence
        {
            get
            {
                return kind switch
                {
                    SyntaxKind.StarToken or SyntaxKind.SlashToken or SyntaxKind.PercentToken => 140,
                    SyntaxKind.PlusToken or SyntaxKind.MinusToken => 130,
                    SyntaxKind.LessLessToken
                    or SyntaxKind.GreaterGreaterToken
                    or SyntaxKind.GreaterGreaterGreaterToken => 120,
                    SyntaxKind.LessToken
                    or SyntaxKind.GreaterToken
                    or SyntaxKind.LessEqualToken
                    or SyntaxKind.GreaterEqualToken => 110,
                    SyntaxKind.SpaceshipToken => 100,
                    SyntaxKind.EqualEqualToken or SyntaxKind.BangEqualToken => 90,
                    SyntaxKind.AmpToken => 80,
                    SyntaxKind.CaretToken => 70,
                    SyntaxKind.PipeToken => 60,
                    SyntaxKind.AmpAmpToken => 50,
                    SyntaxKind.PipePipeToken => 40,
                    SyntaxKind.QuestionQuestionToken => 30,
                    SyntaxKind.QuestionToken => 20,
                    SyntaxKind.EqualToken
                    or SyntaxKind.PlusEqualToken
                    or SyntaxKind.MinusEqualToken
                    or SyntaxKind.StarEqualToken
                    or SyntaxKind.SlashEqualToken
                    or SyntaxKind.PercentEqualToken
                    or SyntaxKind.LessLessEqualToken
                    or SyntaxKind.GreaterGreaterEqualToken
                    or SyntaxKind.GreaterGreaterGreaterEqualToken
                    or SyntaxKind.AmpEqualToken
                    or SyntaxKind.PipeEqualToken
                    or SyntaxKind.CaretEqualToken
                    or SyntaxKind.AmpAmpEqualToken
                    or SyntaxKind.PipePipeEqualToken => 10,
                    _ => -1,
                };
            }
        }
    }
}
