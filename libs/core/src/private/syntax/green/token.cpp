/**
 * @file green_token.cpp
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
module;

#include "prism/core/syntax.hpp"

#include <libassert/assert-macros.hpp>

module prism.core:syntax.green.token.impl;

import :syntax.green.token;
import :syntax.lexing_utils;

namespace prism
{
    GreenToken::GreenToken(const SyntaxKind kind, GreenTriviaList leading_trivia, GreenTriviaList trailing_trivia)
        : GreenToken{kind,
                     static_cast<std::uint32_t>(get_name(kind).length()),
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
        DEBUG_ASSERT(prism::is_token(kind));
        set_child_count(2);
        adjust_flags_and_width(leading_trivia_);
        adjust_flags_and_width(trailing_trivia_);
    }

    const GreenPtr<GreenToken> &GreenToken::eof()
    {
        static auto instance = make_ref_counted<const GreenToken>(SyntaxKind::eof_token);
        return instance;
    }

    const GreenPtr<GreenToken> &GreenToken::bad_token()
    {
        static auto instance = make_ref_counted<const GreenToken>(SyntaxKind::bad_token, 1);
        return instance;
    }

    GreenPtr<GreenToken> GreenToken::from(const SyntaxKind kind)
    {
        switch (kind)
        {
            case SyntaxKind::eof_token:
                return eof();
            case SyntaxKind::bad_token:
                return bad_token();
            default:
                return get_static_green_token(kind);
        }
    }

    std::string_view GreenToken::text() const
    {
        return get_name(kind());
    }

    std::uint32_t GreenToken::leading_trivia_width() const
    {
        return leading_trivia_.node().transform([](const GreenNode &trivia) { return trivia.width(); }).value_or(0);
    }

    std::uint32_t GreenToken::trailing_trivia_width() const
    {
        return trailing_trivia_.node().transform([](const GreenNode &trivia) { return trivia.width(); }).value_or(0);
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

    GreenPtr<GreenToken> GreenToken::update(GreenTriviaList leading_trivia, GreenTriviaList trailing_trivia) const
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
