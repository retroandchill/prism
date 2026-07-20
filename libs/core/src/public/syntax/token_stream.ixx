/**
 * @file token_stream.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
module;

#include <cassert>

export module prism.core:syntax.token_stream;

import :syntax.lexer;

namespace prism
{
    class TokenStream final
    {
      public:
        explicit constexpr TokenStream(const std::string_view text) : lexer_{text}
        {
        }

        [[nodiscard]] bool at_end();

        [[nodiscard]] constexpr const std::vector<GreenPtr<GreenToken>> &tokens() const
        {
            return tokens_;
        }

        [[nodiscard]] constexpr const GreenToken &previous() const
        {
            assert(!tokens_.empty());
            return *tokens_.back();
        }

        [[nodiscard]] const GreenToken &peek(std::size_t count = 1);

        void advance();

        void replace_next(GreenPtr<GreenToken> token);

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, GreenPtr<GreenToken>>
        void replace_next(Range &&range)
        {
            [[maybe_unused]] const auto &next = peek();
            assert(next.kind() != SyntaxKind::eof_token);

            lookahead_.pop_front();
            lookahead_.append_range(std::forward<Range>(range));
        }

      private:
        void buffer_tokens();

        Lexer lexer_;
        std::vector<GreenPtr<GreenToken>> tokens_;
        std::deque<GreenPtr<GreenToken>> lookahead_;
    };
} // namespace prism
