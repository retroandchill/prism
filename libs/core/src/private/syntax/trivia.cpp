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

        auto &element = green_->get_child_unchecked<GreenTrivia>(index);
        return SyntaxTrivia{parent_, element.shared_from_this(), green_->get_child_offset(index)};
    }
} // namespace prism
