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
        const auto position =
            parent_.position_ +
            green_.node().transform([index](const GreenNode &node) { return node.get_slot_offset(index); }).value_or(0);
        return SyntaxTrivia{parent_, element, position};
    }

    StructuredTriviaSyntax &StructuredTriviaSyntax::create(SyntaxLifetime &lifetime, const SyntaxTrivia &trivia)
    {
        auto *parent = trivia.token_.parent().value_ptr();
        auto &red =
            static_cast<StructuredTriviaSyntax &>(trivia.green_->create_red(lifetime, parent, trivia.position_));
        red.parent_trivia_ = trivia;
        return red;
    }
} // namespace prism
