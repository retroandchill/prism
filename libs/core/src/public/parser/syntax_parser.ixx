/**
 * @file syntax_parser.ixx
 * @author Francesco Corso
 * @date 7/24/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:parser.syntax_parser;

import :parser.token_stream;
import :syntax.green.helpers;

namespace prism
{
    class SyntaxParser
    {
      protected:
        explicit constexpr SyntaxParser(const std::string_view text) : stream_{text}
        {
        }

        const GreenToken &current_token();
        const GreenToken &peek_token(int offset = 1);
        GreenPtr<GreenToken> eat_token();

        template <std::derived_from<GreenNode> T>
            requires(!std::is_const_v<T>)
        static void add_trailing_skipped_syntax(T &node, const GreenNode &skipped_syntax)
        {
            if constexpr (std::derived_from<T, GreenToken>)
            {
                add_skipped_syntax(static_cast<GreenToken &>(node), skipped_syntax, true);
            }
            else
            {
                auto last_token = node.last_token();
                DEBUG_ASSERT(last_token.has_value());
                auto copy = last_token->clone();
                add_skipped_syntax(*copy, skipped_syntax, true);
                replace_last_token(node, std::move(copy));
            }
        }

        static void add_skipped_syntax(GreenToken &target, const GreenNode &skipped_syntax, bool trailing);

      private:
        TokenStream stream_;
    };
} // namespace prism
