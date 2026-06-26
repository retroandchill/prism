/**
 * @file token_stream.ixx
 * @author Francesco Corso
 * @date 6/26/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

#include <cassert>

export module prism.core.token_stream;

import std;
import prism.core.lexer.token;
import prism.core.lexer;
import prism.core.source.source_file;

namespace prism
{
    export class PRISM_CORE_API TokenStream
    {
      public:
        constexpr explicit TokenStream(const SourceFile &source_file) : lexer_{source_file}
        {
        }

        [[nodiscard]] constexpr bool at_end()
        {
            return !consumed_tokens_.empty() && peek().kind == TokenKind::eof;
        }

        [[nodiscard]] constexpr const std::vector<Token> &consumed() const
        {
            return consumed_tokens_;
        }

        Token previous() const;

        Token peek();

        Token consume();

        void advance();

        bool replace_next(const Token &token);

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_value_t<Range>, Token>
        bool replace_next(Range &&range)
        {
            [[maybe_unused]] const auto [k, f, r] = peek();
            assert(k != TokenKind::eof);

            pending_tokens_.pop_front();
            for (auto &&t : std::forward<Range>(range))
            {
                consumed_tokens_.emplace_back(std::forward<decltype(t)>(t));
            }

            return true;
        }

      private:
        void buffer_tokens();

        Lexer lexer_;

        std::vector<Token> consumed_tokens_;
        std::deque<Token> pending_tokens_;
    };
} // namespace prism
