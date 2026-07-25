/**
 * @file syntax_parser.ixx
 * @author Francesco Corso
 * @date 7/24/2026
 * @brief
 */
export module prism.core:parser.syntax_parser;

import :parser.token_stream;

namespace prism
{
    class SyntaxParser
    {
      protected:
        explicit constexpr SyntaxParser(const std::string_view text) : stream_{text}
        {
        }

        template <std::derived_from<GreenNode> T>
        GreenPtr<T> add_trailing_skipped_syntax(GreenPtr<T> node, GreenPtr<GreenNode> skipped_syntax);

        template <std::derived_from<GreenNode> T, typename... Args>
            requires(sizeof...(Args) > 0 && (std::convertible_to<RefCountPtr<const DiagnosticInfo>, Args> && ...))
        GreenPtr<T> add_diagnostics(GreenPtr<T> node, Args &&...args);

        const GreenToken &current_token();
        const GreenToken &peek_token(int offset = 1);
        GreenPtr<GreenToken> eat_token();

      private:
        TokenStream stream_;
    };
} // namespace prism
