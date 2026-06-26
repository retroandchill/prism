/**
 * @file token.ixx
 * @author Francesco Corso
 * @date 6/20/2026
 * @brief Contains the definitions of all the token types
 */
module;

#include <cassert>

export module prism.core.lexer.token;

import std;
import prism.core.util;
import prism.core.source.source_file;

namespace prism
{
    export enum class TokenFlags : std::uint8_t
    {
        none = 0,
        synthetic = 1 << 0
    };

    template <>
    constexpr bool is_flag_enum<TokenFlags> = true;

    export enum class TokenKind
    {
        eof,
        unrecognized,
        unterminated_block_comment,
        unterminated_string_literal,
        doc_comment,

        kw_var,
        kw_func,
        kw_extern,
        kw_return,
        kw_mut,

        kw_true,
        kw_false,

        kw_bool,
        kw_i8,
        kw_i16,
        kw_i32,
        kw_i64,
        kw_i128,
        kw_u8,
        kw_u16,
        kw_u32,
        kw_u64,
        kw_u128,
        kw_isize,
        kw_usize,
        kw_f16,
        kw_f32,
        kw_f64,
        kw_char,
        kw_char16,
        kw_rune,
        kw_str,

        /**
         * [
         */
        lbracket,

        /**
         * ]
         */
        rbracket,

        /**
         * (
         */
        lparen,

        /**
         * )
         */
        rparen,

        /**
         * {
         */
        lbrace,

        /**
         * }
         */
        rbrace,

        /**
         * .
         */
        period,

        /**
         * ...
         */
        ellipsis,

        /**
         * &
         */
        amp,

        /**
         * &&
         */
        amp_amp,

        /**
         * &=
         */
        amp_equal,

        /**
         * *
         */
        star,

        /**
         * *=
         */
        star_equal,

        /**
         * +
         */
        plus,

        /**
         * ++
         */
        plus_plus,

        /**
         * +=
         */
        plus_equal,

        /**
         * -
         */
        minus,

        /**
         * ->
         */
        arrow,

        /**
         * --
         */
        minus_minus,

        /**
         * -=
         */
        minus_equal,

        /**
         * ~
         */
        tilde,

        /**
         * !
         */
        exclaim,

        /**
         * !=
         */
        exclaim_equal,

        /**
         * /
         */
        slash,

        /**
         * /=
         */
        slash_equal,

        /**
         * %
         */
        percent,

        /**
         * %=
         */
        percent_equal,

        /**
         * <
         */
        less,

        /**
         * <<
         */
        less_less,

        /**
         * <=
         */
        less_equal,

        /**
         * <<=
         */
        less_less_equal,

        /**
         * <=>
         */
        spaceship,

        /**
         * >
         */
        greater,

        /**
         * >>
         */
        greater_greater,

        /**
         * >=
         */
        greater_equal,

        /**
         * >>=
         */
        greater_greater_equal,

        /**
         * ^
         */
        caret,

        /**
         * ^=
         */
        caret_equal,

        /**
         * |
         */
        pipe,

        /**
         * ||
         */
        pipe_pipe,

        /**
         * |=
         */
        pipe_equal,

        /**
         * ?
         */
        question,

        /**
         * ??
         */
        question_question,

        /**
         * ?.
         */
        question_period,

        /**
         * :
         */
        colon,

        /**
         * ::
         */
        colon_colon,

        /**
         * ;
         */
        semicolon,

        /**
         * =
         */
        equal,

        /**
         * ==
         */
        equal_equal,

        /**
         * ,
         */
        comma,

        identifier,
        number,
        string_literal,
    };

    export struct Token
    {
        TokenKind kind{};
        TokenFlags flags{};
        SourceRange range{};
    };

    export constexpr Token make_token(const TokenKind kind, const std::size_t start = 0, const std::size_t end = 0)
    {
        assert(end >= start);
        return Token{.kind = kind, .range = {start, end}};
    }
} // namespace prism
