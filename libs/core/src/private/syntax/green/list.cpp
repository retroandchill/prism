/**
 * @file list.cpp
 * @author Francesco Corso
 * @date 7/13/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:syntax.green.list.impl;

import :syntax.green.list;
import :syntax.list;
import :syntax.lifetime;

namespace prism
{
    GreenListNode::GreenListNode(GreenSyntaxVector children)
        : GreenNode{SyntaxKind::list}, children_{std::move(children)}
    {
        DEBUG_ASSERT(children_.size() <= std::numeric_limits<std::uint32_t>::max());
        set_slot_count(static_cast<std::uint32_t>(children_.size()));
        for (auto &child : children_)
        {
            DEBUG_ASSERT(child != nullptr);
        }
    }

    GreenPtr<GreenListNode> GreenListNode::with_slot(size_t index, GreenPtr<GreenNode> slot) const
    {
        ASSUME(index < children_.size());
        if (children_[index] == slot)
        {
            return shared_from_this();
        }

        auto copy = clone();
        copy->set_slot(index, std::move(slot));
        return std::move(copy);
    }

    SyntaxNode &GreenListNode::create_red(SyntaxLifetime &lifetime,
                                          const SyntaxNode *parent,
                                          const std::uint32_t position) const
    {
        return lifetime.add<SyntaxListImpl>(*this, parent, position);
    }

    RefCountPtr<GreenNode> GreenListNode::clone_internal() const
    {
        return make_ref_counted<GreenListNode>(children_);
    }
} // namespace prism
