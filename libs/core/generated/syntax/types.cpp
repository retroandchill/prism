module prism.core:syntax.types.impl;

import :syntax.types;
import :syntax.names;

namespace prism
{
    SyntaxToken PredefinedTypeSyntax::keyword() const
    {
        return SyntaxToken{static_cast<const GreenPredefinedType &>(SyntaxNodeInternal::get_green(*this)).keyword(),
                           this,
                           position()};
    }

    Optional<const SyntaxNode &> PredefinedTypeSyntax::get_node_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    Optional<const SyntaxNode &> PredefinedTypeSyntax::get_cached_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    const NameSyntax &NamedTypeSyntax::identifier() const
    {
        return *get_red(identifier_);
    }

    Optional<const SyntaxNode &> NamedTypeSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 0 ? get_red(identifier_) : std::nullopt;
    }

    Optional<const SyntaxNode &> NamedTypeSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 0 ? Optional<const SyntaxNode &>{identifier_.try_get_value(nullptr)} : std::nullopt;
    }
} // namespace prism
