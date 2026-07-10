/**
 * @file green_token.cpp
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
module;

#include <cassert>

module prism.core:syntax.green.green_token.impl;

import :syntax.green.green_token;

namespace prism
{

    GreenToken::GreenToken(const SyntaxKind kind,
                           RefCountPtr<const GreenTriviaList> leading_trivia,
                           RefCountPtr<const GreenTriviaList> trailing_trivia)
        : GreenToken{kind,
                     static_cast<std::uint32_t>(to_string(kind).length()),
                     std::move(leading_trivia),
                     std::move(trailing_trivia)}
    {
    }

    GreenToken::GreenToken(const SyntaxKind kind,
                           const std::uint32_t width,
                           RefCountPtr<const GreenTriviaList> leading_trivia,
                           RefCountPtr<const GreenTriviaList> trailing_trivia)
        : GreenNode{kind, width}, leading_trivia_{normalize_trivia(std::move(leading_trivia))},
          trailing_trivia_{normalize_trivia(std::move(trailing_trivia))}
    {
        assert(is_token(kind));
        set_child_count((leading_trivia != nullptr ? 1 : 0) + (trailing_trivia != nullptr ? 1 : 0));
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

    RefCountPtr<GreenToken> GreenToken::with_leading_trivia(RefCountPtr<const GreenTriviaList> leading_trivia) const
    {
        leading_trivia = normalize_trivia(std::move(leading_trivia));

        if (leading_trivia_ == leading_trivia)
            return shared_from_this();

        return clone_with_trivia(std::move(leading_trivia), trailing_trivia_);
    }

    RefCountPtr<GreenToken> GreenToken::with_trailing_trivia(RefCountPtr<const GreenTriviaList> trailing_trivia) const
    {
        trailing_trivia = normalize_trivia(std::move(trailing_trivia));

        if (trailing_trivia_ == trailing_trivia)
            return shared_from_this();

        return clone_with_trivia(leading_trivia_, std::move(trailing_trivia));
    }

    RefCountPtr<GreenToken> GreenToken::with_leading_and_trailing_trivia(
        RefCountPtr<const GreenTriviaList> leading_trivia,
        RefCountPtr<const GreenTriviaList> trailing_trivia) const
    {
        leading_trivia = normalize_trivia(std::move(leading_trivia));
        trailing_trivia = normalize_trivia(std::move(trailing_trivia));

        if (leading_trivia_ == leading_trivia && trailing_trivia_ == trailing_trivia)
            return shared_from_this();

        return clone_with_trivia(std::move(leading_trivia), std::move(trailing_trivia));
    }

    RefCountPtr<GreenToken> GreenToken::clone_with_trivia(RefCountPtr<const GreenTriviaList> leading_trivia,
                                                          RefCountPtr<const GreenTriviaList> trailing_trivia) const
    {
        return make_ref_counted<GreenToken>(kind(), width(), std::move(leading_trivia), std::move(trailing_trivia));
    }
} // namespace prism
