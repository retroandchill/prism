export module prism.core:syntax.kind;

import std;

namespace prism
{
    export enum class SyntaxKind : std::uint16_t
    {
        none = 0,
        list = 1,

#pragma region Trivia
        trivia_start = 100,
        whitespace_trivia = 100,
        new_line_trivia = 101,
        line_comment_trivia = 102,
        block_comment_trivia = 103,
        trivia_end = 103,
#pragma endregion

#pragma region Keywords
        keyword_start = 200,
        var_keyword = 200,
        func_keyword = 201,
        extern_keyword = 202,
        return_keyword = 203,
        mut_keyword = 204,
        true_keyword = 205,
        false_keyword = 206,
        void_keyword = 207,
        bool_keyword = 208,
        i8_keyword = 209,
        i16_keyword = 210,
        i32_keyword = 211,
        i64_keyword = 212,
        i128_keyword = 213,
        u8_keyword = 214,
        u16_keyword = 215,
        u32_keyword = 216,
        u64_keyword = 217,
        u128_keyword = 218,
        isize_keyword = 219,
        usize_keyword = 220,
        f16_keyword = 221,
        f32_keyword = 222,
        f64_keyword = 223,
        char_keyword = 224,
        char16_keyword = 225,
        rune_keyword = 226,
        str_keyword = 227,
        keyword_end = 227,
#pragma endregion

#pragma region Punctuations
        punctuation_start = 300,
        open_bracket_token = 300,
        close_bracket_token = 301,
        open_paren_token = 302,
        close_paren_token = 303,
        open_brace_token = 304,
        close_brace_token = 305,
        period_token = 306,
        ellipsis_token = 307,
        amp_token = 308,
        amp_amp_token = 309,
        amp_equal_token = 310,
        star_token = 311,
        star_equal_token = 312,
        plus_token = 313,
        plus_plus_token = 314,
        plus_equal_token = 315,
        minus_token = 316,
        minus_minus_token = 317,
        minus_equal_token = 318,
        tilde_token = 319,
        bang_token = 320,
        bang_equal_token = 321,
        slash_token = 322,
        slash_equal_token = 323,
        percent_token = 324,
        percent_equal_token = 325,
        less_token = 326,
        less_less_token = 327,
        less_equal_token = 328,
        less_less_equal_token = 329,
        spaceship_token = 330,
        greater_token = 331,
        greater_greater_token = 332,
        greater_greater_greater_token = 333,
        greater_equal_token = 334,
        greater_greater_equal_token = 335,
        greater_greater_greater_equal_token = 336,
        caret_token = 337,
        caret_equal_token = 338,
        pipe_token = 339,
        pipe_pipe_token = 340,
        pipe_equal_token = 341,
        question_token = 342,
        question_question_token = 343,
        question_question_equal_token = 344,
        question_period_token = 345,
        colon_token = 346,
        double_colon_token = 347,
        semicolon_token = 348,
        equal_token = 349,
        arrow_token = 350,
        equal_equal_token = 351,
        comma_token = 352,
        punctuation_end = 352,
#pragma endregion

#pragma region Other tokens
        other_token_start = 400,
        eof_token = 400,
        bad_token = 401,
        identifier_token = 402,
        numeric_literal_token = 403,
        character_literal_token = 404,
        string_literal_token = 405,
        other_token_end = 405,
#pragma endregion

#pragma region Types
        type_start = 1000,
        type = 1000,
        named_type = 1001,
        simple_named_type = 1002,
        identifier_named_type = 1003,
        type_end = 1003,
#pragma endregion

#pragma region Clauses
        clause_start = 2000,
        initializer = 2000,
        type_specifier = 2001,
        named_parameter = 2002,
        argument_list = 2003,
        argument = 2004,
        parameter_list = 2005,
        parameter = 2006,
        expression_body = 2007,
        clause_end = 2007,
#pragma endregion

#pragma region Expressions
        expression_start = 3000,
        expression = 3000,
        literal_expression = 3001,
        identifier_expression = 3002,
        parenthesized_expression = 3003,
        binary_expression = 3004,
        assignment_expression = 3005,
        prefix_expression = 3006,
        postfix_expression = 3007,
        ternary_expression = 3008,
        invocation_expression = 3009,
        expression_end = 3009,
#pragma endregion

#pragma region Statements
        statement_start = 4000,
        statement = 4000,
        variable_declaration_statement = 4001,
        block = 4002,
        return_statement = 4003,
        expression_statement = 4004,
        empty_statement = 4005,
        statement_end = 4005,
#pragma endregion

#pragma region Declarations
        declaration_start = 5000,
        declaration = 5000,
        variable_declaration = 5001,
        function_declaration = 5002,
        declaration_end = 5002,
#pragma endregion
    };

    export constexpr std::string_view get_name(const SyntaxKind kind)
    {
        switch (kind)
        {
            case SyntaxKind::whitespace_trivia:
                return "WhitespaceTrivia";
            case SyntaxKind::new_line_trivia:
                return "NewLineTrivia";
            case SyntaxKind::line_comment_trivia:
                return "LineCommentTrivia";
            case SyntaxKind::block_comment_trivia:
                return "BlockCommentTrivia";
            case SyntaxKind::var_keyword:
                return "var";
            case SyntaxKind::func_keyword:
                return "func";
            case SyntaxKind::extern_keyword:
                return "extern";
            case SyntaxKind::return_keyword:
                return "return";
            case SyntaxKind::mut_keyword:
                return "mut";
            case SyntaxKind::true_keyword:
                return "true";
            case SyntaxKind::false_keyword:
                return "false";
            case SyntaxKind::void_keyword:
                return "void";
            case SyntaxKind::bool_keyword:
                return "bool";
            case SyntaxKind::i8_keyword:
                return "i8";
            case SyntaxKind::i16_keyword:
                return "i16";
            case SyntaxKind::i32_keyword:
                return "i32";
            case SyntaxKind::i64_keyword:
                return "i64";
            case SyntaxKind::i128_keyword:
                return "i128";
            case SyntaxKind::u8_keyword:
                return "u8";
            case SyntaxKind::u16_keyword:
                return "u16";
            case SyntaxKind::u32_keyword:
                return "u32";
            case SyntaxKind::u64_keyword:
                return "u64";
            case SyntaxKind::u128_keyword:
                return "u128";
            case SyntaxKind::isize_keyword:
                return "isize";
            case SyntaxKind::usize_keyword:
                return "usize";
            case SyntaxKind::f16_keyword:
                return "f16";
            case SyntaxKind::f32_keyword:
                return "f32";
            case SyntaxKind::f64_keyword:
                return "f64";
            case SyntaxKind::char_keyword:
                return "char";
            case SyntaxKind::char16_keyword:
                return "char16";
            case SyntaxKind::rune_keyword:
                return "rune";
            case SyntaxKind::str_keyword:
                return "str";
            case SyntaxKind::open_bracket_token:
                return "[";
            case SyntaxKind::close_bracket_token:
                return "]";
            case SyntaxKind::open_paren_token:
                return "(";
            case SyntaxKind::close_paren_token:
                return ")";
            case SyntaxKind::open_brace_token:
                return "{";
            case SyntaxKind::close_brace_token:
                return "}";
            case SyntaxKind::period_token:
                return ".";
            case SyntaxKind::ellipsis_token:
                return "...";
            case SyntaxKind::amp_token:
                return "&";
            case SyntaxKind::amp_amp_token:
                return "&&";
            case SyntaxKind::amp_equal_token:
                return "&=";
            case SyntaxKind::star_token:
                return "*";
            case SyntaxKind::star_equal_token:
                return "*=";
            case SyntaxKind::plus_token:
                return "+";
            case SyntaxKind::plus_plus_token:
                return "++";
            case SyntaxKind::plus_equal_token:
                return "+=";
            case SyntaxKind::minus_token:
                return "-";
            case SyntaxKind::minus_minus_token:
                return "--";
            case SyntaxKind::minus_equal_token:
                return "-=";
            case SyntaxKind::tilde_token:
                return "~";
            case SyntaxKind::bang_token:
                return "!";
            case SyntaxKind::bang_equal_token:
                return "!=";
            case SyntaxKind::slash_token:
                return "/";
            case SyntaxKind::slash_equal_token:
                return "/=";
            case SyntaxKind::percent_token:
                return "%";
            case SyntaxKind::percent_equal_token:
                return "%=";
            case SyntaxKind::less_token:
                return "<";
            case SyntaxKind::less_less_token:
                return "<<";
            case SyntaxKind::less_equal_token:
                return "<=";
            case SyntaxKind::less_less_equal_token:
                return "<<=";
            case SyntaxKind::spaceship_token:
                return "<=>";
            case SyntaxKind::greater_token:
                return ">";
            case SyntaxKind::greater_greater_token:
                return ">>";
            case SyntaxKind::greater_greater_greater_token:
                return ">>>";
            case SyntaxKind::greater_equal_token:
                return ">=";
            case SyntaxKind::greater_greater_equal_token:
                return ">>=";
            case SyntaxKind::greater_greater_greater_equal_token:
                return ">>>=";
            case SyntaxKind::caret_token:
                return "^";
            case SyntaxKind::caret_equal_token:
                return "^=";
            case SyntaxKind::pipe_token:
                return "|";
            case SyntaxKind::pipe_pipe_token:
                return "||";
            case SyntaxKind::pipe_equal_token:
                return "|=";
            case SyntaxKind::question_token:
                return "?";
            case SyntaxKind::question_question_token:
                return "??";
            case SyntaxKind::question_question_equal_token:
                return "??=";
            case SyntaxKind::question_period_token:
                return "?.";
            case SyntaxKind::colon_token:
                return ":";
            case SyntaxKind::double_colon_token:
                return "::";
            case SyntaxKind::semicolon_token:
                return ";";
            case SyntaxKind::equal_token:
                return "=";
            case SyntaxKind::arrow_token:
                return "=>";
            case SyntaxKind::equal_equal_token:
                return "==";
            case SyntaxKind::comma_token:
                return ",";
            case SyntaxKind::eof_token:
                return "EOF";
            case SyntaxKind::bad_token:
                return "<unrecognized>";
            case SyntaxKind::identifier_token:
                return "IdentifierToken";
            case SyntaxKind::numeric_literal_token:
                return "NumericLiteralToken";
            case SyntaxKind::character_literal_token:
                return "CharacterLiteralToken";
            case SyntaxKind::string_literal_token:
                return "StringLiteralToken";
            case SyntaxKind::type:
                return "Type";
            case SyntaxKind::named_type:
                return "NamedType";
            case SyntaxKind::simple_named_type:
                return "SimpleNamedType";
            case SyntaxKind::identifier_named_type:
                return "IdentifierNamedType";
            case SyntaxKind::initializer:
                return "Initializer";
            case SyntaxKind::type_specifier:
                return "TypeSpecifier";
            case SyntaxKind::named_parameter:
                return "NamedParameter";
            case SyntaxKind::argument_list:
                return "ArgumentList";
            case SyntaxKind::argument:
                return "Argument";
            case SyntaxKind::parameter_list:
                return "ParameterList";
            case SyntaxKind::parameter:
                return "Parameter";
            case SyntaxKind::expression_body:
                return "ExpressionBody";
            case SyntaxKind::expression:
                return "Expression";
            case SyntaxKind::literal_expression:
                return "LiteralExpression";
            case SyntaxKind::identifier_expression:
                return "IdentifierExpression";
            case SyntaxKind::parenthesized_expression:
                return "ParenthesizedExpression";
            case SyntaxKind::binary_expression:
                return "BinaryExpression";
            case SyntaxKind::assignment_expression:
                return "AssignmentExpression";
            case SyntaxKind::prefix_expression:
                return "PrefixExpression";
            case SyntaxKind::postfix_expression:
                return "PostfixExpression";
            case SyntaxKind::ternary_expression:
                return "TernaryExpression";
            case SyntaxKind::invocation_expression:
                return "InvocationExpression";
            case SyntaxKind::statement:
                return "Statement";
            case SyntaxKind::variable_declaration_statement:
                return "VariableDeclarationStatement";
            case SyntaxKind::block:
                return "Block";
            case SyntaxKind::return_statement:
                return "ReturnStatement";
            case SyntaxKind::expression_statement:
                return "ExpressionStatement";
            case SyntaxKind::empty_statement:
                return "EmptyStatement";
            case SyntaxKind::declaration:
                return "Declaration";
            case SyntaxKind::variable_declaration:
                return "VariableDeclaration";
            case SyntaxKind::function_declaration:
                return "FunctionDeclaration";
            default:
                throw std::invalid_argument{"Unknown SyntaxKind"};
        }
    }

    export constexpr bool is_trivia(const SyntaxKind kind)
    {
        return std::to_underlying(kind) >= std::to_underlying(SyntaxKind::trivia_start) &&
               std::to_underlying(kind) <= std::to_underlying(SyntaxKind::trivia_end);
    }

    export constexpr bool is_keyword(const SyntaxKind kind)
    {
        return std::to_underlying(kind) >= std::to_underlying(SyntaxKind::keyword_start) &&
               std::to_underlying(kind) <= std::to_underlying(SyntaxKind::keyword_end);
    }

    export constexpr bool is_punctuation(const SyntaxKind kind)
    {
        return std::to_underlying(kind) >= std::to_underlying(SyntaxKind::punctuation_start) &&
               std::to_underlying(kind) <= std::to_underlying(SyntaxKind::punctuation_end);
    }

    export constexpr bool is_other_token(const SyntaxKind kind)
    {
        return std::to_underlying(kind) >= std::to_underlying(SyntaxKind::other_token_start) &&
               std::to_underlying(kind) <= std::to_underlying(SyntaxKind::other_token_end);
    }

    export constexpr bool is_type(const SyntaxKind kind)
    {
        return std::to_underlying(kind) >= std::to_underlying(SyntaxKind::type_start) &&
               std::to_underlying(kind) <= std::to_underlying(SyntaxKind::type_end);
    }

    export constexpr bool is_clause(const SyntaxKind kind)
    {
        return std::to_underlying(kind) >= std::to_underlying(SyntaxKind::clause_start) &&
               std::to_underlying(kind) <= std::to_underlying(SyntaxKind::clause_end);
    }

    export constexpr bool is_expression(const SyntaxKind kind)
    {
        return std::to_underlying(kind) >= std::to_underlying(SyntaxKind::expression_start) &&
               std::to_underlying(kind) <= std::to_underlying(SyntaxKind::expression_end);
    }

    export constexpr bool is_statement(const SyntaxKind kind)
    {
        return std::to_underlying(kind) >= std::to_underlying(SyntaxKind::statement_start) &&
               std::to_underlying(kind) <= std::to_underlying(SyntaxKind::statement_end);
    }

    export constexpr bool is_declaration(const SyntaxKind kind)
    {
        return std::to_underlying(kind) >= std::to_underlying(SyntaxKind::declaration_start) &&
               std::to_underlying(kind) <= std::to_underlying(SyntaxKind::declaration_end);
    }

    export constexpr bool is_token(const SyntaxKind kind)
    {
        return is_keyword(kind) || is_punctuation(kind) || is_other_token(kind);
    }

    export constexpr bool is_node(const SyntaxKind kind)
    {
        return is_type(kind) || is_clause(kind) || is_expression(kind) || is_statement(kind) || is_declaration(kind);
    }
} // namespace prism
