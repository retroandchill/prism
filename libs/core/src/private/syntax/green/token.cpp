/**
 * @file green_token.cpp
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
module;

#include "prism/core/syntax.hpp"

#include <cassert>

module prism.core:syntax.green.token.impl;

import :syntax.green.token;

namespace prism
{
    GreenToken::GreenToken(const SyntaxKind kind, GreenTriviaList leading_trivia, GreenTriviaList trailing_trivia)
        : GreenToken{kind,
                     static_cast<std::uint32_t>(to_string(kind).length()),
                     std::move(leading_trivia),
                     std::move(trailing_trivia)}
    {
        set_child_count(2);
        adjust_flags_and_width(leading_trivia_);
        adjust_flags_and_width(trailing_trivia_);
    }

    GreenToken::GreenToken(const SyntaxKind kind,
                           const std::uint32_t width,
                           GreenTriviaList leading_trivia,
                           GreenTriviaList trailing_trivia)
        : GreenNode{kind, width}, leading_trivia_{std::move(leading_trivia)},
          trailing_trivia_{std::move(trailing_trivia)}
    {
        assert(prism::is_token(kind));
        set_child_count(2);
        adjust_flags_and_width(leading_trivia_);
        adjust_flags_and_width(trailing_trivia_);
    }

    const GreenPtr<GreenToken> &GreenToken::eof()
    {
        static auto instance = make_ref_counted<const GreenToken>(SyntaxKind::end_of_file_token);
        return instance;
    }

    const GreenPtr<GreenToken> &GreenToken::bad_token()
    {
        static auto instance = make_ref_counted<const GreenToken>(SyntaxKind::bad_token, 1);
        return instance;
    }

    GreenPtr<GreenToken> GreenToken::from(const SyntaxKind kind)
    {
        using namespace std::string_view_literals;
        if (is_keyword(kind))
        {
            static std::array keywords = {
#define X(name) make_ref_counted<GreenToken>(SyntaxKind::name##_keyword),
                PRISM_SYNTAX_KEYWORDS(X)
#undef X
            };

            return keywords[std::to_underlying(kind) - keyword_start];
        }

        if (is_punctuation(kind))
        {
            static std::array punctuations = {
#define X(name, str) make_ref_counted<GreenToken>(SyntaxKind::name##_token),
                PRISM_SYNTAX_PUNCTUATIONS(X)
#undef X
            };

            return punctuations[std::to_underlying(kind) - punctuation_start];
        }

        return nullptr;
    }

    std::string_view GreenToken::text() const
    {
        return to_string(kind());
    }

    std::uint32_t GreenToken::leading_trivia_width() const
    {
        return leading_trivia_.node().width();
    }

    std::uint32_t GreenToken::trailing_trivia_width() const
    {
        return trailing_trivia_.node().width();
    }

    Optional<const GreenNode &> GreenToken::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return leading_trivia_.node();
            case 1:
                return trailing_trivia_.node();
            default:
                return std::nullopt;
        }
    }

    GreenPtr<GreenToken> GreenToken::with_leading_trivia(GreenTriviaList leading_trivia) const
    {
        if (leading_trivia_ == leading_trivia)
            return shared_from_this();

        return clone_with_trivia(std::move(leading_trivia), trailing_trivia_);
    }

    GreenPtr<GreenToken> GreenToken::with_trailing_trivia(GreenTriviaList trailing_trivia) const
    {
        if (trailing_trivia_ == trailing_trivia)
            return shared_from_this();

        return clone_with_trivia(leading_trivia_, std::move(trailing_trivia));
    }

    GreenPtr<GreenToken> GreenToken::with_leading_and_trailing_trivia(GreenTriviaList leading_trivia,
                                                                      GreenTriviaList trailing_trivia) const
    {
        if (leading_trivia_ == leading_trivia && trailing_trivia_ == trailing_trivia)
            return shared_from_this();

        return clone_with_trivia(std::move(leading_trivia), std::move(trailing_trivia));
    }

    GreenPtr<GreenToken> GreenToken::clone_with_trivia(GreenTriviaList leading_trivia,
                                                       GreenTriviaList trailing_trivia) const
    {
        return make_ref_counted<const GreenToken>(kind(),
                                                  width(),
                                                  std::move(leading_trivia),
                                                  std::move(trailing_trivia));
    }
} // namespace prism
