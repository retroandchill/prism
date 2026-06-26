/**
 * @file token_stream.cpp
 * @author Francesco Corso
 * @date 6/26/2026
 * @brief
 */
module;

#include <cassert>

module prism.core.token_stream;

namespace prism
{
    Token TokenStream::previous() const
    {
        return consumed_tokens_.back();
    }

    Token TokenStream::peek()
    {
        if (pending_tokens_.empty())
        {
            buffer_tokens();
        }

        return pending_tokens_.front();
    }

    Token TokenStream::consume()
    {
        advance();
        return previous();
    }

    void TokenStream::advance()
    {
        const auto token = peek();
        pending_tokens_.pop_front();
        consumed_tokens_.push_back(token);
    }

    bool TokenStream::replace_next(const Token &token)
    {
        [[maybe_unused]] const auto [kind, flags, range] = peek();
        assert(kind != TokenKind::eof);

        pending_tokens_.front() = token;
        return true;
    }

    void TokenStream::buffer_tokens()
    {
        static constexpr auto max_tokens = 1024;

        for (std::size_t i = 0; i < max_tokens; ++i)
        {
            auto token = lexer_.next();
            pending_tokens_.push_back(std::move(token));

            if (token.kind == TokenKind::eof)
            {
                break;
            }
        }
    }
} // namespace prism
