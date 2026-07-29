module prism.core:syntax.types.impl;

import :syntax.types;

namespace prism
{
    SyntaxToken PredefinedTypeSyntax::keyword() const
    {
        return SyntaxToken{static_cast<const GreenPredefinedType &>(green()).keyword(), this, position()};
    }

    Optional<const SyntaxNode &> PredefinedTypeSyntax::get_node_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    Optional<const SyntaxNode &> PredefinedTypeSyntax::get_cached_slot(const std::size_t) const
    {
        return std::nullopt;
    }

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
