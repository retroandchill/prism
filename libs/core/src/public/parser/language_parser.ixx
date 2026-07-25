/**
 * @file language_parser.ixx
 * @author Francesco Corso
 * @date 7/24/2026
 * @brief
 */
export module prism.core:parser.language_parser;

import :parser.syntax_parser;
import :syntax.green.top_level;

namespace prism
{
    class LanguageParser final : public SyntaxParser
    {
      public:
        explicit constexpr LanguageParser(const std::string_view text) : SyntaxParser{text}
        {
        }

        template <std::derived_from<GreenNode> T>
        GreenPtr<T> consume_unexpected_tokens(GreenPtr<T> node)
        {
            if (current_token().kind() == SyntaxKind::eof_token)
                return std::move(node);

            GreenListBuilder<GreenToken> builder;
            while (current_token().kind() != SyntaxKind::eof_token)
            {
                builder.add(eat_token());
            }

            const auto list = std::move(builder).build();

            node = add_trailing_skipped_syntax(std::move(node), list.node()->shared_from_this());
            return node;
        }

        GreenPtr<GreenCompilationUnit> parse_compilation_unit();
    };
} // namespace prism
