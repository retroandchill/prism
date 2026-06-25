/**
 * @file defaults.cpp
 * @author Francesco Corso
 * @date 6/21/2026
 * @brief
 */
module prism.core.lexer.defaults;

import prism.core.lexer.matchers;

namespace prism
{
    namespace
    {
        template <std::derived_from<TokenMatcher>... T>
            requires(std::is_default_constructible_v<T> && ...)
        std::vector<std::unique_ptr<TokenMatcher>> create_matchers()
        {
            std::vector<std::unique_ptr<TokenMatcher>> matchers;
            matchers.reserve(sizeof...(T));

            (matchers.emplace_back(std::make_unique<T>()), ...);

            return matchers;
        }
    } // namespace

    std::vector<std::unique_ptr<TokenMatcher>> default_matchers()
    {
        return create_matchers<CommentMatcher,
                               IdentifierMatcher,
                               NumberMatcher,
                               OperatorMatcher,
                               StringLiteralMatcher>();
    }

    Lexer default_lexer()
    {
        Lexer lexer;
        lexer.add_matchers(default_matchers() | std::views::as_rvalue);
        return lexer;
    }
} // namespace prism
