/**
 * @file token.cpp
 * @author Francesco Corso
 * @date 7/20/2026
 * @brief
 */
module prism.core:syntax.token.impl;

import :syntax.token;
import :syntax.tree;
import :syntax.trivia;

namespace prism
{
    Optional<const SyntaxTree &> SyntaxToken::tree() const
    {
        return parent().transform([](const SyntaxNode &node) -> auto & { return node.tree(); });
    }

    SyntaxTriviaList SyntaxToken::leading_trivia() const noexcept
    {
        auto &trivia = green_->leading_trivia();
        return SyntaxTriviaList{*this, trivia};
    }

    SyntaxTriviaList SyntaxToken::trailing_trivia() const noexcept
    {
        auto &trivia = green_->trailing_trivia();
        return SyntaxTriviaList{*this, trivia};
    }

    SyntaxToken SyntaxTokenList::operator[](std::size_t index) const
    {
        return SyntaxToken{green_[index], parent_, position_};
    }

} // namespace prism
