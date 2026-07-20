/**
 * @file node.cpp
 * @author Francesco Corso
 * @date 7/20/2026
 * @brief
 */
module;

#include <cassert>

module prism.core:syntax.node.impl;

import :syntax.node;
import :syntax.tree;

namespace prism
{

    std::shared_ptr<const SyntaxTree> SyntaxNode::syntax_tree() const noexcept
    {
        auto tree = tree_.load(std::memory_order_acquire).lock();
        if (tree == nullptr)
        {
            tree = compute_tree(this);
        }

        assert(tree != nullptr);
        return tree;
    }

    std::shared_ptr<const SyntaxTree> SyntaxNode::compute_tree(const SyntaxNode *node)
    {
        std::vector<const SyntaxNode *> nodes;
        std::shared_ptr<const SyntaxTree> tree;

        while (true)
        {
            auto observed = node->tree_.load(std::memory_order_acquire);
            tree = observed.lock();
            if (tree != nullptr)
                break;

            auto parent = node->parent();
            if (!parent.has_value())
            {
                auto new_tree = std::make_shared<SyntaxTree>(SyntaxTree::no_clone, node->shared_from_this());
                if (node->tree_.compare_exchange_strong(observed,
                                                        new_tree,
                                                        std::memory_order_release,
                                                        std::memory_order_acquire))
                {
                    tree = std::move(new_tree);
                    break;
                }

                tree = observed.lock();
                if (tree != nullptr)
                    break;

                continue;
            }

            tree = parent->tree_.load(std::memory_order_acquire).lock();
            if (tree != nullptr)
            {
                node->tree_.store(tree, std::memory_order_release);
            }

            nodes.push_back(node);
            node = parent.value_ptr();
        }

        assert(tree != nullptr);

        for (auto *n : nodes)
        {
            auto existing_tree = n->tree_.load(std::memory_order_acquire).lock();
            if (existing_tree != nullptr)
            {
                assert(existing_tree == tree);
                break;
            }
            n->tree_.store(tree, std::memory_order_release);
        }

        return tree;
    }
} // namespace prism
