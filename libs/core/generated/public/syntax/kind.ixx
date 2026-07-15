export module prism.core:syntax.kind;

import std;

namespace prism
{
    export enum class SyntaxKind : std::uint16_t
    {
        none = 0,
        list = 1,

#pragma region Trivia
        trivia_start = 50,
        whitespace_trivia = 50,
        new_line_trivia = 51,
        line_comment_trivia = 52,
        block_comment_trivia = 53,
        trivia_end = 53,
#pragma endregion

#pragma region Keywords
        keyword_start = 100,
        var_keyword = 100,
        func_keyword = 101,
        extern_keyword = 102,
        return_keyword = 103,
        mut_keyword = 104,
        true_keyword = 105,
        false_keyword = 106,
        void_keyword = 107,
        bool_keyword = 108,
        i8_keyword = 109,
        i16_keyword = 110,
        i32_keyword = 111,
        i64_keyword = 112,
        i128_keyword = 113,
        u8_keyword = 114,
        u16_keyword = 115,
        u32_keyword = 116,
        u64_keyword = 117,
        u128_keyword = 118,
        isize_keyword = 119,
        usize_keyword = 120,
        f16_keyword = 121,
        f32_keyword = 122,
        f64_keyword = 123,
        char_keyword = 124,
        char16_keyword = 125,
        rune_keyword = 126,
        str_keyword = 127,
        keyword_end = 127,
#pragma endregion

#pragma region Punctuation
        punctuation_start = 200,
        lbracket_token = 200,
        rbracket_token = 201,
        lparen_token = 202,
        rparen_token = 203,
        lbrace_token = 204,
        rbrace_token = 205,
        period_token = 206,
        ellipsis_token = 207,
        amp_token = 208,
        amp_amp_token = 209,
        amp_equal_token = 210,
        star_token = 211,
        star_equal_token = 212,
        plus_token = 213,
        plus_plus_token = 214,
        plus_equal_token = 215,
        minus_token = 216,
        arrow_token = 217,
        minus_minus_token = 218,
        minus_equal_token = 219,
        tilde_token = 220,
        exclaim_token = 221,
        exclaim_equal_token = 222,
        slash_token = 223,
        slash_equal_token = 224,
        percent_token = 225,
        percent_equal_token = 226,
        less_token = 227,
        less_less_token = 228,
        less_equal_token = 229,
        less_less_equal_token = 230,
        spaceship_token = 231,
        greater_token = 232,
        greater_greater_token = 233,
        greater_greater_greater_token = 234,
        greater_equal_token = 235,
        greater_greater_equal_token = 236,
        greater_greater_greater_equal_token = 237,
        caret_token = 238,
        caret_equal_token = 239,
        pipe_token = 240,
        pipe_pipe_token = 241,
        pipe_equal_token = 242,
        question_token = 243,
        question_question_token = 244,
        question_period_token = 245,
        colon_token = 246,
        colon_colon_token = 247,
        semicolon_token = 248,
        equal_token = 249,
        big_arrow_token = 250,
        equal_equal_token = 251,
        comma_token = 252,
        punctuation_end = 252,
#pragma endregion

#pragma region Other Tokens
        other_token_start = 300,
        eof_token = 300,
        bad_token = 301,
        identifier_token = 302,
        number_token = 303,
        character_literal_token = 304,
        string_literal_token = 305,
        other_token_end = 305,
#pragma endregion

#pragma region Type
        type_start = 400,
        identifier_named_type = 400,
        type_end = 400,
#pragma endregion

#pragma region Clauses
        clauses_start = 500,
        initializer = 500,
        type_hint = 501,
        named_parameter = 502,
        argument_list = 503,
        argument = 504,
        clauses_end = 504,
#pragma endregion

#pragma region Expression
        expression_start = 600,
        literal_expression = 600,
        identifier_expression = 601,
        parenthesized_expression = 602,
        binary_expression = 603,
        prefix_expression = 604,
        postfix_expression = 605,
        ternary_expression = 606,
        invocation_expression = 607,
        expression_end = 607,
#pragma endregion

#pragma region Statement
        statement_start = 700,
        variable_declaration_statement = 700,
        block = 701,
        return_statement = 702,
        expression_statement = 703,
        empty_statement = 704,
        statement_end = 704,
#pragma endregion

#pragma region Declaration
        declaration_start = 800,
        variable_declaration = 800,
        declaration_end = 800,
#pragma endregion
    };

    export constexpr std::string_view get_name(const SyntaxKind kind)
    {
        switch (kind)
        {
            case SyntaxKind::whitespace_trivia:
                return "Whitespace";
            case SyntaxKind::new_line_trivia:
                return "New Line";
            case SyntaxKind::line_comment_trivia:
                return "Line Comment";
            case SyntaxKind::block_comment_trivia:
                return "Block Comment";
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
            case SyntaxKind::lbracket_token:
                return "[";
            case SyntaxKind::rbracket_token:
                return "]";
            case SyntaxKind::lparen_token:
                return "(";
            case SyntaxKind::rparen_token:
                return ")";
            case SyntaxKind::lbrace_token:
                return "{";
            case SyntaxKind::rbrace_token:
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
            case SyntaxKind::arrow_token:
                return "->";
            case SyntaxKind::minus_minus_token:
                return "--";
            case SyntaxKind::minus_equal_token:
                return "-=";
            case SyntaxKind::tilde_token:
                return "~";
            case SyntaxKind::exclaim_token:
                return "!";
            case SyntaxKind::exclaim_equal_token:
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
            case SyntaxKind::question_period_token:
                return "?.";
            case SyntaxKind::colon_token:
                return ":";
            case SyntaxKind::colon_colon_token:
                return "::";
            case SyntaxKind::semicolon_token:
                return ";";
            case SyntaxKind::equal_token:
                return "=";
            case SyntaxKind::big_arrow_token:
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
                return "Identifier";
            case SyntaxKind::number_token:
                return "Number";
            case SyntaxKind::character_literal_token:
                return "Character Literal";
            case SyntaxKind::string_literal_token:
                return "String Literal";
            case SyntaxKind::identifier_named_type:
                return "Identifier Named Type";
            case SyntaxKind::initializer:
                return "Initializer";
            case SyntaxKind::type_hint:
                return "Type Hint";
            case SyntaxKind::named_parameter:
                return "Named Parameter";
            case SyntaxKind::argument_list:
                return "Argument List";
            case SyntaxKind::argument:
                return "Argument";
            case SyntaxKind::literal_expression:
                return "Literal Expression";
            case SyntaxKind::identifier_expression:
                return "Identifier Expression";
            case SyntaxKind::parenthesized_expression:
                return "Parenthesized Expression";
            case SyntaxKind::binary_expression:
                return "Binary Expression";
            case SyntaxKind::prefix_expression:
                return "Prefix Expression";
            case SyntaxKind::postfix_expression:
                return "Postfix Expression";
            case SyntaxKind::ternary_expression:
                return "Ternary Expression";
            case SyntaxKind::invocation_expression:
                return "Invocation Expression";
            case SyntaxKind::variable_declaration_statement:
                return "Variable Declaration Statement";
            case SyntaxKind::block:
                return "Block";
            case SyntaxKind::return_statement:
                return "Return Statement";
            case SyntaxKind::expression_statement:
                return "Expression Statement";
            case SyntaxKind::empty_statement:
                return "Empty Statement";
            case SyntaxKind::variable_declaration:
                return "Variable Declaration";
            default:
                throw std::invalid_argument{"Unknown SyntaxKind"};
        };
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

    export constexpr bool is_clauses(const SyntaxKind kind)
    {
        return std::to_underlying(kind) >= std::to_underlying(SyntaxKind::clauses_start) &&
               std::to_underlying(kind) <= std::to_underlying(SyntaxKind::clauses_end);
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
        return is_type(kind) || is_clauses(kind) || is_expression(kind) || is_statement(kind) || is_declaration(kind);
    }
} // namespace prism
