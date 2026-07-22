/**
 * @file node.cpp
 * @author Francesco Corso
 * @date 7/20/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:syntax.node.impl;

import :syntax.node;
import :syntax.tree;
import :syntax.child_list;
import :syntax.trivia;
import :util.overload;

namespace prism
{

    SyntaxTriviaList SyntaxNode::leading_trivia() const noexcept
    {
        return first_token().leading_trivia();
    }

    SyntaxTriviaList SyntaxNode::trailing_trivia() const noexcept
    {
        return last_token().trailing_trivia();
    }

    const SyntaxTree &SyntaxNode::tree() const
    {
        auto *tree = tree_.load(std::memory_order_acquire);
        if (tree == nullptr)
        {
            tree = compute_tree(this);
        }

        DEBUG_ASSERT(tree != nullptr);

        return *tree;
    }

    SyntaxLifetime &SyntaxNode::lifetime() const
    {
        return *tree().lifetime_;
    }
    const SyntaxNode &SyntaxNode::get_required_node_slot(std::size_t index) const
    {
        return get_node_slot(index).value();
    }

    std::uint32_t SyntaxNode::get_child_position(std::size_t index) const
    {
        if (const auto cached = get_cached_slot(index); cached.has_value())
        {
            return cached->position_;
        }

        std::uint32_t offset = 0;
        auto *green = green_;
        while (index > 0)
        {
            index--;
            if (const auto prev_sibling = get_cached_slot(index); prev_sibling.has_value())
            {
                return prev_sibling->end_position() + offset;
            }

            if (const auto green_child = green->get_child(index); green_child.has_value())
            {
                offset += green_child->full_width();
            }
        }

        return position_ + offset;
    }

    ChildSyntaxList SyntaxNode::child_nodes_and_tokens() const
    {
        return ChildSyntaxList{*this};
    }

    std::generator<const SyntaxNode &> SyntaxNode::child_nodes() const
    {
        for (const auto child : child_nodes_and_tokens())
        {
            auto *node = get_if<SyntaxNodeRef>(&child);
            if (node != nullptr)
                co_yield node->get();
        }
    }

    std::generator<SyntaxToken> SyntaxNode::child_tokens() const
    {
        for (const auto child : child_nodes_and_tokens())
        {
            if (auto *token = get_if<SyntaxToken>(&child); token != nullptr)
                co_yield *token;
        }
    }

    SyntaxToken SyntaxNode::first_token() const
    {
        auto first = child_nodes_and_tokens().first();
        const auto *token = get_if<SyntaxToken>(&first);
        while (token == nullptr)
        {
            first = get<SyntaxNodeRef>(first).get().child_nodes_and_tokens().first();
            token = get_if<SyntaxToken>(&first);
        }

        return *token;
    }

    SyntaxToken SyntaxNode::last_token() const
    {
        auto last = child_nodes_and_tokens().last();
        const auto *token = get_if<SyntaxToken>(&last);
        while (token == nullptr)
        {
            last = get<SyntaxNodeRef>(last).get().child_nodes_and_tokens().last();
            token = get_if<SyntaxToken>(&last);
        }

        return *token;
    }

    const SyntaxTree *SyntaxNode::compute_tree(const SyntaxNode *node)
    {
        std::vector<const SyntaxNode *> nodes;
        const SyntaxTree *tree = nullptr;

        while (true)
        {
            tree = node->tree_.load(std::memory_order_acquire);
            if (tree != nullptr)
                break;

            auto *parent = node->parent_;
            if (parent == nullptr)
            {
                // TODO: Eventually we do want to allow for the lazy creation of syntax trees for orphan nodes
                // but doing so will require the memory model to be updated to support that, and for getting a
                // working compiler, that is not necessary at this time.
                throw InvalidStateException{"Parent node has no syntax tree defined"};
            }

            tree = parent->tree_.load(std::memory_order_acquire);
            if (tree != nullptr)
            {
                node->tree_.store(tree, std::memory_order_release);
                break;
            }

            nodes.push_back(node);
            node = parent;
        }

        ASSUME(tree != nullptr);

        for (auto *n : nodes)
        {
            if (auto *existing_tree = n->tree_.load(std::memory_order_acquire); existing_tree != nullptr)
            {
                DEBUG_ASSERT(existing_tree == tree);
                break;
            }

            n->tree_.store(tree, std::memory_order_release);
        }

        return tree;
    }
} // namespace prism
