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

        return node_->green().is_list() ? node_->green().slot_count() : 1;
    }

    SyntaxNodeOrToken SyntaxNodeOrTokenList::operator[](const std::size_t index) const
    {
        if (node_ == nullptr)
            throw std::out_of_range{"Index out of range"};

        if (!node_->green().is_list())
        {
            if (index > 0)
                throw std::out_of_range{"Index out of range"};

            return *node_;
        }

        if (index >= node_->green().slot_count())
            throw std::out_of_range{"Index out of range"};

        if (auto &green = node_->green().get_required_slot<GreenToken>(index); green.is_token())
        {
            return SyntaxToken{green, parent().value_ptr(), node_->get_slot_position(index)};
        }

        return node_->get_required_node_slot(index);
    }

    Optional<const SyntaxNode &> SyntaxNodeOrTokenList::parent() const
    {
        ASSUME(node_ != nullptr);
        return node_->parent();
    }
} // namespace prism
