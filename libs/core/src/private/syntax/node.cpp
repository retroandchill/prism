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

namespace prism
{

    const SyntaxTree &SyntaxNode::tree() const
    {
        auto *tree = tree_.load(std::memory_order_acquire);
        if (tree == nullptr)
        {
            tree = compute_tree(this);
        }

        assert(tree != nullptr);

        return *tree;
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

        assert(tree != nullptr);

        for (auto *n : nodes)
        {
            if (auto *existing_tree = n->tree_.load(std::memory_order_acquire); existing_tree != nullptr)
            {
                assert(existing_tree == tree);
                break;
            }

            n->tree_.store(tree, std::memory_order_release);
        }

        return tree;
    }
} // namespace prism
