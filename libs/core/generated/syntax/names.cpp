module prism.core:syntax.names.impl;

import :syntax.names;

namespace prism
{
    SyntaxToken SimpleNameSyntax::identifier() const
    {
        return SyntaxToken{static_cast<const GreenSimpleName &>(green()).identifier(), this, position()};
    }

    Optional<const SyntaxNode &> SimpleNameSyntax::get_node_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    Optional<const SyntaxNode &> SimpleNameSyntax::get_cached_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    const NameSyntax &QualifiedNameSyntax::left() const
    {
        return *get_red(left_);
    }

    SyntaxToken QualifiedNameSyntax::separator() const
    {
        return SyntaxToken{static_cast<const GreenQualifiedName &>(green()).separator(), this, get_slot_position(1)};
    }

    const SimpleNameSyntax &QualifiedNameSyntax::right() const
    {
        return *get_red(right_, 2);
    }

    Optional<const SyntaxNode &> QualifiedNameSyntax::get_node_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return get_red(left_);
            case 2:
                return get_red(right_, 2);
            default:
                return std::nullopt;
        }
    }

    Optional<const SyntaxNode &> QualifiedNameSyntax::get_cached_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return left_.value_or(nullptr);
            case 2:
                return right_.value_or(nullptr);
            default:
                return std::nullopt;
        }
    }
} // namespace prism
