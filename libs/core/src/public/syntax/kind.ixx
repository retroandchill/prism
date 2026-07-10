/**
 * @file kind.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
module;

#include "prism/core/syntax.hpp"

export module prism.core:syntax.kind;

import std;

namespace prism
{
    constexpr std::uint16_t trivia_start = 50;
    constexpr std::uint16_t keyword_start = 100;
    constexpr std::uint16_t punctuation_start = 200;
    constexpr std::uint16_t token_start = 300;
    constexpr std::uint16_t node_start = 400;

#define GEN_ENUM(r, start, i, elem)                                                                                    \
    BOOST_PP_IF(BOOST_PP_EQUAL(i, 0), PRISM_KIND_ENUM_NAME(elem) = start, PRISM_KIND_ENUM_NAME(elem), ),

#define PRISM_SYNTAX_KINDS(MACRO)                                                                                      \
    BOOST_PP_SEQ_FOR_EACH_I(MACRO, trivia_start, PRISM_SYNTAX_TRIVIA(PRISM_TRIVIA_ELEM))                               \
    BOOST_PP_SEQ_FOR_EACH_I(MACRO, keyword_start, PRISM_SYNTAX_KEYWORDS(PRISM_KEYWORD_ELEM))                           \
    BOOST_PP_SEQ_FOR_EACH_I(MACRO, punctuation_start, PRISM_SYNTAX_PUNCTUATIONS(PRISM_PUNCTUATION_ELEM))               \
    BOOST_PP_SEQ_FOR_EACH_I(MACRO, token_start, PRISM_SYNTAX_OTHER_TOKENS(PRISM_TOKEN_ELEM))

    export enum class SyntaxKind : std::uint16_t
    {
        none = 0,
        list = 1,

        PRISM_SYNTAX_KINDS(GEN_ENUM)
    };

#define CONDITIONAL_CHECK(r, data, elem)                                                                               \
    case SyntaxKind::PRISM_KIND_ENUM_NAME(elem):                                                                       \
        return PRISM_KIND_TEXT(elem);

    export constexpr std::string_view to_string(const SyntaxKind kind)
    {
        switch (kind)
        {
            BOOST_PP_SEQ_FOR_EACH(CONDITIONAL_CHECK, _, PRISM_SYNTAX_KIND_ENTRIES);
            default:
                throw std::invalid_argument{"Unknown syntax kind"};
        }
    }

    export constexpr bool is_trivia(const SyntaxKind kind) noexcept
    {
        const auto underlying = std::to_underlying(kind);
        return underlying >= trivia_start && underlying < keyword_start;
    }

    export constexpr bool is_token(const SyntaxKind kind) noexcept
    {
        const auto underlying = std::to_underlying(kind);
        return underlying >= keyword_start && underlying < node_start;
    }

    export constexpr bool is_node(const SyntaxKind kind) noexcept
    {
        const auto underlying = std::to_underlying(kind);
        return underlying >= node_start;
    }
} // namespace prism
