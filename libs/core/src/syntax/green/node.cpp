/**
 * @file node.cpp
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
module prism.core:syntax.green.node.impl;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.trivia;
import :syntax.green.child_list;
import :syntax.green.view;
import :diagnostics.info;

namespace prism
{
    std::uint32_t GreenNode::width() const
    {
        return full_width_ - leading_trivia_width() - trailing_trivia_width();
    }

    Optional<const GreenNode &> GreenNode::leading_trivia() const
    {
        if (full_width_ == 0)
            return std::nullopt;

        return first_terminal()->leading_trivia();
    }

    std::uint32_t GreenNode::leading_trivia_width() const
    {
        return full_width_ > 0 ? first_terminal()->leading_trivia_width() : 0;
    }

    Optional<const GreenNode &> GreenNode::trailing_trivia() const
    {
        if (full_width_ == 0)
            return std::nullopt;

        return last_terminal()->trailing_trivia();
    }

    std::uint32_t GreenNode::trailing_trivia_width() const
    {
        return full_width_ > 0 ? last_terminal()->trailing_trivia_width() : 0;
    }

    Optional<const GreenNode &> GreenNode::first_terminal() const
    {
        auto *node = this;

        do
        {
            const GreenNode *first_child = nullptr;
            for (const std::size_t i : std::views::iota(0uz, node->slot_count()))
            {
                auto child = node->get_slot(i);
                if (!child.has_value())
                    continue;

                first_child = child.value_ptr();
                break;
            }

            node = first_child;
        } while (node != nullptr && node->slot_count() > 0);

        return node;
    }

    Optional<const GreenToken &> GreenNode::first_token() const
    {
        return first_terminal().and_then([](const GreenNode &node) { return node.as<GreenToken>(); });
    }

    Optional<const GreenNode &> GreenNode::last_terminal() const
    {
        auto *node = this;

        do
        {
            const GreenNode *last_child = nullptr;

            for (std::size_t i = node->slot_count(); i > 0; --i)
            {
                auto child = node->get_slot(i - 1);
                if (!child.has_value())
                    continue;

                last_child = child.value_ptr();
                break;
            }

            node = last_child;

        } while (node != nullptr && node->slot_count() > 0);

        return node;
    }

    Optional<const GreenToken &> GreenNode::last_token() const
    {
        return last_terminal().and_then([](const GreenNode &node) { return node.as<GreenToken>(); });
    }

    std::uint32_t GreenNode::get_slot_offset(const std::size_t index) const
    {
        std::uint32_t offset = 0;
        for (std::size_t i = 0; i < index; ++i)
        {
            auto child = get_slot(i);
            if (!child.has_value())
                continue;

            offset += child->full_width();
        }

        return offset;
    }

    GreenChildList GreenNode::child_nodes_and_tokens() const
    {
        return GreenChildList{*this};
    }

    GreenNodeView GreenNode::enumerate_nodes() const
    {
        return GreenNodeView{*this};
    }

    void GreenNode::add_diagnostic(std::shared_ptr<const DiagnosticInfo> diagnostic)
    {
        diagnostics_ = diagnostics_.add(std::move(diagnostic));
        if (diagnostics_.size() > 1)
        {
            flags_ |= SyntaxFlags::contains_diagnostics;
        }
    }

    void GreenNode::write_to(TextWriter &writer) const
    {
        for (const std::size_t i : std::views::iota(0uz, child_count_))
        {
            auto child = get_slot(i);
            if (child.has_value())
                continue;

            child->write_to(writer);
        }
    }

    void GreenNode::adjust_flags_and_width(const GreenNode &node)
    {
        set_flags(node.flags() & SyntaxFlags::inherit_mask);
        full_width_ += node.full_width();
    }
} // namespace prism
