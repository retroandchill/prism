/**
 * @file token_stream.cpp
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
module;

#include <cassert>

module prism.core:syntax.token_stream.impl;

import :syntax.token_stream;

namespace prism
{

    bool TokenStream::at_end()
    {
        return peek().kind() == SyntaxKind::end_of_file_token;
    }

    const SyntaxToken &TokenStream::peek(const std::size_t count)
    {
        if (lookahead_.empty())
            buffer_tokens();

        return !lookahead_.empty() ? lookahead_[count - 1] : previous();
    }

    void TokenStream::advance()
    {
        auto token = peek();
        tokens_.push_back(std::move(token));
        lookahead_.pop_front();
    }

    void TokenStream::replace_next(SyntaxToken token)
    {
        [[maybe_unused]] const auto &next = peek();
        assert(next.kind() != SyntaxKind::end_of_file_token);

        lookahead_.pop_front();
        lookahead_.push_front(std::move(token));
    }

    void TokenStream::buffer_tokens()
    {
        static constexpr std::size_t max_tokens = 1024;

        for (auto i : std::views::iota(0uz, max_tokens))
        {
            if (const auto &token = lookahead_.emplace_back(lexer_.next());
                token.kind() == SyntaxKind::end_of_file_token)
                break;
        }
    }
} // namespace prism
