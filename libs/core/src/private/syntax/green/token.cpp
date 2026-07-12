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
    const GreenPtr<GreenToken> GreenToken::eof = make_ref_counted<const GreenToken>(SyntaxKind::end_of_file_token);
    const GreenPtr<GreenToken> GreenToken::bad_token = make_ref_counted<const GreenToken>(SyntaxKind::bad_token, 1);

    GreenToken::GreenToken(const SyntaxKind kind,
                           GreenPtr<GreenTriviaList> leading_trivia,
                           GreenPtr<GreenTriviaList> trailing_trivia)
        : GreenToken{kind,
                     static_cast<std::uint32_t>(to_string(kind).length()),
                     std::move(leading_trivia),
                     std::move(trailing_trivia)}
    {
    }

    GreenToken::GreenToken(const SyntaxKind kind,
                           const std::uint32_t width,
                           GreenPtr<GreenTriviaList> leading_trivia,
                           GreenPtr<GreenTriviaList> trailing_trivia)
        : GreenNode{kind, width}, leading_trivia_{normalize_trivia(std::move(leading_trivia))},
          trailing_trivia_{normalize_trivia(std::move(trailing_trivia))}
    {
        assert(is_token(kind));
        set_child_count((leading_trivia != nullptr ? 1 : 0) + (trailing_trivia != nullptr ? 1 : 0));
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
        return leading_trivia_ != nullptr ? leading_trivia_->width() : 0;
    }

    std::uint32_t GreenToken::trailing_trivia_width() const
    {
        return trailing_trivia_ != nullptr ? trailing_trivia_->width() : 0;
    }

    Optional<const GreenNode &> GreenToken::get_child(std::size_t index) const
    {
        if (index == 0)
        {
            if (leading_trivia_ != nullptr)
                return leading_trivia_.get();
            if (trailing_trivia_ != nullptr)
                return trailing_trivia_.get();
        }
        else if (index == 1)
        {
            if (leading_trivia_ == nullptr && trailing_trivia_ != nullptr)
                return trailing_trivia_.get();
        }

        return nullptr;
    }

    GreenPtr<GreenToken> GreenToken::with_leading_trivia(GreenPtr<GreenTriviaList> leading_trivia) const
    {
        leading_trivia = normalize_trivia(std::move(leading_trivia));

        if (leading_trivia_ == leading_trivia)
            return shared_from_this();

        return clone_with_trivia(std::move(leading_trivia), trailing_trivia_);
    }

    GreenPtr<GreenToken> GreenToken::with_trailing_trivia(GreenPtr<GreenTriviaList> trailing_trivia) const
    {
        trailing_trivia = normalize_trivia(std::move(trailing_trivia));

        if (trailing_trivia_ == trailing_trivia)
            return shared_from_this();

        return clone_with_trivia(leading_trivia_, std::move(trailing_trivia));
    }

    GreenPtr<GreenToken> GreenToken::with_leading_and_trailing_trivia(GreenPtr<GreenTriviaList> leading_trivia,
                                                                      GreenPtr<GreenTriviaList> trailing_trivia) const
    {
        leading_trivia = normalize_trivia(std::move(leading_trivia));
        trailing_trivia = normalize_trivia(std::move(trailing_trivia));

        if (leading_trivia_ == leading_trivia && trailing_trivia_ == trailing_trivia)
            return shared_from_this();

        return clone_with_trivia(std::move(leading_trivia), std::move(trailing_trivia));
    }

    GreenPtr<GreenToken> GreenToken::clone_with_trivia(GreenPtr<GreenTriviaList> leading_trivia,
                                                       GreenPtr<GreenTriviaList> trailing_trivia) const
    {
        return make_ref_counted<const GreenToken>(kind(),
                                                  width(),
                                                  std::move(leading_trivia),
                                                  std::move(trailing_trivia));
    }
} // namespace prism
