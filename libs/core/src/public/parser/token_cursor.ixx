/**
 * @file token_cursor.ixx
 * @author Francesco Corso
 * @date 6/24/2026
 * @brief Defines the class for the cursor over the list of tokens
 */
export module prism.core.parser.token_cursor;

import std;
import prism.core.lexer.token;

namespace prism
{
    export class TokenCursor
    {
      public:
        constexpr explicit TokenCursor(const std::span<const Token> tokens) : tokens_{tokens}
        {
        }

        [[nodiscard]] constexpr bool at_end() const
        {
            return index_ >= tokens_.size();
        }

        [[nodiscard]] constexpr const Token &current() const
        {
            return tokens_[index_];
        }

        [[nodiscard]] constexpr const Token *peek(const std::size_t offset = 1) const
        {
            const auto target_index = index_ + offset;
            return target_index < tokens_.size() ? &tokens_[target_index] : nullptr;
        }

        constexpr void advance()
        {
            ++index_;
        }

        [[nodiscard]] constexpr bool match(const TokenKind kind) const
        {
            return current().kind == kind;
        }

        [[nodiscard]] constexpr bool match_ahead(const TokenKind kind, const std::size_t offset) const
        {
            const auto target_index = index_ + offset;
            return target_index < tokens_.size() && tokens_[target_index].kind == kind;
        }

      private:
        std::span<const Token> tokens_;
        std::size_t index_ = 0;
    };
} // namespace prism
