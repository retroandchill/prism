/**
 * @file token_matcher.ixx
 * @author Francesco Corso
 * @date 6/21/2026
 * @brief Contains the interface declaration for a simple token matcher
 */
export module prism.core.lexer.token_matcher;

import std;
import prism.core.lexer.text_cursor;
import prism.core.lexer.token;

namespace prism
{
    export class TokenMatcher
    {
      protected:
        constexpr explicit TokenMatcher(const std::int32_t priority = 0) : priority_{priority}
        {
        }

      public:
        TokenMatcher(const TokenMatcher &) = delete;
        TokenMatcher(TokenMatcher &&) = delete;
        virtual ~TokenMatcher() = default;
        TokenMatcher &operator=(const TokenMatcher &) = delete;
        TokenMatcher &operator=(TokenMatcher &&) = delete;

        [[nodiscard]] constexpr std::int32_t priority() const noexcept
        {
            return priority_;
        }

        [[nodiscard]] virtual std::optional<Token> try_match(TextCursor &cursor) const = 0;

      private:
        std::int32_t priority_ = 0;
    };

    export struct PriorityLess
    {
        constexpr bool operator()(const TokenMatcher &lhs, const TokenMatcher &rhs) const noexcept
        {
            return lhs.priority() < rhs.priority();
        }

        constexpr bool operator()(const std::unique_ptr<TokenMatcher> &lhs,
                                  const std::unique_ptr<TokenMatcher> &rhs) const noexcept
        {
            return lhs->priority() < rhs->priority();
        }
    };

    export constexpr PriorityLess priority_less{};
} // namespace prism
