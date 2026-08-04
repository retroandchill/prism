/**
 * @file location.cpp
 * @author Francesco Corso
 * @date 8/3/2026
 * @brief
 */
module prism.core:diagnostics.location.impl;

import :diagnostics.location;
import :syntax.tree;

namespace prism
{

    FileSourcePositionSpan SourceLocation::get_position_span() const
    {
        return syntax_tree_->get_position_span(span_);
    }
} // namespace prism
