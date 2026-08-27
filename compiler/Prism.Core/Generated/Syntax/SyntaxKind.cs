// Generated file, do not edit
namespace Prism.Core.Syntax;

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
    MutKeyword = 1006,
    TrueKeyword = 1007,
    FalseKeyword = 1008,
    VoidKeyword = 1009,
    BoolKeyword = 1010,
    I8Keyword = 1011,
    I16Keyword = 1012,
    I32Keyword = 1013,
    I64Keyword = 1014,
    I128Keyword = 1015,
    U8Keyword = 1016,
    U16Keyword = 1017,
    U32Keyword = 1018,
    U64Keyword = 1019,
    U128Keyword = 1020,
    IsizeKeyword = 1021,
    UsizeKeyword = 1022,
    F16Keyword = 1023,
    F32Keyword = 1024,
    F64Keyword = 1025,
    CharKeyword = 1026,
    Char16Keyword = 1027,
    RuneKeyword = 1028,
    StrKeyword = 1029,
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
    StarToken = 1211,
    StarEqualToken = 1212,
    PlusToken = 1213,
    PlusPlusToken = 1214,
    PlusEqualToken = 1215,
    MinusToken = 1216,
    MinusMinusToken = 1217,
    MinusEqualToken = 1218,
    TildeToken = 1219,
    BangToken = 1220,
    BangEqualToken = 1221,
    SlashToken = 1222,
    SlashEqualToken = 1223,
    PercentToken = 1224,
    PercentEqualToken = 1225,
    LessToken = 1226,
    LessLessToken = 1227,
    LessEqualToken = 1228,
    LessLessEqualToken = 1229,
    SpaceshipToken = 1230,
    GreaterToken = 1231,
    GreaterGreaterToken = 1232,
    GreaterGreaterGreaterToken = 1233,
    GreaterEqualToken = 1234,
    GreaterGreaterEqualToken = 1235,
    GreaterGreaterGreaterEqualToken = 1236,
    CaretToken = 1237,
    CaretEqualToken = 1238,
    PipeToken = 1239,
    PipePipeToken = 1240,
    PipeEqualToken = 1241,
    QuestionToken = 1242,
    QuestionQuestionToken = 1243,
    QuestionQuestionEqualToken = 1244,
    QuestionPeriodToken = 1245,
    ColonToken = 1246,
    DoubleColonToken = 1247,
    SemicolonToken = 1248,
    EqualToken = 1249,
    ArrowToken = 1250,
    EqualEqualToken = 1251,
    CommaToken = 1252,
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
    #endregion

    #region Statements
    VariableDeclarationStatement = 7000,
    Block = 7001,
    ReturnStatement = 7002,
    ExpressionStatement = 7003,
    EmptyStatement = 7004,
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

public static class SyntaxKindExtensions
{
    extension(SyntaxKind kind)
    {
        public string Name
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
                    SyntaxKind.MutKeyword => "mut",
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
                    SyntaxKind.IsizeKeyword => "isize",
                    SyntaxKind.UsizeKeyword => "usize",
                    SyntaxKind.F16Keyword => "f16",
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
                    SyntaxKind.LiteralExpression => "LiteralExpression",
                    SyntaxKind.IdentifierExpression => "IdentifierExpression",
                    SyntaxKind.ParenthesizedExpression => "ParenthesizedExpression",
                    SyntaxKind.BinaryExpression => "BinaryExpression",
                    SyntaxKind.AssignmentExpression => "AssignmentExpression",
                    SyntaxKind.PrefixExpression => "PrefixExpression",
                    SyntaxKind.PostfixExpression => "PostfixExpression",
                    SyntaxKind.TernaryExpression => "TernaryExpression",
                    SyntaxKind.InvocationExpression => "InvocationExpression",
                    SyntaxKind.VariableDeclarationStatement => "VariableDeclarationStatement",
                    SyntaxKind.Block => "Block",
                    SyntaxKind.ReturnStatement => "ReturnStatement",
                    SyntaxKind.ExpressionStatement => "ExpressionStatement",
                    SyntaxKind.EmptyStatement => "EmptyStatement",
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

        public bool IsTrivia => (ushort)kind >= 100 && (ushort)kind <= 103;

        public bool IsStructuredTrivium => (ushort)kind >= 200 && (ushort)kind <= 200;

        public bool IsKeyword => (ushort)kind >= 1000 && (ushort)kind <= 1029;

        public bool IsPunctuation => (ushort)kind >= 1200 && (ushort)kind <= 1252;

        public bool IsOtherToken => (ushort)kind >= 1300 && (ushort)kind <= 1306;

        public bool IsTopLevel => (ushort)kind >= 2000 && (ushort)kind <= 2000;

        public bool IsType => (ushort)kind >= 3000 && (ushort)kind <= 3001;

        public bool IsName => (ushort)kind >= 4000 && (ushort)kind <= 4001;

        public bool IsClause => (ushort)kind >= 5000 && (ushort)kind <= 5007;

        public bool IsExpression => (ushort)kind >= 6000 && (ushort)kind <= 6008;

        public bool IsStatement => (ushort)kind >= 7000 && (ushort)kind <= 7004;

        public bool IsDeclaration => (ushort)kind >= 8000 && (ushort)kind <= 8004;

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
    }
}
