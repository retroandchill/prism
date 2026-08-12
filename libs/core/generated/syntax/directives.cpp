module prism.core:syntax.directives.impl;

import :syntax.directives;
import :syntax.names;

namespace prism
{
    SyntaxToken UsingDirectiveSyntax::using_keyword() const
    {
        return SyntaxToken{static_cast<const GreenUsingDirective &>(green()).using_keyword(), this, position()};
    }

    const NameSyntax &UsingDirectiveSyntax::name() const
    {
        return *get_red(name_, 1);
    }

    SyntaxToken UsingDirectiveSyntax::semicolon() const
    {
        return SyntaxToken{static_cast<const GreenUsingDirective &>(green()).semicolon(), this, get_slot_position(2)};
    }

    Optional<const SyntaxNode &> UsingDirectiveSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 1 ? get_red(name_) : std::nullopt;
    }

    Optional<const SyntaxNode &> UsingDirectiveSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 1 ? Optional<const SyntaxNode &>{name_.try_get_value(nullptr)} : std::nullopt;
    }
} // namespace prism
