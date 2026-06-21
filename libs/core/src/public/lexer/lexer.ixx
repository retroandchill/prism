/**
 * @file lexer.ixx
 * @author Francesco Corso
 * @date 6/20/2026
 * @brief Defines the lexer class that holds the state of the parse
 */
module;

#include "prism/core/exports.h"

export module prism.core.lexer;

import std;
import prism.core.lexer.text_cursor;
import prism.core.lexer.token;
import prism.core.source.source_file;
import prism.core.lexer.token_matcher;

namespace prism
{
    export class PRISM_CORE_API Lexer
    {
      public:
        void add_matcher(std::unique_ptr<TokenMatcher> matcher);

        template <std::ranges::input_range Range>
            requires std::constructible_from<std::unique_ptr<TokenMatcher>, std::ranges::range_reference_t<Range>>
        void add_matchers(Range &&range)
        {
            if (std::ranges::empty(range))
                return;

            const auto old_end_index = matchers_.size();

            if constexpr (std::ranges::sized_range<Range>)
            {
                matchers_.reserve(std::ranges::size(range));
            }

            if constexpr (std::ranges::common_range<Range>)
            {
                matchers_.append_range(std::forward<Range>(range));
            }
            else
            {
                for (auto &&matcher : std::forward<Range>(range))
                {
                    matchers_.push_back(std::forward<decltype(matcher)>(matcher));
                }
            }

            const auto old_end_it = std::next(matchers_.begin(), static_cast<std::ptrdiff_t>(old_end_index));
            std::sort(old_end_it, matchers_.end(), priority_less);
            std::ranges::inplace_merge(matchers_, old_end_it, priority_less);
        }

        [[nodiscard]] std::vector<Token> lex(const SourceFile &source_file) const;

      private:
        std::vector<std::unique_ptr<TokenMatcher>> matchers_;
    };
} // namespace prism
