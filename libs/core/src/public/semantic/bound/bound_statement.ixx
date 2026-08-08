/**
 * @file bound_statement.ixx
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
export module prism.core:semantic.bound.bound_statement;

import :semantic.bound.bound_node;
import :syntax.statements;

namespace prism
{
    class BoundExpression;
    class VariableSymbol;

    class BoundStatement : public BoundNode
    {
      protected:
        constexpr BoundStatement(const BoundNodeKind kind, const StatementSyntax &syntax) : BoundNode{kind, syntax}
        {
        }

        ~BoundStatement() noexcept = default;
    };

    class BoundBlock : public BoundStatement
    {
      public:
        constexpr BoundBlock(const BlockSyntax &syntax, const BoundSpan<BoundStatement> statements)
            : BoundStatement{BoundNodeKind::block, syntax}, statements_{statements}
        {
        }

        [[nodiscard]] constexpr BoundSpan<BoundStatement> statements() const noexcept
        {
            return statements_;
        }

      private:
        BoundSpan<BoundStatement> statements_{};
    };

    class BoundVariableDeclaration : public BoundStatement
    {
      public:
        constexpr BoundVariableDeclaration(const VariableDeclarationStatementSyntax &syntax,
                                           const VariableSymbol &symbol,
                                           const BoundExpression *initializer)
            : BoundStatement{BoundNodeKind::variable_declaration, syntax}, symbol_{symbol}, initializer_{initializer}
        {
        }

        [[nodiscard]] constexpr const VariableSymbol &symbol() const noexcept
        {
            return symbol_;
        }

        [[nodiscard]] constexpr Optional<const BoundExpression &> initializer() const noexcept
        {
            return initializer_;
        }

      private:
        const VariableSymbol &symbol_;
        const BoundExpression *initializer_;
    };

    class BoundExpressionStatement : public BoundStatement
    {
      public:
        constexpr BoundExpressionStatement(const ExpressionStatementSyntax &syntax, const BoundExpression &expression)
            : BoundStatement{BoundNodeKind::expression_statement, syntax}, expression_{expression}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &expression() const noexcept
        {
            return expression_;
        }

      private:
        const BoundExpression &expression_;
    };

    class BoundReturnStatement : public BoundStatement
    {
      public:
        constexpr BoundReturnStatement(const ExpressionStatementSyntax &syntax, const BoundExpression &expression)
            : BoundStatement{BoundNodeKind::return_statement, syntax}, expression_{expression}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &expression() const noexcept
        {
            return expression_;
        }

      private:
        const BoundExpression &expression_;
    };
} // namespace prism
