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
        eof = 0,
        unrecognized,

        kw_var,
        kw_func,
        kw_extern,
        kw_return,

        lbrace = 100,
        rbrace,
        lbracket,
        rbracket,
        lparen,
        rparen,
        comma,
        semicolon,
        colon,
        dot,
        equal,
        plus,
        minus,
        star,
        slash,
        percent,
        ampersand,
        pipe,
        caret,
        question,
        exclamation,

        identifier = 200,
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
