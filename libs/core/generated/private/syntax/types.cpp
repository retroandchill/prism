module prism.core:syntax.types.impl;

import :syntax.types;

namespace prism
{
    SyntaxToken IdentifierNamedTypeSyntax::identifier() const
    {
        return SyntaxToken{static_cast<const GreenIdentifierNamedType &>(green()).identifier(), this, position()};
    }

    Optional<const SyntaxNode &> IdentifierNamedTypeSyntax::get_node_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    Optional<const SyntaxNode &> IdentifierNamedTypeSyntax::get_cached_slot(const std::size_t) const
    {
        return std::nullopt;
    }
} // namespace prism
