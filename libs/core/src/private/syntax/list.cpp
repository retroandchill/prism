/**
 * @file list.cpp
 * @author Francesco Corso
 * @date 7/21/2026
 * @brief
 */
export module prism.core:syntax.list.impl;

import :syntax.list;
import :syntax.lifetime;

namespace prism
{

    std::span<std::atomic<const SyntaxNode *>> SyntaxListImpl::create_children() const
    {
        return lifetime().allocate_child_slots(green().child_count());
    }

    Optional<const SyntaxNode &> SyntaxListImpl::get_node_slot(const std::size_t index) const
    {
        return get_red(children_[index], index);
    }

    Optional<const SyntaxNode &> SyntaxListImpl::get_cached_slot(const std::size_t index) const
    {
        return children_[index].load(std::memory_order_acquire);
    }
} // namespace prism
