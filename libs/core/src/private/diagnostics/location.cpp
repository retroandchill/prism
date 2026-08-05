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
    SourceLocation::SourceLocation(const SyntaxNode &node) noexcept : syntax_tree_{&node.tree()}, span_{node.span()}
    {
    }

    SourceLocation::SourceLocation(const SyntaxToken &token) noexcept
        : syntax_tree_{token.tree().value_ptr()}, span_{token.span()}
    {
    }

    SourceLocation::SourceLocation(const SyntaxNodeOrToken &token) noexcept
        : syntax_tree_{get_tree(token).value_ptr()}, span_{get_span(token)}
    {
    }

    FileSourcePositionSpan SourceLocation::get_position_span() const
    {
        return syntax_tree_->get_position_span(span_);
    }
} // namespace prism
