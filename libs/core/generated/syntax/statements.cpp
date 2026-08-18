module prism.core:syntax.statements.impl;

import :syntax.statements;
import :syntax.declarations;
import :syntax.expressions;

namespace prism
{
    const VariableDeclarationSyntax &VariableDeclarationStatementSyntax::declaration() const
    {
        return *get_red(declaration_);
    }

    Optional<const SyntaxNode &> VariableDeclarationStatementSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 0 ? get_red(declaration_) : std::nullopt;
    }

    Optional<const SyntaxNode &> VariableDeclarationStatementSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 0 ? Optional<const SyntaxNode &>{declaration_.try_get_value(nullptr)} : std::nullopt;
    }

    SyntaxToken BlockSyntax::open_brace() const
    {
        return SyntaxToken{static_cast<const GreenBlock &>(SyntaxNodeInternal::get_green(*this)).open_brace(),
                           this,
                           position()};
    }

    SyntaxList<StatementSyntax> BlockSyntax::statements() const
    {
        const auto red = get_red(statements_, 1);
        return SyntaxNodeInternal::make_syntax_list<StatementSyntax>(red);
    }

    SyntaxToken BlockSyntax::close_brace() const
    {
        return SyntaxToken{static_cast<const GreenBlock &>(SyntaxNodeInternal::get_green(*this)).close_brace(),
                           this,
                           get_slot_position(2)};
    }

    Optional<const SyntaxNode &> BlockSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 1 ? get_red(statements_) : std::nullopt;
    }

    Optional<const SyntaxNode &> BlockSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 1 ? Optional<const SyntaxNode &>{statements_.try_get_value(nullptr)} : std::nullopt;
    }

    SyntaxToken ReturnStatementSyntax::return_keyword() const
    {
        return SyntaxToken{
            static_cast<const GreenReturnStatement &>(SyntaxNodeInternal::get_green(*this)).return_keyword(),
            this,
            position()};
    }

    Optional<const ExpressionSyntax &> ReturnStatementSyntax::expression() const
    {
        return get_red(expression_, 1);
    }

    SyntaxToken ReturnStatementSyntax::semicolon() const
    {
        return SyntaxToken{static_cast<const GreenReturnStatement &>(SyntaxNodeInternal::get_green(*this)).semicolon(),
                           this,
                           get_slot_position(2)};
    }

    Optional<const SyntaxNode &> ReturnStatementSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 1 ? get_red(expression_) : std::nullopt;
    }

    Optional<const SyntaxNode &> ReturnStatementSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 1 ? Optional<const SyntaxNode &>{expression_.try_get_value(nullptr)} : std::nullopt;
    }

    const ExpressionSyntax &ExpressionStatementSyntax::expression() const
    {
        return *get_red(expression_);
    }

    SyntaxToken ExpressionStatementSyntax::semicolon() const
    {
        return SyntaxToken{
            static_cast<const GreenExpressionStatement &>(SyntaxNodeInternal::get_green(*this)).semicolon(),
            this,
            get_slot_position(1)};
    }

    Optional<const SyntaxNode &> ExpressionStatementSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 0 ? get_red(expression_) : std::nullopt;
    }

    Optional<const SyntaxNode &> ExpressionStatementSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 0 ? Optional<const SyntaxNode &>{expression_.try_get_value(nullptr)} : std::nullopt;
    }

    SyntaxToken EmptyStatementSyntax::semicolon() const
    {
        return SyntaxToken{static_cast<const GreenEmptyStatement &>(SyntaxNodeInternal::get_green(*this)).semicolon(),
                           this,
                           position()};
    }

    Optional<const SyntaxNode &> EmptyStatementSyntax::get_node_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    Optional<const SyntaxNode &> EmptyStatementSyntax::get_cached_slot(const std::size_t) const
    {
        return std::nullopt;
    }
} // namespace prism
