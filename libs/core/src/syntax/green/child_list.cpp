/**
 * @file child_list.cpp
 * @author Francesco Corso
 * @date 7/26/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:syntax.green.child_list.impl;

import :syntax.green.child_list;

namespace prism
{

    GreenChildIterator &GreenChildIterator::operator++()
    {
        if (list_ != nullptr)
        {
            if (++list_index_ < list_->slot_count())
            {
                current_child_ = list_->get_slot(list_index_).value_ptr();
                return *this;
            }

            list_ = nullptr;
            list_index_ = 0;
        }

        ++slot_index;
        move_to_next();

        return *this;
    }

    GreenChildIterator &GreenChildIterator::operator--()
    {
        if (current_child_ == nullptr)
        {
            slot_index = node_->slot_count();
        }

        if (list_ != nullptr)
        {
            if (list_index_ > 0)
            {
                --list_index_;
                current_child_ = list_->get_slot(list_index_).value_ptr();
                return *this;
            }

            list_ = nullptr;
        }

        while (slot_index > 0)
        {
            --slot_index;

            auto child = node_->get_slot(slot_index);
            if (!child.has_value())
                continue;

            if (child->is_list())
            {
                list_ = child.value_ptr();
                list_index_ = list_->slot_count();

                if (list_index_ > 0)
                {
                    --list_index_;
                    current_child_ = list_->get_slot(list_index_).value_ptr();
                    return *this;
                }

                list_ = nullptr;
                continue;
            }

            current_child_ = child.value_ptr();
            return *this;
        }

        current_child_ = nullptr;
        return *this;
    }

    void GreenChildIterator::move_to_next()
    {
        while (slot_index < node_->slot_count())
        {
            auto child = node_->get_slot(slot_index);

            if (!child.has_value())
            {
                slot_index++;
                continue;
            }

            if (child->is_list())
            {
                list_ = child.value_ptr();
                list_index_ = 0;

                if (list_index_ < list_->slot_count())
                {
                    current_child_ = list_->get_slot(list_index_).value_ptr();
                    DEBUG_ASSERT(!current_child_->is_list(), "Cannot hold a list as a distinct element");
                    return;
                }

                list_ = nullptr;
                slot_index++;
                continue;
            }

            current_child_ = child.value_ptr();
            return;
        }

        current_child_ = nullptr;
    }

    std::size_t GreenChildList::size() const noexcept
    {
        if (size_.has_value())
            return *size_;

        size_ = count_nodes();
        return *size_;
    }

    std::uint32_t GreenChildList::count_nodes() const
    {
        return static_cast<std::uint32_t>(std::ranges::distance(begin(), end()));
    }
} // namespace prism
