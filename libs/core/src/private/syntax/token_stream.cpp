/**
 * @file token_stream.cpp
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:syntax.token_stream.impl;

import :syntax.token_stream;

namespace prism
{

    bool TokenStream::at_end()
    {
        return peek().kind() == SyntaxKind::eof_token;
    }

    const GreenToken &TokenStream::peek(const std::size_t count)
    {
        if (lookahead_.empty())
            buffer_tokens();

        return !lookahead_.empty() ? *lookahead_[count - 1] : previous();
    }

    void TokenStream::advance()
    {
        auto &token = peek();
        tokens_.push_back(token.shared_from_this());
        lookahead_.pop_front();
    }

    void TokenStream::replace_next(GreenPtr<GreenToken> token)
    {
        [[maybe_unused]] const auto &next = peek();
        DEBUG_ASSERT(next.kind() != SyntaxKind::eof_token);

        lookahead_.pop_front();
        lookahead_.push_front(std::move(token));
    }

    void TokenStream::buffer_tokens()
    {
        static constexpr std::size_t max_tokens = 1024;

        for ([[maybe_unused]] auto i : std::views::iota(0uz, max_tokens))
        {
            if (const auto &token = lookahead_.emplace_back(lexer_.next()); token->kind() == SyntaxKind::eof_token)
                break;
        }
    }
} // namespace prism
