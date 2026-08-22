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

        constexpr BoundStatement(const BoundNodeKind kind, const ExpressionBodySyntax &syntax) : BoundNode{kind, syntax}
        {
        }

      public:
        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return is_statement(node.kind());
        }
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

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::block;
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

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::variable_declaration;
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

        constexpr BoundExpressionStatement(const ExpressionBodySyntax &syntax, const BoundExpression &expression)
            : BoundStatement{BoundNodeKind::expression_statement, syntax}, expression_{expression}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &expression() const noexcept
        {
            return expression_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::expression_statement;
        }

      private:
        const BoundExpression &expression_;
    };

    class BoundReturnStatement : public BoundStatement
    {
      public:
        constexpr BoundReturnStatement(const ReturnStatementSyntax &syntax, const BoundExpression *expression)
            : BoundStatement{BoundNodeKind::return_statement, syntax}, expression_{expression}
        {
        }

        constexpr BoundReturnStatement(const ExpressionBodySyntax &syntax, const BoundExpression *expression)
            : BoundStatement{BoundNodeKind::expression_statement, syntax}, expression_{expression}
        {
        }

        [[nodiscard]] constexpr Optional<const BoundExpression &> expression() const noexcept
        {
            return expression_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::return_statement;
        }

      private:
        const BoundExpression *expression_;
    };
} // namespace prism
