/**
 * @file child_list.cpp
 * @author Francesco Corso
 * @date 7/21/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:syntax.child_list.impl;

import :syntax.child_list;

namespace prism
{
    SyntaxNodeOrToken ChildSyntaxList::operator[](const std::size_t index) const
    {
        if (index >= size()) [[unlikely]]
            throw std::out_of_range{"Index out of range"};

        return get_item(*node_, index);
    }

    std::size_t ChildSyntaxList::count_nodes(const GreenNode &node) noexcept
    {
        std::size_t n = 0;

        for (std::size_t i = 0, s = node.child_count(); i < s; ++i)
        {
            if (auto &child = *node.get_child(i); !child.is_list())
            {
                n += child.child_count();
            }
            else
            {
                n++;
            }
        }

        return n;
    }

    SyntaxNodeOrToken ChildSyntaxList::get_item(const SyntaxNode &node, std::size_t index)
    {
        auto slot_data = SlotData{.position_at_slot_index = node.position()};

        return get_item(node, index, slot_data);
    }

    SyntaxNodeOrToken ChildSyntaxList::get_item(const SyntaxNode &node, std::size_t index, SlotData &slot_data)
    {
        Optional<const GreenNode &> green_child;
        auto &green = node.green();

        DEBUG_ASSERT(index > slot_data.preceding_occupant_slot_count);
        auto idx = index - slot_data.preceding_occupant_slot_count;
        auto slot_index = slot_data.slot_index;
        auto position = slot_data.slot_index;

        while (true)
        {
            green_child = green.get_child(slot_index);
            if (green_child.has_value())
            {
                auto current_occupancy = occupancy(*green_child);
                if (idx < current_occupancy)
                    break;

                idx -= current_occupancy;
                position += green_child->full_width();
            }

            slot_index++;
        }

        if (slot_index != slot_data.slot_index)
        {
            slot_data = {.slot_index = slot_index,
                         .preceding_occupant_slot_count = index - idx,
                         .position_at_slot_index = position};
        }

        auto red = node.get_node_slot(slot_index);
        if (!green_child->is_list())
        {
            if (red.has_value())
                return *red;
        }
        else if (red.has_value())
        {
            auto red_child = red->get_node_slot(idx);
            if (red_child.has_value())
                return *red_child;

            green_child = green.get_child(idx);
            position = red->get_child_position(idx);
        }
        else
        {
            position += green_child->get_child_offset(idx);
            green_child = green_child->get_child(idx);
        }

        ASSUME(green_child->is<GreenToken>(), "The child must be a token");
        DEBUG_ASSERT(position <= std::numeric_limits<std::uint32_t>::max());
        return SyntaxToken{static_cast<const GreenToken &>(*green_child), &node, static_cast<std::uint32_t>(position)};
    }

    std::size_t ChildSyntaxList::occupancy(const GreenNode &node)
    {
        return node.is_list() ? node.child_count() : 1;
    }
} // namespace prism
