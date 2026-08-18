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
            SyntaxTokenInternal::get_position(parent_) +
            green_.node().transform([index](const GreenNode &node) { return node.get_slot_offset(index); }).value_or(0);
        return SyntaxTriviaInternal::create(parent_, element, position);
    }

    StructuredTriviaSyntax &StructuredTriviaSyntax::create(SyntaxLifetime &lifetime, const SyntaxTrivia &trivia)
    {
        auto *parent = trivia.token().parent().value_ptr();
        auto &red = static_cast<StructuredTriviaSyntax &>(
            SyntaxTriviaInternal::get_green(trivia).create_red(lifetime,
                                                               parent,
                                                               SyntaxTriviaInternal::get_position(trivia)));
        red.parent_trivia_ = trivia;
        return red;
    }
} // namespace prism
