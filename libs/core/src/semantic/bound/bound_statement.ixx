/**
 * @file bound_statement.ixx
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
export module prism.core:semantic.bound.bound_statement;

import :semantic.bound.bound_node;
import :syntax.statements;
import :syntax.clauses;
import :semantic.bound.bound_expression;

namespace prism
{
    class VariableSymbol;

    class BoundStatement : public BoundNode
    {
      protected:
        constexpr BoundStatement(const BoundNodeKind kind, const StatementSyntax &syntax) : BoundNode{kind, syntax}
        {
        }

        constexpr BoundStatement(const BoundNodeKind kind, const ExpressionBodySyntax &syntax) : BoundNode{kind, syntax}
        {
        }
    };

    class BoundBlock : public BoundStatement
    {
      public:
        constexpr BoundBlock(const BlockSyntax &syntax, BoundList<BoundStatement> statements)
            : BoundStatement{BoundNodeKind::block, syntax}, statements_{std::move(statements)}
        {
        }

        [[nodiscard]] constexpr const BoundList<BoundStatement> &statements() const noexcept
        {
            return statements_;
        }

      private:
        BoundList<BoundStatement> statements_{};
    };

    class BoundVariableDeclaration : public BoundStatement
    {
      public:
        constexpr BoundVariableDeclaration(const VariableDeclarationStatementSyntax &syntax,
                                           const VariableSymbol &symbol,
                                           BoundPtr<BoundExpression> initializer)
            : BoundStatement{BoundNodeKind::variable_declaration, syntax}, symbol_{symbol},
              initializer_{std::move(initializer)}
        {
        }

        [[nodiscard]] constexpr const VariableSymbol &symbol() const noexcept
        {
            return symbol_;
        }

        [[nodiscard]] constexpr Optional<const BoundExpression &> initializer() const noexcept
        {
            return initializer_.get();
        }

      private:
        const VariableSymbol &symbol_;
        BoundPtr<BoundExpression> initializer_{};
    };

    class BoundExpressionStatement : public BoundStatement
    {
      public:
        constexpr BoundExpressionStatement(const ExpressionStatementSyntax &syntax,
                                           BoundPtr<BoundExpression> expression)
            : BoundStatement{BoundNodeKind::expression_statement, syntax}, expression_{std::move(expression)}
        {
        }

        constexpr BoundExpressionStatement(const ExpressionBodySyntax &syntax, BoundPtr<BoundExpression> expression)
            : BoundStatement{BoundNodeKind::expression_statement, syntax}, expression_{std::move(expression)}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &expression() const noexcept
        {
            return *expression_;
        }

      private:
        BoundPtr<BoundExpression> expression_{};
    };

    class BoundReturnStatement : public BoundStatement
    {
      public:
        constexpr BoundReturnStatement(const ReturnStatementSyntax &syntax, BoundPtr<BoundExpression> expression)
            : BoundStatement{BoundNodeKind::return_statement, syntax}, expression_{std::move(expression)}
        {
        }

        constexpr BoundReturnStatement(const ExpressionBodySyntax &syntax, BoundPtr<BoundExpression> expression)
            : BoundStatement{BoundNodeKind::expression_statement, syntax}, expression_{std::move(expression)}
        {
        }

        [[nodiscard]] constexpr Optional<const BoundExpression &> expression() const noexcept
        {
            return expression_.get();
        }

      private:
        BoundPtr<BoundExpression> expression_{};
    };
} // namespace prism
