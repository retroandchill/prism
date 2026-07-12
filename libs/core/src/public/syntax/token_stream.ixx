/**
 * @file token_stream.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

#include <cassert>

export module prism.core:syntax.token_stream;

import :syntax.lexer;

namespace prism
{
    export class PRISM_CORE_API TokenStream final
    {
      public:
        explicit constexpr TokenStream(const std::string_view text) : lexer_{text}
        {
        }

        [[nodiscard]] bool at_end();

        [[nodiscard]] constexpr const std::vector<SyntaxToken> &tokens() const
        {
            return tokens_;
        }

        [[nodiscard]] constexpr const SyntaxToken &previous() const
        {
            assert(!tokens_.empty());
            return tokens_.back();
        }

        [[nodiscard]] const SyntaxToken &peek(std::size_t count = 1);

        void advance();

        void replace_next(SyntaxToken token);

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, SyntaxToken>
        void replace_next(Range &&range)
        {
            [[maybe_unused]] const auto &next = peek();
            assert(next.kind() != SyntaxKind::end_of_file_token);

            lookahead_.pop_front();
            lookahead_.append_range(std::forward<Range>(range));
        }

      private:
        void buffer_tokens();

        Lexer lexer_;
        std::vector<SyntaxToken> tokens_;
        std::deque<SyntaxToken> lookahead_;
    };
} // namespace prism
