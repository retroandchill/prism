/**
 * @file trivia.cpp
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
module prism.core:syntax.trivia.impl;

import :syntax.trivia;

namespace prism
{

    SyntaxTrivia SyntaxTriviaList::operator[](const std::size_t index) const
    {
        if (index >= size())
            throw std::out_of_range{"Invalid index"};

        auto &element = green_[index];
        auto position =
            parent_.position_ + green_.node()
                                    .transform([index](const GreenNode &node) { return node.get_child_offset(index); })
                                    .value_or(0);
        return SyntaxTrivia{parent_, element, position};
    }
} // namespace prism
