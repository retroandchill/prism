/**
 * @file green_node.cpp
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
module prism.core:syntax.green.green_node.impl;

import :syntax.green.green_node;

namespace prism
{

    std::uint32_t GreenNode::leading_trivia_width() const
    {
        return full_width_ > 0 ? first_leaf()->leading_trivia_width() : 0;
    }

    std::uint32_t GreenNode::trailing_trivia_width() const
    {
        return full_width_ > 0 ? last_leaf()->trailing_trivia_width() : 0;
    }

    const GreenNode *GreenNode::first_leaf() const
    {
        auto *node = this;

        do
        {
            const GreenNode *first_child = nullptr;
            for (const std::size_t i : std::views::iota(0uz, node->child_count()))
            {
                auto *child = node->get_child(i);
                if (child == nullptr)
                    continue;

                first_child = child;
                break;
            }

            node = first_child;
        } while (node != nullptr && node->child_count() > 0);

        return node;
    }

    const GreenNode *GreenNode::last_leaf() const
    {
        auto *node = this;

        do
        {
            const GreenNode *last_child = nullptr;

            for (std::size_t i = node->child_count() - 1; i != std::numeric_limits<std::size_t>::max(); --i)
            {
                auto *child = get_child(i);
                if (child == nullptr)
                    continue;

                last_child = child;
                break;
            }

            node = last_child;

        } while (node != nullptr && node->child_count() > 0);

        return node;
    }

    std::uint32_t GreenNode::get_child_offset(const std::size_t index) const
    {
        std::uint32_t offset = 0;
        for (std::size_t i = 0; i < index; ++i)
        {
            auto *child = get_child(i);
            if (child == nullptr)
                continue;

            offset += child->full_width();
        }

        return offset;
    }

    void GreenNode::add_diagnostic(RefCountPtr<DiagnosticInfo> diagnostic)
    {
        diagnostics_.push_back(std::move(diagnostic));
        if (diagnostics_.size() > 1)
        {
            flags_ |= SyntaxFlags::contains_diagnostics;
        }
    }
} // namespace prism
