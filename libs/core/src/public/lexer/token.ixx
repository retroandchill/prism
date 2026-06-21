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
import prism.core.source.source_file;

namespace prism
{
    export enum class TokenKind
    {
        eof,
        unrecognized,

        kw_var,
        kw_func,
        kw_extern,
        kw_return,

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
        string,
    };

    export struct Token
    {
        TokenKind kind{};
        SourceRange range{};
    };

    export constexpr Token make_token(const TokenKind kind, const std::size_t start = 0, const std::size_t end = 0)
    {
        assert(end >= start);
        return Token{kind, {start, end}};
    }
} // namespace prism
