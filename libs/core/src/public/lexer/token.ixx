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
#define PRISM_TOKEN(name, str) name,
#define PRISM_KEYWORD(name) kw_##name,
#define PRISM_PUNCT(name, str) name,
#include "tokens.inl"

#undef PRISM_TOKEN
#undef PRISM_KEYWORD
#undef PRISM_PUNCT
    };

    export constexpr std::string_view to_string(const TokenKind kind) noexcept
    {
        switch (kind)
        {
#define PRISM_TOKEN(name, str)                                                                                         \
    case TokenKind::name:                                                                                              \
        return str;
#define PRISM_KEYWORD(name)                                                                                            \
    case TokenKind::kw_##name:                                                                                         \
        return "'" #name "'";
#define PRISM_PUNCT(name, str)                                                                                         \
    case TokenKind::name:                                                                                              \
        return "'" #str "'";
#include "tokens.inl"

#undef PRISM_TOKEN
#undef PRISM_KEYWORD
#undef PRISM_PUNCT
        }

        return "<invalid>";
    }

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

template <typename CharType>
struct std::formatter<prism::TokenKind, CharType>
{
    static constexpr auto parse(std::format_parse_context &ctx)
    {
        auto it = ctx.begin();
        while (it != ctx.end() && *it != '}')
            ++it;
        return it;
    }

    auto format(const prism::TokenKind kind, auto &ctx) const
    {
        return std::format_to(ctx.out(), "{}", static_cast<std::underlying_type_t<prism::TokenKind>>(kind));
    }
};
