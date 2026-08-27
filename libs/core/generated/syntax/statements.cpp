module prism.core:syntax.statements.impl;

import :syntax.statements;
import :syntax.clauses;
import :syntax.declarations;
import :syntax.expressions;

namespace prism
{
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

    SyntaxToken IfStatementSyntax::if_keyword() const
    {
        return SyntaxToken{static_cast<const GreenIfStatement &>(SyntaxNodeInternal::get_green(*this)).if_keyword(),
                           this,
                           position()};
    }

    SyntaxToken IfStatementSyntax::open_paren() const
    {
        return SyntaxToken{static_cast<const GreenIfStatement &>(SyntaxNodeInternal::get_green(*this)).open_paren(),
                           this,
                           get_slot_position(1)};
    }

    const ExpressionSyntax &IfStatementSyntax::condition() const
    {
        return *get_red(condition_, 2);
    }

    SyntaxToken IfStatementSyntax::close_paren() const
    {
        return SyntaxToken{static_cast<const GreenIfStatement &>(SyntaxNodeInternal::get_green(*this)).close_paren(),
                           this,
                           get_slot_position(3)};
    }

    const BlockSyntax &IfStatementSyntax::block() const
    {
        return *get_red(block_, 4);
    }

    Optional<const ElseClauseSyntax &> IfStatementSyntax::else_clause() const
    {
        return get_red(else_clause_, 5);
    }

    Optional<const SyntaxNode &> IfStatementSyntax::get_node_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 2:
                return get_red(condition_, 2);
            case 4:
                return get_red(block_, 4);
            case 5:
                return get_red(else_clause_, 5);
            default:
                return std::nullopt;
        }
    }

    Optional<const SyntaxNode &> IfStatementSyntax::get_cached_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 2:
                return condition_.try_get_value(nullptr);
            case 4:
                return block_.try_get_value(nullptr);
            case 5:
                return else_clause_.try_get_value(nullptr);
            default:
                return std::nullopt;
        }
    }

    SyntaxToken WhileStatementSyntax::while_keyword() const
    {
        return SyntaxToken{
            static_cast<const GreenWhileStatement &>(SyntaxNodeInternal::get_green(*this)).while_keyword(),
            this,
            position()};
    }

    SyntaxToken WhileStatementSyntax::open_paren() const
    {
        return SyntaxToken{static_cast<const GreenWhileStatement &>(SyntaxNodeInternal::get_green(*this)).open_paren(),
                           this,
                           get_slot_position(1)};
    }

    const ExpressionSyntax &WhileStatementSyntax::condition() const
    {
        return *get_red(condition_, 2);
    }

    SyntaxToken WhileStatementSyntax::close_paren() const
    {
        return SyntaxToken{static_cast<const GreenWhileStatement &>(SyntaxNodeInternal::get_green(*this)).close_paren(),
                           this,
                           get_slot_position(3)};
    }

    const BlockSyntax &WhileStatementSyntax::block() const
    {
        return *get_red(block_, 4);
    }

    Optional<const SyntaxNode &> WhileStatementSyntax::get_node_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 2:
                return get_red(condition_, 2);
            case 4:
                return get_red(block_, 4);
            default:
                return std::nullopt;
        }
    }

    Optional<const SyntaxNode &> WhileStatementSyntax::get_cached_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 2:
                return condition_.try_get_value(nullptr);
            case 4:
                return block_.try_get_value(nullptr);
            default:
                return std::nullopt;
        }
    }

    SyntaxToken LoopStatementSyntax::loop_keyword() const
    {
        return SyntaxToken{static_cast<const GreenLoopStatement &>(SyntaxNodeInternal::get_green(*this)).loop_keyword(),
                           this,
                           position()};
    }

    const BlockSyntax &LoopStatementSyntax::block() const
    {
        return *get_red(block_, 1);
    }

    Optional<const SyntaxNode &> LoopStatementSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 1 ? get_red(block_) : std::nullopt;
    }

    Optional<const SyntaxNode &> LoopStatementSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 1 ? Optional<const SyntaxNode &>{block_.try_get_value(nullptr)} : std::nullopt;
    }

    SyntaxToken ForStatementSyntax::for_keyword() const
    {
        return SyntaxToken{static_cast<const GreenForStatement &>(SyntaxNodeInternal::get_green(*this)).for_keyword(),
                           this,
                           position()};
    }

    SyntaxToken ForStatementSyntax::open_paren() const
    {
        return SyntaxToken{static_cast<const GreenForStatement &>(SyntaxNodeInternal::get_green(*this)).open_paren(),
                           this,
                           get_slot_position(1)};
    }

    Optional<const VariableDeclarationStatementSyntax &> ForStatementSyntax::declaration() const
    {
        return get_red(declaration_, 2);
    }

    SeparatedSyntaxList<ExpressionSyntax> ForStatementSyntax::initializers() const
    {
        const auto red = get_red(initializers_, 3);
        return SyntaxNodeInternal::make_separated_syntax_list<ExpressionSyntax>(red);
    }

    SyntaxToken ForStatementSyntax::first_semicolon() const
    {
        return SyntaxToken{
            static_cast<const GreenForStatement &>(SyntaxNodeInternal::get_green(*this)).first_semicolon(),
            this,
            get_slot_position(4)};
    }

    Optional<const ExpressionSyntax &> ForStatementSyntax::condition() const
    {
        return get_red(condition_, 5);
    }

    SyntaxToken ForStatementSyntax::second_semicolon() const
    {
        return SyntaxToken{
            static_cast<const GreenForStatement &>(SyntaxNodeInternal::get_green(*this)).second_semicolon(),
            this,
            get_slot_position(6)};
    }

    SeparatedSyntaxList<ExpressionSyntax> ForStatementSyntax::incrementors() const
    {
        const auto red = get_red(incrementors_, 7);
        return SyntaxNodeInternal::make_separated_syntax_list<ExpressionSyntax>(red);
    }

    SyntaxToken ForStatementSyntax::close_paren() const
    {
        return SyntaxToken{static_cast<const GreenForStatement &>(SyntaxNodeInternal::get_green(*this)).close_paren(),
                           this,
                           get_slot_position(8)};
    }

    const BlockSyntax &ForStatementSyntax::block() const
    {
        return *get_red(block_, 9);
    }

    Optional<const SyntaxNode &> ForStatementSyntax::get_node_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 2:
                return get_red(declaration_, 2);
            case 3:
                return get_red(initializers_, 3);
            case 5:
                return get_red(condition_, 5);
            case 7:
                return get_red(incrementors_, 7);
            case 9:
                return get_red(block_, 9);
            default:
                return std::nullopt;
        }
    }

    Optional<const SyntaxNode &> ForStatementSyntax::get_cached_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 2:
                return declaration_.try_get_value(nullptr);
            case 3:
                return initializers_.try_get_value(nullptr);
            case 5:
                return condition_.try_get_value(nullptr);
            case 7:
                return incrementors_.try_get_value(nullptr);
            case 9:
                return block_.try_get_value(nullptr);
            default:
                return std::nullopt;
        }
    }

    SyntaxToken BreakStatementSyntax::keyword() const
    {
        return SyntaxToken{static_cast<const GreenBreakStatement &>(SyntaxNodeInternal::get_green(*this)).keyword(),
                           this,
                           position()};
    }

    SyntaxToken BreakStatementSyntax::semicolon() const
    {
        return SyntaxToken{static_cast<const GreenBreakStatement &>(SyntaxNodeInternal::get_green(*this)).semicolon(),
                           this,
                           get_slot_position(1)};
    }

    Optional<const SyntaxNode &> BreakStatementSyntax::get_node_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    Optional<const SyntaxNode &> BreakStatementSyntax::get_cached_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    SyntaxToken ContinueStatementSyntax::keyword() const
    {
        return SyntaxToken{static_cast<const GreenContinueStatement &>(SyntaxNodeInternal::get_green(*this)).keyword(),
                           this,
                           position()};
    }

    SyntaxToken ContinueStatementSyntax::semicolon() const
    {
        return SyntaxToken{
            static_cast<const GreenContinueStatement &>(SyntaxNodeInternal::get_green(*this)).semicolon(),
            this,
            get_slot_position(1)};
    }

    Optional<const SyntaxNode &> ContinueStatementSyntax::get_node_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    Optional<const SyntaxNode &> ContinueStatementSyntax::get_cached_slot(const std::size_t) const
    {
        return std::nullopt;
    }
} // namespace prism
