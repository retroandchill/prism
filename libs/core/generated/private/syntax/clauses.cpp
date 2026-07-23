module prism.core:syntax.clauses.impl;

import :syntax.clauses;
import :syntax.expressions;
import :syntax.types;

namespace prism
{
    SyntaxToken InitializerSyntax::equal_sign() const
    {
        return SyntaxToken{static_cast<const GreenInitializer &>(green()).equal_sign(), this, position()};
    }

    const ExpressionSyntax &InitializerSyntax::value() const
    {
        return *get_red(value_, 1);
    }

    Optional<const SyntaxNode &> InitializerSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 1 ? get_red(value_) : std::nullopt;
    }

    Optional<const SyntaxNode &> InitializerSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 1 ? Optional<const SyntaxNode &>{value_.load(std::memory_order_acquire)} : std::nullopt;
    }

    SyntaxToken TypeSpecifierSyntax::colon() const
    {
        return SyntaxToken{static_cast<const GreenTypeSpecifier &>(green()).colon(), this, position()};
    }

    const TypeSyntax &TypeSpecifierSyntax::type() const
    {
        return *get_red(type_, 1);
    }

    Optional<const SyntaxNode &> TypeSpecifierSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 1 ? get_red(type_) : std::nullopt;
    }

    Optional<const SyntaxNode &> TypeSpecifierSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 1 ? Optional<const SyntaxNode &>{type_.load(std::memory_order_acquire)} : std::nullopt;
    }

    SyntaxToken NamedParameterSyntax::name() const
    {
        return SyntaxToken{static_cast<const GreenNamedParameter &>(green()).name(), this, position()};
    }

    SyntaxToken NamedParameterSyntax::colon() const
    {
        return SyntaxToken{static_cast<const GreenNamedParameter &>(green()).colon(), this, get_child_position(1)};
    }

    Optional<const SyntaxNode &> NamedParameterSyntax::get_node_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    Optional<const SyntaxNode &> NamedParameterSyntax::get_cached_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    SyntaxToken ArgumentListSyntax::open_paren() const
    {
        return SyntaxToken{static_cast<const GreenArgumentList &>(green()).open_paren(), this, position()};
    }

    SeparatedSyntaxList<ArgumentSyntax> ArgumentListSyntax::arguments() const
    {
        const auto red = get_red(arguments_, 1);
        return make_separated_syntax_list<ArgumentSyntax>(red);
    }

    SyntaxToken ArgumentListSyntax::close_paren() const
    {
        return SyntaxToken{static_cast<const GreenArgumentList &>(green()).close_paren(), this, get_child_position(2)};
    }

    Optional<const SyntaxNode &> ArgumentListSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 1 ? get_red(arguments_) : std::nullopt;
    }

    Optional<const SyntaxNode &> ArgumentListSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 1 ? Optional<const SyntaxNode &>{arguments_.load(std::memory_order_acquire)} : std::nullopt;
    }

    Optional<const NamedParameterSyntax &> ArgumentSyntax::name() const
    {
        return get_red(name_);
    }

    const ExpressionSyntax &ArgumentSyntax::value() const
    {
        return *get_red(value_, 1);
    }

    Optional<const SyntaxNode &> ArgumentSyntax::get_node_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return get_red(name_);
            case 1:
                return get_red(value_, 1);
            default:
                return std::nullopt;
        }
    }

    Optional<const SyntaxNode &> ArgumentSyntax::get_cached_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return name_.load(std::memory_order_acquire);
            case 1:
                return value_.load(std::memory_order_acquire);
            default:
                return std::nullopt;
        }
    }

    SyntaxToken ParameterListSyntax::open_paren() const
    {
        return SyntaxToken{static_cast<const GreenParameterList &>(green()).open_paren(), this, position()};
    }

    SeparatedSyntaxList<ParameterSyntax> ParameterListSyntax::parameters() const
    {
        const auto red = get_red(parameters_, 1);
        return make_separated_syntax_list<ParameterSyntax>(red);
    }

    SyntaxToken ParameterListSyntax::close_paren() const
    {
        return SyntaxToken{static_cast<const GreenParameterList &>(green()).close_paren(), this, get_child_position(2)};
    }

    Optional<const SyntaxNode &> ParameterListSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 1 ? get_red(parameters_) : std::nullopt;
    }

    Optional<const SyntaxNode &> ParameterListSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 1 ? Optional<const SyntaxNode &>{parameters_.load(std::memory_order_acquire)} : std::nullopt;
    }

    Optional<SyntaxToken> ParameterSyntax::mut_keyword() const
    {
        return static_cast<const GreenParameter &>(green()).mut_keyword().transform(
            [this](const GreenToken &g) {
                return SyntaxToken{g, this, position()};
            });
    }

    SyntaxToken ParameterSyntax::name() const
    {
        return SyntaxToken{static_cast<const GreenParameter &>(green()).name(), this, get_child_position(1)};
    }

    const TypeSpecifierSyntax &ParameterSyntax::type_specifier() const
    {
        return *get_red(type_specifier_, 2);
    }

    Optional<const InitializerSyntax &> ParameterSyntax::default_value() const
    {
        return get_red(default_value_, 3);
    }

    Optional<const SyntaxNode &> ParameterSyntax::get_node_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 2:
                return get_red(type_specifier_, 2);
            case 3:
                return get_red(default_value_, 3);
            default:
                return std::nullopt;
        }
    }

    Optional<const SyntaxNode &> ParameterSyntax::get_cached_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 2:
                return type_specifier_.load(std::memory_order_acquire);
            case 3:
                return default_value_.load(std::memory_order_acquire);
            default:
                return std::nullopt;
        }
    }

    SyntaxToken ExpressionBodySyntax::arrow() const
    {
        return SyntaxToken{static_cast<const GreenExpressionBody &>(green()).arrow(), this, position()};
    }

    const ExpressionSyntax &ExpressionBodySyntax::expression() const
    {
        return *get_red(expression_, 1);
    }

    Optional<const SyntaxNode &> ExpressionBodySyntax::get_node_slot(const std::size_t index) const
    {
        return index == 1 ? get_red(expression_) : std::nullopt;
    }

    Optional<const SyntaxNode &> ExpressionBodySyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 1 ? Optional<const SyntaxNode &>{expression_.load(std::memory_order_acquire)} : std::nullopt;
    }
} // namespace prism
