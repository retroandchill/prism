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

#pragma region Structured trivia
        structured_trivium_start = 200,
        skipped_tokens_trivia = 200,
        structured_trivium_end = 200,
#pragma endregion

#pragma region Keywords
        keyword_start = 1000,
        var_keyword = 1000,
        func_keyword = 1001,
        extern_keyword = 1002,
        return_keyword = 1003,
        mut_keyword = 1004,
        true_keyword = 1005,
        false_keyword = 1006,
        void_keyword = 1007,
        bool_keyword = 1008,
        i8_keyword = 1009,
        i16_keyword = 1010,
        i32_keyword = 1011,
        i64_keyword = 1012,
        i128_keyword = 1013,
        u8_keyword = 1014,
        u16_keyword = 1015,
        u32_keyword = 1016,
        u64_keyword = 1017,
        u128_keyword = 1018,
        isize_keyword = 1019,
        usize_keyword = 1020,
        f16_keyword = 1021,
        f32_keyword = 1022,
        f64_keyword = 1023,
        char_keyword = 1024,
        char16_keyword = 1025,
        rune_keyword = 1026,
        str_keyword = 1027,
        keyword_end = 1027,
#pragma endregion

#pragma region Punctuations
        punctuation_start = 1200,
        open_bracket_token = 1200,
        close_bracket_token = 1201,
        open_paren_token = 1202,
        close_paren_token = 1203,
        open_brace_token = 1204,
        close_brace_token = 1205,
        period_token = 1206,
        ellipsis_token = 1207,
        amp_token = 1208,
        amp_amp_token = 1209,
        amp_equal_token = 1210,
        star_token = 1211,
        star_equal_token = 1212,
        plus_token = 1213,
        plus_plus_token = 1214,
        plus_equal_token = 1215,
        minus_token = 1216,
        minus_minus_token = 1217,
        minus_equal_token = 1218,
        tilde_token = 1219,
        bang_token = 1220,
        bang_equal_token = 1221,
        slash_token = 1222,
        slash_equal_token = 1223,
        percent_token = 1224,
        percent_equal_token = 1225,
        less_token = 1226,
        less_less_token = 1227,
        less_equal_token = 1228,
        less_less_equal_token = 1229,
        spaceship_token = 1230,
        greater_token = 1231,
        greater_greater_token = 1232,
        greater_greater_greater_token = 1233,
        greater_equal_token = 1234,
        greater_greater_equal_token = 1235,
        greater_greater_greater_equal_token = 1236,
        caret_token = 1237,
        caret_equal_token = 1238,
        pipe_token = 1239,
        pipe_pipe_token = 1240,
        pipe_equal_token = 1241,
        question_token = 1242,
        question_question_token = 1243,
        question_question_equal_token = 1244,
        question_period_token = 1245,
        colon_token = 1246,
        double_colon_token = 1247,
        semicolon_token = 1248,
        equal_token = 1249,
        arrow_token = 1250,
        equal_equal_token = 1251,
        comma_token = 1252,
        punctuation_end = 1252,
#pragma endregion

#pragma region Other tokens
        other_token_start = 1300,
        eof_token = 1300,
        bad_token = 1301,
        identifier_token = 1302,
        numeric_literal_token = 1303,
        character_literal_token = 1304,
        string_literal_token = 1305,
        other_token_end = 1305,
#pragma endregion

#pragma region Top level
        top_level_start = 2000,
        compilation_unit = 2000,
        top_level_end = 2000,
#pragma endregion

#pragma region Types
        type_start = 3000,
        type = 3000,
        predefined_type = 3001,
        named_type = 3002,
        simple_named_type = 3003,
        identifier_named_type = 3004,
        type_end = 3004,
#pragma endregion

#pragma region Clauses
        clause_start = 4000,
        initializer = 4000,
        type_specifier = 4001,
        named_parameter = 4002,
        argument_list = 4003,
        argument = 4004,
        parameter_list = 4005,
        parameter = 4006,
        expression_body = 4007,
        clause_end = 4007,
#pragma endregion

#pragma region Expressions
        expression_start = 5000,
        expression = 5000,
        literal_expression = 5001,
        identifier_expression = 5002,
        parenthesized_expression = 5003,
        binary_expression = 5004,
        assignment_expression = 5005,
        prefix_expression = 5006,
        postfix_expression = 5007,
        ternary_expression = 5008,
        invocation_expression = 5009,
        expression_end = 5009,
#pragma endregion

#pragma region Statements
        statement_start = 6000,
        statement = 6000,
        variable_declaration_statement = 6001,
        block = 6002,
        return_statement = 6003,
        expression_statement = 6004,
        empty_statement = 6005,
        statement_end = 6005,
#pragma endregion

#pragma region Declarations
        declaration_start = 7000,
        declaration = 7000,
        incomplete_declaration = 7001,
        variable_declaration = 7002,
        function_declaration = 7003,
        declaration_end = 7003,
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
            case SyntaxKind::skipped_tokens_trivia:
                return "SkippedTokensTrivia";
            case SyntaxKind::compilation_unit:
                return "CompilationUnit";
            case SyntaxKind::type:
                return "Type";
            case SyntaxKind::predefined_type:
                return "PredefinedType";
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
            case SyntaxKind::incomplete_declaration:
                return "IncompleteDeclaration";
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

    export constexpr bool is_structured_trivium(const SyntaxKind kind)
    {
        return std::to_underlying(kind) >= std::to_underlying(SyntaxKind::structured_trivium_start) &&
               std::to_underlying(kind) <= std::to_underlying(SyntaxKind::structured_trivium_end);
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

    export constexpr bool is_top_level(const SyntaxKind kind)
    {
        return std::to_underlying(kind) >= std::to_underlying(SyntaxKind::top_level_start) &&
               std::to_underlying(kind) <= std::to_underlying(SyntaxKind::top_level_end);
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

    export constexpr bool is_structured_trivia(const SyntaxKind kind)
    {
        return is_structured_trivium(kind);
    }

    export constexpr bool is_token(const SyntaxKind kind)
    {
        return is_keyword(kind) || is_punctuation(kind) || is_other_token(kind);
    }

    export constexpr bool is_node(const SyntaxKind kind)
    {
        return is_top_level(kind) || is_type(kind) || is_clause(kind) || is_expression(kind) || is_statement(kind) ||
               is_declaration(kind);
    }
} // namespace prism
