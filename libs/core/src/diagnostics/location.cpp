/**
 * @file location.cpp
 * @author Francesco Corso
 * @date 8/3/2026
 * @brief
 */
module prism.core:diagnostics.location.impl;

import :diagnostics.location;
import :syntax.tree;
import :syntax.reference;
import :syntax.token;

namespace prism
{
    SourceLocation::SourceLocation(const SyntaxTree &syntax_tree, const TextSpan span) noexcept
        : syntax_tree_{syntax_tree.shared_from_this()}, span_{span}
    {
    }

    SourceLocation::SourceLocation(const SyntaxReference &reference) noexcept
        : SourceLocation{reference.tree(), reference.span()}
    {
    }

    SourceLocation::SourceLocation(const SyntaxNode &node) noexcept
        : syntax_tree_{node.tree().shared_from_this()}, span_{node.span()}
    {
    }

    SourceLocation::SourceLocation(const SyntaxToken &token) noexcept
        : syntax_tree_{token.tree().value_ptr()}, span_{token.span()}
    {
    }

    FileSourcePositionSpan SourceLocation::get_position_span() const
    {
        return syntax_tree_->get_position_span(span_);
    }
} // namespace prism
