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

    class BoundBlock final : public BoundStatement
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

    class BoundVariableDeclaration final : public BoundStatement
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

    class BoundExpressionStatement final : public BoundStatement
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

    class BoundReturnStatement final : public BoundStatement
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

    class BoundIfStatement : public BoundStatement
    {
      public:
        constexpr BoundIfStatement(const IfStatementSyntax &syntax,
                                   const BoundExpression &condition,
                                   const BoundStatement &then_statement,
                                   const BoundStatement *else_statement)
            : BoundStatement{BoundNodeKind::if_statement, syntax}, condition_{condition},
              then_statement_{then_statement}, else_statement_{else_statement}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &condition() const noexcept
        {
            return condition_;
        }

        [[nodiscard]] constexpr const BoundStatement &then_statement() const noexcept
        {
            return then_statement_;
        }

        [[nodiscard]] constexpr Optional<const BoundStatement &> else_statement() const noexcept
        {
            return else_statement_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::if_statement;
        }

      private:
        const BoundExpression &condition_;
        const BoundStatement &then_statement_;
        const BoundStatement *else_statement_;
    };

    class BoundWhileStatement : public BoundStatement
    {
      public:
        constexpr BoundWhileStatement(const WhileStatementSyntax &syntax,
                                      const BoundExpression &condition,
                                      const BoundBlock &loop_body)
            : BoundStatement{BoundNodeKind::while_statement, syntax}, condition_{condition}, loop_body_{loop_body}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &condition() const noexcept
        {
            return condition_;
        }

        [[nodiscard]] constexpr const BoundBlock &loop_body() const noexcept
        {
            return loop_body_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::while_statement;
        }

      private:
        const BoundExpression &condition_;
        const BoundBlock &loop_body_;
    };

    class BoundLoopStatement : public BoundStatement
    {
      public:
        constexpr BoundLoopStatement(const LoopStatementSyntax &syntax, const BoundBlock &loop_body)
            : BoundStatement{BoundNodeKind::loop_statement, syntax}, loop_body_{loop_body}
        {
        }

        [[nodiscard]] constexpr const BoundBlock &loop_body() const noexcept
        {
            return loop_body_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::loop_statement;
        }

      private:
        const BoundBlock &loop_body_;
    };

    class BoundForStatement : public BoundStatement
    {
      public:
        constexpr BoundForStatement(const ForStatementSyntax &syntax,
                                    const BoundVariableDeclaration *variable,
                                    const BoundSpan<BoundExpression> initializers,
                                    const BoundExpression *condition,
                                    const BoundSpan<BoundExpression> incrementors,
                                    const BoundBlock &loop_body)
            : BoundStatement{BoundNodeKind::for_statement, syntax}, variable_{variable}, initializers_{initializers},
              condition_{condition}, incrementors_{incrementors}, loop_body_{loop_body}
        {
        }

        [[nodiscard]] constexpr Optional<const BoundVariableDeclaration &> variable() const noexcept
        {
            return variable_;
        }

        [[nodiscard]] constexpr BoundSpan<BoundExpression> initializers() const noexcept
        {
            return initializers_;
        }

        [[nodiscard]] constexpr Optional<const BoundExpression &> condition() const noexcept
        {
            return condition_;
        }

        [[nodiscard]] constexpr BoundSpan<BoundExpression> incrementors() const noexcept
        {
            return incrementors_;
        }

        [[nodiscard]] constexpr const BoundBlock &loop_body() const noexcept
        {
            return loop_body_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::while_statement;
        }

      private:
        const BoundVariableDeclaration *variable_;
        BoundSpan<BoundExpression> initializers_{};
        const BoundExpression *condition_;
        BoundSpan<BoundExpression> incrementors_{};
        const BoundBlock &loop_body_;
    };

    class BoundBreakStatement final : public BoundStatement
    {
      public:
        explicit constexpr BoundBreakStatement(const BreakStatementSyntax &syntax)
            : BoundStatement{BoundNodeKind::break_statement, syntax}
        {
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::break_statement;
        }
    };

    class BoundContinueStatement final : public BoundStatement
    {
      public:
        explicit constexpr BoundContinueStatement(const ContinueStatementSyntax &syntax)
            : BoundStatement{BoundNodeKind::continue_statement, syntax}
        {
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::continue_statement;
        }
    };
} // namespace prism
