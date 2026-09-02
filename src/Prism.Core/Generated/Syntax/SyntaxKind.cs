// Generated file, do not edit
using NetEscapades.EnumGenerators;
using Prism.Core.Text;

namespace Prism.Core.Syntax;

[EnumExtensions]
public enum SyntaxKind : ushort
{
    None = 0,
    List = 1,

    #region Trivia
    WhitespaceTrivia = 100,
    NewLineTrivia = 101,
    LineCommentTrivia = 102,
    BlockCommentTrivia = 103,
    #endregion

    #region Structured trivia
    SkippedTokensTrivia = 200,
    #endregion

    #region Keywords
    NamespaceKeyword = 1000,
    UsingKeyword = 1001,
    VarKeyword = 1002,
    FuncKeyword = 1003,
    ExternKeyword = 1004,
    ReturnKeyword = 1005,
    MutableKeyword = 1006,
    AsKeyword = 1007,
    IfKeyword = 1008,
    ElseKeyword = 1009,
    WhileKeyword = 1010,
    LoopKeyword = 1011,
    ForKeyword = 1012,
    BreakKeyword = 1013,
    ContinueKeyword = 1014,
    TrueKeyword = 1015,
    FalseKeyword = 1016,
    VoidKeyword = 1017,
    BoolKeyword = 1018,
    I8Keyword = 1019,
    I16Keyword = 1020,
    I32Keyword = 1021,
    I64Keyword = 1022,
    I128Keyword = 1023,
    U8Keyword = 1024,
    U16Keyword = 1025,
    U32Keyword = 1026,
    U64Keyword = 1027,
    U128Keyword = 1028,
    ISizeKeyword = 1029,
    USizeKeyword = 1030,
    F32Keyword = 1031,
    F64Keyword = 1032,
    CharKeyword = 1033,
    Char16Keyword = 1034,
    RuneKeyword = 1035,
    StrKeyword = 1036,
    #endregion

    #region Punctuations
    OpenBracketToken = 1200,
    CloseBracketToken = 1201,
    OpenParenToken = 1202,
    CloseParenToken = 1203,
    OpenBraceToken = 1204,
    CloseBraceToken = 1205,
    PeriodToken = 1206,
    EllipsisToken = 1207,
    AmpToken = 1208,
    AmpAmpToken = 1209,
    AmpEqualToken = 1210,
    AmpAmpEqualToken = 1211,
    StarToken = 1212,
    StarEqualToken = 1213,
    PlusToken = 1214,
    PlusPlusToken = 1215,
    PlusEqualToken = 1216,
    MinusToken = 1217,
    MinusMinusToken = 1218,
    MinusEqualToken = 1219,
    TildeToken = 1220,
    BangToken = 1221,
    BangEqualToken = 1222,
    SlashToken = 1223,
    SlashEqualToken = 1224,
    PercentToken = 1225,
    PercentEqualToken = 1226,
    LessToken = 1227,
    LessLessToken = 1228,
    LessEqualToken = 1229,
    LessLessEqualToken = 1230,
    SpaceshipToken = 1231,
    GreaterToken = 1232,
    GreaterGreaterToken = 1233,
    GreaterGreaterGreaterToken = 1234,
    GreaterEqualToken = 1235,
    GreaterGreaterEqualToken = 1236,
    GreaterGreaterGreaterEqualToken = 1237,
    CaretToken = 1238,
    CaretEqualToken = 1239,
    PipeToken = 1240,
    PipePipeToken = 1241,
    PipeEqualToken = 1242,
    PipePipeEqualToken = 1243,
    QuestionToken = 1244,
    QuestionQuestionToken = 1245,
    QuestionQuestionEqualToken = 1246,
    QuestionPeriodToken = 1247,
    ColonToken = 1248,
    DoubleColonToken = 1249,
    SemicolonToken = 1250,
    EqualToken = 1251,
    ArrowToken = 1252,
    EqualEqualToken = 1253,
    CommaToken = 1254,
    #endregion

    #region Other tokens
    EofToken = 1300,
    BadToken = 1301,
    IdentifierToken = 1302,
    IntegerLiteralToken = 1303,
    FloatingPointLiteralToken = 1304,
    CharacterLiteralToken = 1305,
    StringLiteralToken = 1306,
    #endregion

    #region Top level
    CompilationUnit = 2000,
    #endregion

    #region Types
    PredefinedType = 3000,
    NamedType = 3001,
    #endregion

    #region Names
    SimpleName = 4000,
    QualifiedName = 4001,
    #endregion

    #region Clauses
    Initializer = 5000,
    TypeSpecifier = 5001,
    NamedParameter = 5002,
    ArgumentList = 5003,
    Argument = 5004,
    ParameterList = 5005,
    Parameter = 5006,
    ExpressionBody = 5007,
    ElseClause = 5008,
    #endregion

    #region Expressions
    LiteralExpression = 6000,
    IdentifierExpression = 6001,
    ParenthesizedExpression = 6002,
    BinaryExpression = 6003,
    AssignmentExpression = 6004,
    PrefixExpression = 6005,
    PostfixExpression = 6006,
    TernaryExpression = 6007,
    InvocationExpression = 6008,
    CastExpression = 6009,
    #endregion

    #region Statements
    EmptyStatement = 7000,
    VariableDeclarationStatement = 7001,
    Block = 7002,
    ReturnStatement = 7003,
    ExpressionStatement = 7004,
    IfStatement = 7005,
    WhileStatement = 7006,
    LoopStatement = 7007,
    ForStatement = 7008,
    BreakStatement = 7009,
    ContinueStatement = 7010,
    LabeledStatement = 7011,
    #endregion

    #region Declarations
    IncompleteDeclaration = 8000,
    BlockNamespaceDeclaration = 8001,
    FileScopedNamespaceDeclaration = 8002,
    VariableDeclaration = 8003,
    FunctionDeclaration = 8004,
    #endregion

    #region Directives
    UsingDirective = 9000,
    #endregion
}

public static partial class SyntaxKindExtensions
{
    extension(SyntaxKind kind)
    {
        public string DisplayText
        {
            get
            {
                return kind switch
                {
                    SyntaxKind.WhitespaceTrivia => "WhitespaceTrivia",
                    SyntaxKind.NewLineTrivia => "NewLineTrivia",
                    SyntaxKind.LineCommentTrivia => "LineCommentTrivia",
                    SyntaxKind.BlockCommentTrivia => "BlockCommentTrivia",
                    SyntaxKind.NamespaceKeyword => "namespace",
                    SyntaxKind.UsingKeyword => "using",
                    SyntaxKind.VarKeyword => "var",
                    SyntaxKind.FuncKeyword => "func",
                    SyntaxKind.ExternKeyword => "extern",
                    SyntaxKind.ReturnKeyword => "return",
                    SyntaxKind.MutableKeyword => "mutable",
                    SyntaxKind.AsKeyword => "as",
                    SyntaxKind.IfKeyword => "if",
                    SyntaxKind.ElseKeyword => "else",
                    SyntaxKind.WhileKeyword => "while",
                    SyntaxKind.LoopKeyword => "loop",
                    SyntaxKind.ForKeyword => "for",
                    SyntaxKind.BreakKeyword => "break",
                    SyntaxKind.ContinueKeyword => "continue",
                    SyntaxKind.TrueKeyword => "true",
                    SyntaxKind.FalseKeyword => "false",
                    SyntaxKind.VoidKeyword => "void",
                    SyntaxKind.BoolKeyword => "bool",
                    SyntaxKind.I8Keyword => "i8",
                    SyntaxKind.I16Keyword => "i16",
                    SyntaxKind.I32Keyword => "i32",
                    SyntaxKind.I64Keyword => "i64",
                    SyntaxKind.I128Keyword => "i128",
                    SyntaxKind.U8Keyword => "u8",
                    SyntaxKind.U16Keyword => "u16",
                    SyntaxKind.U32Keyword => "u32",
                    SyntaxKind.U64Keyword => "u64",
                    SyntaxKind.U128Keyword => "u128",
                    SyntaxKind.ISizeKeyword => "isize",
                    SyntaxKind.USizeKeyword => "usize",
                    SyntaxKind.F32Keyword => "f32",
                    SyntaxKind.F64Keyword => "f64",
                    SyntaxKind.CharKeyword => "char",
                    SyntaxKind.Char16Keyword => "char16",
                    SyntaxKind.RuneKeyword => "rune",
                    SyntaxKind.StrKeyword => "str",
                    SyntaxKind.OpenBracketToken => "[",
                    SyntaxKind.CloseBracketToken => "]",
                    SyntaxKind.OpenParenToken => "(",
                    SyntaxKind.CloseParenToken => ")",
                    SyntaxKind.OpenBraceToken => "{",
                    SyntaxKind.CloseBraceToken => "}",
                    SyntaxKind.PeriodToken => ".",
                    SyntaxKind.EllipsisToken => "...",
                    SyntaxKind.AmpToken => "&",
                    SyntaxKind.AmpAmpToken => "&&",
                    SyntaxKind.AmpEqualToken => "&=",
                    SyntaxKind.AmpAmpEqualToken => "&&=",
                    SyntaxKind.StarToken => "*",
                    SyntaxKind.StarEqualToken => "*=",
                    SyntaxKind.PlusToken => "+",
                    SyntaxKind.PlusPlusToken => "++",
                    SyntaxKind.PlusEqualToken => "+=",
                    SyntaxKind.MinusToken => "-",
                    SyntaxKind.MinusMinusToken => "--",
                    SyntaxKind.MinusEqualToken => "-=",
                    SyntaxKind.TildeToken => "~",
                    SyntaxKind.BangToken => "!",
                    SyntaxKind.BangEqualToken => "!=",
                    SyntaxKind.SlashToken => "/",
                    SyntaxKind.SlashEqualToken => "/=",
                    SyntaxKind.PercentToken => "%",
                    SyntaxKind.PercentEqualToken => "%=",
                    SyntaxKind.LessToken => "<",
                    SyntaxKind.LessLessToken => "<<",
                    SyntaxKind.LessEqualToken => "<=",
                    SyntaxKind.LessLessEqualToken => "<<=",
                    SyntaxKind.SpaceshipToken => "<=>",
                    SyntaxKind.GreaterToken => ">",
                    SyntaxKind.GreaterGreaterToken => ">>",
                    SyntaxKind.GreaterGreaterGreaterToken => ">>>",
                    SyntaxKind.GreaterEqualToken => ">=",
                    SyntaxKind.GreaterGreaterEqualToken => ">>=",
                    SyntaxKind.GreaterGreaterGreaterEqualToken => ">>>=",
                    SyntaxKind.CaretToken => "^",
                    SyntaxKind.CaretEqualToken => "^=",
                    SyntaxKind.PipeToken => "|",
                    SyntaxKind.PipePipeToken => "||",
                    SyntaxKind.PipeEqualToken => "|=",
                    SyntaxKind.PipePipeEqualToken => "||=",
                    SyntaxKind.QuestionToken => "?",
                    SyntaxKind.QuestionQuestionToken => "??",
                    SyntaxKind.QuestionQuestionEqualToken => "??=",
                    SyntaxKind.QuestionPeriodToken => "?.",
                    SyntaxKind.ColonToken => ":",
                    SyntaxKind.DoubleColonToken => "::",
                    SyntaxKind.SemicolonToken => ";",
                    SyntaxKind.EqualToken => "=",
                    SyntaxKind.ArrowToken => "=>",
                    SyntaxKind.EqualEqualToken => "==",
                    SyntaxKind.CommaToken => ",",
                    SyntaxKind.EofToken => "EOF",
                    SyntaxKind.BadToken => "<unrecognized>",
                    SyntaxKind.IdentifierToken => "IdentifierToken",
                    SyntaxKind.IntegerLiteralToken => "IntegerLiteralToken",
                    SyntaxKind.FloatingPointLiteralToken => "FloatingPointLiteralToken",
                    SyntaxKind.CharacterLiteralToken => "CharacterLiteralToken",
                    SyntaxKind.StringLiteralToken => "StringLiteralToken",
                    SyntaxKind.SkippedTokensTrivia => "SkippedTokensTrivia",
                    SyntaxKind.CompilationUnit => "CompilationUnit",
                    SyntaxKind.PredefinedType => "PredefinedType",
                    SyntaxKind.NamedType => "NamedType",
                    SyntaxKind.SimpleName => "SimpleName",
                    SyntaxKind.QualifiedName => "QualifiedName",
                    SyntaxKind.Initializer => "Initializer",
                    SyntaxKind.TypeSpecifier => "TypeSpecifier",
                    SyntaxKind.NamedParameter => "NamedParameter",
                    SyntaxKind.ArgumentList => "ArgumentList",
                    SyntaxKind.Argument => "Argument",
                    SyntaxKind.ParameterList => "ParameterList",
                    SyntaxKind.Parameter => "Parameter",
                    SyntaxKind.ExpressionBody => "ExpressionBody",
                    SyntaxKind.ElseClause => "ElseClause",
                    SyntaxKind.LiteralExpression => "LiteralExpression",
                    SyntaxKind.IdentifierExpression => "IdentifierExpression",
                    SyntaxKind.ParenthesizedExpression => "ParenthesizedExpression",
                    SyntaxKind.BinaryExpression => "BinaryExpression",
                    SyntaxKind.AssignmentExpression => "AssignmentExpression",
                    SyntaxKind.PrefixExpression => "PrefixExpression",
                    SyntaxKind.PostfixExpression => "PostfixExpression",
                    SyntaxKind.TernaryExpression => "TernaryExpression",
                    SyntaxKind.InvocationExpression => "InvocationExpression",
                    SyntaxKind.CastExpression => "CastExpression",
                    SyntaxKind.EmptyStatement => "EmptyStatement",
                    SyntaxKind.VariableDeclarationStatement => "VariableDeclarationStatement",
                    SyntaxKind.Block => "Block",
                    SyntaxKind.ReturnStatement => "ReturnStatement",
                    SyntaxKind.ExpressionStatement => "ExpressionStatement",
                    SyntaxKind.IfStatement => "IfStatement",
                    SyntaxKind.WhileStatement => "WhileStatement",
                    SyntaxKind.LoopStatement => "LoopStatement",
                    SyntaxKind.ForStatement => "ForStatement",
                    SyntaxKind.BreakStatement => "BreakStatement",
                    SyntaxKind.ContinueStatement => "ContinueStatement",
                    SyntaxKind.LabeledStatement => "LabeledStatement",
                    SyntaxKind.IncompleteDeclaration => "IncompleteDeclaration",
                    SyntaxKind.BlockNamespaceDeclaration => "BlockNamespaceDeclaration",
                    SyntaxKind.FileScopedNamespaceDeclaration => "FileScopedNamespaceDeclaration",
                    SyntaxKind.VariableDeclaration => "VariableDeclaration",
                    SyntaxKind.FunctionDeclaration => "FunctionDeclaration",
                    SyntaxKind.UsingDirective => "UsingDirective",
                    _ => throw new ArgumentException("Unknown SyntaxKind", nameof(kind)),
                };
            }
        }

        public static SyntaxKind TriviaStart => (SyntaxKind)100;
        public static SyntaxKind TriviaEnd => (SyntaxKind)103;

        public bool IsTrivia => (ushort)kind >= 100 && (ushort)kind <= 103;

        public static SyntaxKind StructuredTriviumStart => (SyntaxKind)200;
        public static SyntaxKind StructuredTriviumEnd => (SyntaxKind)200;

        public bool IsStructuredTrivium => (ushort)kind >= 200 && (ushort)kind <= 200;

        public static SyntaxKind KeywordStart => (SyntaxKind)1000;
        public static SyntaxKind KeywordEnd => (SyntaxKind)1036;

        public bool IsKeyword => (ushort)kind >= 1000 && (ushort)kind <= 1036;

        public static SyntaxKind PunctuationStart => (SyntaxKind)1200;
        public static SyntaxKind PunctuationEnd => (SyntaxKind)1254;

        public bool IsPunctuation => (ushort)kind >= 1200 && (ushort)kind <= 1254;

        public static SyntaxKind OtherTokenStart => (SyntaxKind)1300;
        public static SyntaxKind OtherTokenEnd => (SyntaxKind)1306;

        public bool IsOtherToken => (ushort)kind >= 1300 && (ushort)kind <= 1306;

        public static SyntaxKind TopLevelStart => (SyntaxKind)2000;
        public static SyntaxKind TopLevelEnd => (SyntaxKind)2000;

        public bool IsTopLevel => (ushort)kind >= 2000 && (ushort)kind <= 2000;

        public static SyntaxKind TypeStart => (SyntaxKind)3000;
        public static SyntaxKind TypeEnd => (SyntaxKind)3001;

        public bool IsType => (ushort)kind >= 3000 && (ushort)kind <= 3001;

        public static SyntaxKind NameStart => (SyntaxKind)4000;
        public static SyntaxKind NameEnd => (SyntaxKind)4001;

        public bool IsName => (ushort)kind >= 4000 && (ushort)kind <= 4001;

        public static SyntaxKind ClauseStart => (SyntaxKind)5000;
        public static SyntaxKind ClauseEnd => (SyntaxKind)5008;

        public bool IsClause => (ushort)kind >= 5000 && (ushort)kind <= 5008;

        public static SyntaxKind ExpressionStart => (SyntaxKind)6000;
        public static SyntaxKind ExpressionEnd => (SyntaxKind)6009;

        public bool IsExpression => (ushort)kind >= 6000 && (ushort)kind <= 6009;

        public static SyntaxKind StatementStart => (SyntaxKind)7000;
        public static SyntaxKind StatementEnd => (SyntaxKind)7011;

        public bool IsStatement => (ushort)kind >= 7000 && (ushort)kind <= 7011;

        public static SyntaxKind DeclarationStart => (SyntaxKind)8000;
        public static SyntaxKind DeclarationEnd => (SyntaxKind)8004;

        public bool IsDeclaration => (ushort)kind >= 8000 && (ushort)kind <= 8004;

        public static SyntaxKind DirectiveStart => (SyntaxKind)9000;
        public static SyntaxKind DirectiveEnd => (SyntaxKind)9000;

        public bool IsDirective => (ushort)kind >= 9000 && (ushort)kind <= 9000;

        public bool IsStructuredTrivia => kind.IsStructuredTrivium;

        public bool IsToken => kind.IsKeyword || kind.IsPunctuation || kind.IsOtherToken;

        public bool IsNode =>
            kind.IsTopLevel
            || kind.IsType
            || kind.IsName
            || kind.IsClause
            || kind.IsExpression
            || kind.IsStatement
            || kind.IsDeclaration
            || kind.IsDirective;

        internal static SyntaxKind? MatchKeyword(scoped ReadOnlySpan<char> text)
        {
            switch (text.Length)
            {
                case 2:
                    if (text.Equals("as", StringComparison.Ordinal))
                        return SyntaxKind.AsKeyword;
                    if (text.Equals("i8", StringComparison.Ordinal))
                        return SyntaxKind.I8Keyword;
                    if (text.Equals("if", StringComparison.Ordinal))
                        return SyntaxKind.IfKeyword;
                    if (text.Equals("u8", StringComparison.Ordinal))
                        return SyntaxKind.U8Keyword;
                    break;
                case 3:
                    if (text.Equals("f32", StringComparison.Ordinal))
                        return SyntaxKind.F32Keyword;
                    if (text.Equals("f64", StringComparison.Ordinal))
                        return SyntaxKind.F64Keyword;
                    if (text.Equals("for", StringComparison.Ordinal))
                        return SyntaxKind.ForKeyword;
                    if (text.Equals("i16", StringComparison.Ordinal))
                        return SyntaxKind.I16Keyword;
                    if (text.Equals("i32", StringComparison.Ordinal))
                        return SyntaxKind.I32Keyword;
                    if (text.Equals("i64", StringComparison.Ordinal))
                        return SyntaxKind.I64Keyword;
                    if (text.Equals("str", StringComparison.Ordinal))
                        return SyntaxKind.StrKeyword;
                    if (text.Equals("u16", StringComparison.Ordinal))
                        return SyntaxKind.U16Keyword;
                    if (text.Equals("u32", StringComparison.Ordinal))
                        return SyntaxKind.U32Keyword;
                    if (text.Equals("u64", StringComparison.Ordinal))
                        return SyntaxKind.U64Keyword;
                    if (text.Equals("var", StringComparison.Ordinal))
                        return SyntaxKind.VarKeyword;
                    break;
                case 4:
                    if (text.Equals("bool", StringComparison.Ordinal))
                        return SyntaxKind.BoolKeyword;
                    if (text.Equals("char", StringComparison.Ordinal))
                        return SyntaxKind.CharKeyword;
                    if (text.Equals("else", StringComparison.Ordinal))
                        return SyntaxKind.ElseKeyword;
                    if (text.Equals("func", StringComparison.Ordinal))
                        return SyntaxKind.FuncKeyword;
                    if (text.Equals("i128", StringComparison.Ordinal))
                        return SyntaxKind.I128Keyword;
                    if (text.Equals("loop", StringComparison.Ordinal))
                        return SyntaxKind.LoopKeyword;
                    if (text.Equals("rune", StringComparison.Ordinal))
                        return SyntaxKind.RuneKeyword;
                    if (text.Equals("true", StringComparison.Ordinal))
                        return SyntaxKind.TrueKeyword;
                    if (text.Equals("u128", StringComparison.Ordinal))
                        return SyntaxKind.U128Keyword;
                    if (text.Equals("void", StringComparison.Ordinal))
                        return SyntaxKind.VoidKeyword;
                    break;
                case 5:
                    if (text.Equals("break", StringComparison.Ordinal))
                        return SyntaxKind.BreakKeyword;
                    if (text.Equals("false", StringComparison.Ordinal))
                        return SyntaxKind.FalseKeyword;
                    if (text.Equals("isize", StringComparison.Ordinal))
                        return SyntaxKind.ISizeKeyword;
                    if (text.Equals("using", StringComparison.Ordinal))
                        return SyntaxKind.UsingKeyword;
                    if (text.Equals("usize", StringComparison.Ordinal))
                        return SyntaxKind.USizeKeyword;
                    if (text.Equals("while", StringComparison.Ordinal))
                        return SyntaxKind.WhileKeyword;
                    break;
                case 6:
                    if (text.Equals("char16", StringComparison.Ordinal))
                        return SyntaxKind.Char16Keyword;
                    if (text.Equals("extern", StringComparison.Ordinal))
                        return SyntaxKind.ExternKeyword;
                    if (text.Equals("return", StringComparison.Ordinal))
                        return SyntaxKind.ReturnKeyword;
                    break;
                case 7:
                    if (text.Equals("mutable", StringComparison.Ordinal))
                        return SyntaxKind.MutableKeyword;
                    break;
                case 8:
                    if (text.Equals("continue", StringComparison.Ordinal))
                        return SyntaxKind.ContinueKeyword;
                    break;
                case 9:
                    if (text.Equals("namespace", StringComparison.Ordinal))
                        return SyntaxKind.NamespaceKeyword;
                    break;
            }
            return null;
        }

        internal static SyntaxKind? MatchPunctuation(TextCursor cursor)
        {
            switch (cursor.Current)
            {
                case '!':
                    cursor.Advance();
                    if (cursor.Current == '=')
                    {
                        cursor.Advance();
                        return SyntaxKind.BangEqualToken;
                    }
                    return SyntaxKind.BangToken;
                case '%':
                    cursor.Advance();
                    if (cursor.Current == '=')
                    {
                        cursor.Advance();
                        return SyntaxKind.PercentEqualToken;
                    }
                    return SyntaxKind.PercentToken;
                case '&':
                    cursor.Advance();
                    switch (cursor.Current)
                    {
                        case '&':
                            cursor.Advance();
                            if (cursor.Current == '=')
                            {
                                cursor.Advance();
                                return SyntaxKind.AmpAmpEqualToken;
                            }
                            return SyntaxKind.AmpAmpToken;
                        case '=':
                            cursor.Advance();
                            return SyntaxKind.AmpEqualToken;
                    }
                    return SyntaxKind.AmpToken;
                case '(':
                    cursor.Advance();
                    return SyntaxKind.OpenParenToken;
                case ')':
                    cursor.Advance();
                    return SyntaxKind.CloseParenToken;
                case '*':
                    cursor.Advance();
                    if (cursor.Current == '=')
                    {
                        cursor.Advance();
                        return SyntaxKind.StarEqualToken;
                    }
                    return SyntaxKind.StarToken;
                case '+':
                    cursor.Advance();
                    switch (cursor.Current)
                    {
                        case '+':
                            cursor.Advance();
                            return SyntaxKind.PlusPlusToken;
                        case '=':
                            cursor.Advance();
                            return SyntaxKind.PlusEqualToken;
                    }
                    return SyntaxKind.PlusToken;
                case ',':
                    cursor.Advance();
                    return SyntaxKind.CommaToken;
                case '-':
                    cursor.Advance();
                    switch (cursor.Current)
                    {
                        case '-':
                            cursor.Advance();
                            return SyntaxKind.MinusMinusToken;
                        case '=':
                            cursor.Advance();
                            return SyntaxKind.MinusEqualToken;
                    }
                    return SyntaxKind.MinusToken;
                case '.':
                    cursor.Advance();
                    if (cursor.Current == '.')
                    {
                        cursor.Advance();
                        if (cursor.Current == '.')
                        {
                            cursor.Advance();
                            return SyntaxKind.EllipsisToken;
                        }
                    }
                    return SyntaxKind.PeriodToken;
                case '/':
                    cursor.Advance();
                    if (cursor.Current == '=')
                    {
                        cursor.Advance();
                        return SyntaxKind.SlashEqualToken;
                    }
                    return SyntaxKind.SlashToken;
                case ':':
                    cursor.Advance();
                    if (cursor.Current == ':')
                    {
                        cursor.Advance();
                        return SyntaxKind.DoubleColonToken;
                    }
                    return SyntaxKind.ColonToken;
                case ';':
                    cursor.Advance();
                    return SyntaxKind.SemicolonToken;
                case '<':
                    cursor.Advance();
                    switch (cursor.Current)
                    {
                        case '<':
                            cursor.Advance();
                            if (cursor.Current == '=')
                            {
                                cursor.Advance();
                                return SyntaxKind.LessLessEqualToken;
                            }
                            return SyntaxKind.LessLessToken;
                        case '=':
                            cursor.Advance();
                            if (cursor.Current == '>')
                            {
                                cursor.Advance();
                                return SyntaxKind.SpaceshipToken;
                            }
                            return SyntaxKind.LessEqualToken;
                    }
                    return SyntaxKind.LessToken;
                case '=':
                    cursor.Advance();
                    switch (cursor.Current)
                    {
                        case '=':
                            cursor.Advance();
                            return SyntaxKind.EqualEqualToken;
                        case '>':
                            cursor.Advance();
                            return SyntaxKind.ArrowToken;
                    }
                    return SyntaxKind.EqualToken;
                case '>':
                    cursor.Advance();
                    switch (cursor.Current)
                    {
                        case '=':
                            cursor.Advance();
                            return SyntaxKind.GreaterEqualToken;
                        case '>':
                            cursor.Advance();
                            switch (cursor.Current)
                            {
                                case '=':
                                    cursor.Advance();
                                    return SyntaxKind.GreaterGreaterEqualToken;
                                case '>':
                                    cursor.Advance();
                                    if (cursor.Current == '=')
                                    {
                                        cursor.Advance();
                                        return SyntaxKind.GreaterGreaterGreaterEqualToken;
                                    }
                                    return SyntaxKind.GreaterGreaterGreaterToken;
                            }
                            return SyntaxKind.GreaterGreaterToken;
                    }
                    return SyntaxKind.GreaterToken;
                case '?':
                    cursor.Advance();
                    switch (cursor.Current)
                    {
                        case '.':
                            cursor.Advance();
                            return SyntaxKind.QuestionPeriodToken;
                        case '?':
                            cursor.Advance();
                            if (cursor.Current == '=')
                            {
                                cursor.Advance();
                                return SyntaxKind.QuestionQuestionEqualToken;
                            }
                            return SyntaxKind.QuestionQuestionToken;
                    }
                    return SyntaxKind.QuestionToken;
                case '[':
                    cursor.Advance();
                    return SyntaxKind.OpenBracketToken;
                case ']':
                    cursor.Advance();
                    return SyntaxKind.CloseBracketToken;
                case '^':
                    cursor.Advance();
                    if (cursor.Current == '=')
                    {
                        cursor.Advance();
                        return SyntaxKind.CaretEqualToken;
                    }
                    return SyntaxKind.CaretToken;
                case '{':
                    cursor.Advance();
                    return SyntaxKind.OpenBraceToken;
                case '|':
                    cursor.Advance();
                    switch (cursor.Current)
                    {
                        case '=':
                            cursor.Advance();
                            return SyntaxKind.PipeEqualToken;
                        case '|':
                            cursor.Advance();
                            if (cursor.Current == '=')
                            {
                                cursor.Advance();
                                return SyntaxKind.PipePipeEqualToken;
                            }
                            return SyntaxKind.PipePipeToken;
                    }
                    return SyntaxKind.PipeToken;
                case '}':
                    cursor.Advance();
                    return SyntaxKind.CloseBraceToken;
                case '~':
                    cursor.Advance();
                    return SyntaxKind.TildeToken;
            }

            return null;
        }
    }
}
