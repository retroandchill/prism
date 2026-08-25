/**
 * @file node_or_token.cpp
 * @author Francesco Corso
 * @date 7/22/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:syntax.node_or_token.impl;

import :syntax.node_or_token;
import :syntax.node;

namespace prism
{
    Optional<const SyntaxTree &> get_tree(const SyntaxNodeOrToken &node_or_token) noexcept
    {
        return std::visit(Overload{[](const SyntaxNode &node) -> Optional<const SyntaxTree &> { return node.tree(); },
                                   [](const SyntaxToken &token)
                                   {
                                       return token.tree();
                                   }},
                          node_or_token);
    }

    TextSpan get_span(const SyntaxNodeOrToken &node_or_token) noexcept
    {
        return std::visit(Overload{[](const SyntaxNode &node) { return node.span(); },
                                   [](const SyntaxToken &token)
                                   {
                                       return token.span();
                                   }},
                          node_or_token);
    }

    std::size_t SyntaxNodeOrTokenList::size() const noexcept
    {
        if (node_ == nullptr)
            return 0;

        return SyntaxNodeInternal::get_green(*node_).is_list() ? SyntaxNodeInternal::get_green(*node_).slot_count() : 1;
    }

    SyntaxNodeOrToken SyntaxNodeOrTokenList::operator[](const std::size_t index) const
    {
        if (node_ == nullptr)
            throw std::out_of_range{"Index out of range"};

        if (!SyntaxNodeInternal::get_green(*node_).is_list())
        {
            if (index > 0)
                throw std::out_of_range{"Index out of range"};

            return *node_;
        }

        if (index >= SyntaxNodeInternal::get_green(*node_).slot_count())
            throw std::out_of_range{"Index out of range"};

        if (auto green = SyntaxNodeInternal::get_green(*node_).get_slot<GreenToken>(index); green.has_value())
        {
            return SyntaxToken{*green, parent().value_ptr(), SyntaxNodeInternal::get_slot_position(*node_, index)};
        }

        return SyntaxNodeInternal::get_required_node_slot(*node_, index);
    }

    Optional<const SyntaxNode &> SyntaxNodeOrTokenList::parent() const
    {
        ASSUME(node_ != nullptr);
        return node_->parent();
    }
} // namespace prism
