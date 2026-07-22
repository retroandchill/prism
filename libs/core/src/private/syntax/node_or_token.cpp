/**
 * @file node_or_token.cpp
 * @author Francesco Corso
 * @date 7/22/2026
 * @brief
 */
module prism.core:syntax.node_or_token.impl;

import :syntax.node_or_token;

namespace prism
{

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

        if (index >= node_->green().child_count())
            throw std::out_of_range{"Index out of range"};

        if (auto &green = node_->green().get_required_child<GreenToken>(index); green.is_token())
        {
            return SyntaxToken{green, parent().value_ptr(), node_->get_child_position(index)};
        }

        return node_->get_required_node_slot(index);
    }
} // namespace prism
