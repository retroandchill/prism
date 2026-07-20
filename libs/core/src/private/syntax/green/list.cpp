/**
 * @file list.cpp
 * @author Francesco Corso
 * @date 7/13/2026
 * @brief
 */
module;

#include <cassert>

module prism.core:syntax.green.list.impl;

import :syntax.green.list;

namespace prism
{
    GreenListNode::GreenListNode(GreenSyntaxVector children)
        : GreenNode{SyntaxKind::list}, children_{std::move(children)}
    {
        assert(children_.size() <= std::numeric_limits<std::uint32_t>::max());
        set_child_count(static_cast<std::uint32_t>(children_.size()));
        for (auto &child : children_)
        {
            assert(child != nullptr);
        }
    }

    const GreenPtr<GreenListNode> &GreenListNode::empty()
    {
        static auto instance = make_ref_counted<const GreenListNode>();
        return instance;
    }
} // namespace prism
