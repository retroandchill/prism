module prism.core:syntax.declarations.impl;

import :syntax.declarations;
import :syntax.clauses;
import :syntax.directives;
import :syntax.names;
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

    SyntaxTokenList BlockNamespaceDeclarationSyntax::modifiers() const
    {
        return SyntaxTokenList{this,
                               static_cast<const GreenBlockNamespaceDeclaration &>(green()).modifiers(),
                               position()};
    }

    SyntaxToken BlockNamespaceDeclarationSyntax::namespace_token() const
    {
        return SyntaxToken{static_cast<const GreenBlockNamespaceDeclaration &>(green()).namespace_token(),
                           this,
                           get_slot_position(1)};
    }

    const NameSyntax &BlockNamespaceDeclarationSyntax::name() const
    {
        return *get_red(name_, 2);
    }

    SyntaxToken BlockNamespaceDeclarationSyntax::open_brace() const
    {
        return SyntaxToken{static_cast<const GreenBlockNamespaceDeclaration &>(green()).open_brace(),
                           this,
                           get_slot_position(3)};
    }

    SyntaxList<UsingDirectiveSyntax> BlockNamespaceDeclarationSyntax::usings() const
    {
        const auto red = get_red(usings_, 4);
        return make_syntax_list<UsingDirectiveSyntax>(red);
    }

    SyntaxList<DeclarationSyntax> BlockNamespaceDeclarationSyntax::members() const
    {
        const auto red = get_red(members_, 5);
        return make_syntax_list<DeclarationSyntax>(red);
    }

    SyntaxToken BlockNamespaceDeclarationSyntax::close_brace() const
    {
        return SyntaxToken{static_cast<const GreenBlockNamespaceDeclaration &>(green()).close_brace(),
                           this,
                           get_slot_position(6)};
    }

    Optional<const SyntaxNode &> BlockNamespaceDeclarationSyntax::get_node_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 2:
                return get_red(name_, 2);
            case 4:
                return get_red(usings_, 4);
            case 5:
                return get_red(members_, 5);
            default:
                return std::nullopt;
        }
    }

    Optional<const SyntaxNode &> BlockNamespaceDeclarationSyntax::get_cached_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 2:
                return name_.try_get_value(nullptr);
            case 4:
                return usings_.try_get_value(nullptr);
            case 5:
                return members_.try_get_value(nullptr);
            default:
                return std::nullopt;
        }
    }

    SyntaxTokenList FileScopedNamespaceDeclarationSyntax::modifiers() const
    {
        return SyntaxTokenList{this,
                               static_cast<const GreenFileScopedNamespaceDeclaration &>(green()).modifiers(),
                               position()};
    }

    SyntaxToken FileScopedNamespaceDeclarationSyntax::namespace_token() const
    {
        return SyntaxToken{static_cast<const GreenFileScopedNamespaceDeclaration &>(green()).namespace_token(),
                           this,
                           get_slot_position(1)};
    }

    const NameSyntax &FileScopedNamespaceDeclarationSyntax::name() const
    {
        return *get_red(name_, 2);
    }

    SyntaxToken FileScopedNamespaceDeclarationSyntax::semicolon() const
    {
        return SyntaxToken{static_cast<const GreenFileScopedNamespaceDeclaration &>(green()).semicolon(),
                           this,
                           get_slot_position(3)};
    }

    SyntaxList<UsingDirectiveSyntax> FileScopedNamespaceDeclarationSyntax::usings() const
    {
        const auto red = get_red(usings_, 4);
        return make_syntax_list<UsingDirectiveSyntax>(red);
    }

    SyntaxList<DeclarationSyntax> FileScopedNamespaceDeclarationSyntax::members() const
    {
        const auto red = get_red(members_, 5);
        return make_syntax_list<DeclarationSyntax>(red);
    }

    Optional<const SyntaxNode &> FileScopedNamespaceDeclarationSyntax::get_node_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 2:
                return get_red(name_, 2);
            case 4:
                return get_red(usings_, 4);
            case 5:
                return get_red(members_, 5);
            default:
                return std::nullopt;
        }
    }

    Optional<const SyntaxNode &> FileScopedNamespaceDeclarationSyntax::get_cached_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 2:
                return name_.try_get_value(nullptr);
            case 4:
                return usings_.try_get_value(nullptr);
            case 5:
                return members_.try_get_value(nullptr);
            default:
                return std::nullopt;
        }
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
                return type_.try_get_value(nullptr);
            case 5:
                return initializer_.try_get_value(nullptr);
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
                return parameters_.try_get_value(nullptr);
            case 4:
                return return_type_.try_get_value(nullptr);
            case 5:
                return body_.try_get_value(nullptr);
            case 6:
                return expression_body_.try_get_value(nullptr);
            default:
                return std::nullopt;
        }
    }
} // namespace prism
