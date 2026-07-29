module prism.core:syntax.declarations.impl;

import :syntax.declarations;
import :syntax.clauses;
import :syntax.statements;

namespace prism
{
    SyntaxTokenList IncompleteDeclarationSyntax::modifiers() const
    {
        return SyntaxTokenList{this, static_cast<const GreenIncompleteDeclaration &>(green()).modifiers(), position()};
    }

    Optional<const SyntaxNode &> IncompleteDeclarationSyntax::get_node_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    Optional<const SyntaxNode &> IncompleteDeclarationSyntax::get_cached_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    SyntaxTokenList VariableDeclarationSyntax::modifiers() const
    {
        return SyntaxTokenList{this, static_cast<const GreenVariableDeclaration &>(green()).modifiers(), position()};
    }

    SyntaxToken VariableDeclarationSyntax::var_keyword() const
    {
        return SyntaxToken{static_cast<const GreenVariableDeclaration &>(green()).var_keyword(),
                           this,
                           get_slot_position(1)};
    }

    Optional<SyntaxToken> VariableDeclarationSyntax::mut_keyword() const
    {
        return static_cast<const GreenVariableDeclaration &>(green()).mut_keyword().transform(
            [this](const GreenToken &g) {
                return SyntaxToken{g, this, get_slot_position(2)};
            });
    }

    SyntaxToken VariableDeclarationSyntax::identifier() const
    {
        return SyntaxToken{static_cast<const GreenVariableDeclaration &>(green()).identifier(),
                           this,
                           get_slot_position(3)};
    }

    Optional<const TypeSpecifierSyntax &> VariableDeclarationSyntax::type() const
    {
        return get_red(type_, 4);
    }

    Optional<const InitializerSyntax &> VariableDeclarationSyntax::initializer() const
    {
        return get_red(initializer_, 5);
    }

    SyntaxToken VariableDeclarationSyntax::semicolon() const
    {
        return SyntaxToken{static_cast<const GreenVariableDeclaration &>(green()).semicolon(),
                           this,
                           get_slot_position(6)};
    }

    Optional<const SyntaxNode &> VariableDeclarationSyntax::get_node_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 4:
                return get_red(type_, 4);
            case 5:
                return get_red(initializer_, 5);
            default:
                return std::nullopt;
        }
    }

    Optional<const SyntaxNode &> VariableDeclarationSyntax::get_cached_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 4:
                return type_.load(std::memory_order_acquire);
            case 5:
                return initializer_.load(std::memory_order_acquire);
            default:
                return std::nullopt;
        }
    }

    SyntaxTokenList FunctionDeclarationSyntax::modifiers() const
    {
        return SyntaxTokenList{this, static_cast<const GreenFunctionDeclaration &>(green()).modifiers(), position()};
    }

    SyntaxToken FunctionDeclarationSyntax::func_keyword() const
    {
        return SyntaxToken{static_cast<const GreenFunctionDeclaration &>(green()).func_keyword(),
                           this,
                           get_slot_position(1)};
    }

    SyntaxToken FunctionDeclarationSyntax::identifier() const
    {
        return SyntaxToken{static_cast<const GreenFunctionDeclaration &>(green()).identifier(),
                           this,
                           get_slot_position(2)};
    }

    const ParameterListSyntax &FunctionDeclarationSyntax::parameters() const
    {
        return *get_red(parameters_, 3);
    }

    Optional<const TypeSpecifierSyntax &> FunctionDeclarationSyntax::return_type() const
    {
        return get_red(return_type_, 4);
    }

    Optional<const BlockSyntax &> FunctionDeclarationSyntax::body() const
    {
        return get_red(body_, 5);
    }

    Optional<const ExpressionBodySyntax &> FunctionDeclarationSyntax::expression_body() const
    {
        return get_red(expression_body_, 6);
    }

    Optional<SyntaxToken> FunctionDeclarationSyntax::semicolon() const
    {
        return static_cast<const GreenFunctionDeclaration &>(green()).semicolon().transform(
            [this](const GreenToken &g) {
                return SyntaxToken{g, this, get_slot_position(7)};
            });
    }

    Optional<const SyntaxNode &> FunctionDeclarationSyntax::get_node_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 3:
                return get_red(parameters_, 3);
            case 4:
                return get_red(return_type_, 4);
            case 5:
                return get_red(body_, 5);
            case 6:
                return get_red(expression_body_, 6);
            default:
                return std::nullopt;
        }
    }

    Optional<const SyntaxNode &> FunctionDeclarationSyntax::get_cached_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 3:
                return parameters_.load(std::memory_order_acquire);
            case 4:
                return return_type_.load(std::memory_order_acquire);
            case 5:
                return body_.load(std::memory_order_acquire);
            case 6:
                return expression_body_.load(std::memory_order_acquire);
            default:
                return std::nullopt;
        }
    }
} // namespace prism
