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
import libassert;

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

    SyntaxNode &GreenListNode::create_red(SyntaxLifetime &, const SyntaxNode *, std::uint32_t) const
    {
        throw UnsupportedOperationException{};
    }

    RefCountPtr<GreenNode> GreenListNode::clone_internal() const
    {
        return make_ref_counted<GreenListNode>(children_);
    }
} // namespace prism
