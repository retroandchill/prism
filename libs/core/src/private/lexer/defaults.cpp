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
    std::vector<std::unique_ptr<TokenMatcher>> default_matchers()
    {
        std::vector<std::unique_ptr<TokenMatcher>> matchers;
        matchers.reserve(3);

        matchers.emplace_back(std::make_unique<IdentifierMatcher>());
        matchers.emplace_back(std::make_unique<NumberMatcher>());
        matchers.emplace_back(std::make_unique<OperatorMatcher>());

        return matchers;
    }

    Lexer default_lexer()
    {
        Lexer lexer;
        lexer.add_matchers(default_matchers() | std::views::as_rvalue);
        return lexer;
    }
} // namespace prism
