/**
 * @file bound_expression.ixx
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
export module prism.core:semantic.bound.bound_expression;

import :semantic.bound.bound_node;
import :syntax.expressions;
import :semantic.constant_value;
import :symbols.variable_symbol;
import :symbols.parameter_symbol;
import :semantic.operations;
import :symbols.function_symbol;
import :semantic.conversion;
import :symbols.error;

namespace prism
{
    class TypeSymbol;

    class BoundExpression : public BoundNode
    {
      protected:
        constexpr BoundExpression(const BoundNodeKind kind, const ExpressionSyntax &syntax, const TypeSymbol &type)
            : BoundNode{kind, syntax}, type_{type}
        {
        }

        ~BoundExpression() noexcept = default;

      public:
        [[nodiscard]] constexpr const TypeSymbol &type() const noexcept
        {
            return type_;
        }

        [[nodiscard]] constexpr virtual bool is_lvalue() const noexcept
        {
            return false;
        }

        [[nodiscard]] constexpr virtual bool is_assignable() const noexcept
        {
            return false;
        }

        [[nodiscard]] const Optional<ConstantValue> &constant_value() const;

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return is_expression(node.kind());
        }

      protected:
        [[nodiscard]] virtual Optional<ConstantValue> compute_constant_value() const;

      private:
        const TypeSymbol &type_;
        mutable Lazy<Optional<ConstantValue>> constant_value_{};
    };

    class BoundBadExpression final : public BoundExpression
    {
      public:
        constexpr BoundBadExpression(const ExpressionSyntax &syntax, const TypeSymbol &type)
            : BoundExpression{BoundNodeKind::bad_expression, syntax, type}
        {
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::bad_expression;
        }
    };

    class BoundLiteral final : public BoundExpression
    {
      public:
        constexpr BoundLiteral(const ExpressionSyntax &syntax, const ConstantValue &value, const TypeSymbol &type)
            : BoundExpression{BoundNodeKind::literal, syntax, type}, value_{value}
        {
        }

        [[nodiscard]] constexpr const ConstantValue &value() const noexcept
        {
            return value_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::literal;
        }

      protected:
        [[nodiscard]] Optional<ConstantValue> compute_constant_value() const override;

      private:
        ConstantValue value_;
    };

    class BoundVariableAccess final : public BoundExpression
    {
      public:
        constexpr BoundVariableAccess(const ExpressionSyntax &syntax, const VariableSymbol &symbol)
            : BoundExpression{BoundNodeKind::variable_access, syntax, symbol.type()}, symbol_{symbol}
        {
        }

        [[nodiscard]] constexpr const VariableSymbol &symbol() const noexcept
        {
            return symbol_;
        }

        [[nodiscard]] constexpr bool is_lvalue() const noexcept override
        {
            return true;
        }

        [[nodiscard]] constexpr bool is_assignable() const noexcept override
        {
            return symbol_.is_mutable();
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::variable_access;
        }

      private:
        const VariableSymbol &symbol_;
    };

    class BoundParameterAccess final : public BoundExpression
    {
      public:
        constexpr BoundParameterAccess(const ExpressionSyntax &syntax, const ParameterSymbol &symbol)
            : BoundExpression{BoundNodeKind::variable_access, syntax, symbol.type()}, symbol_{symbol}
        {
        }

        [[nodiscard]] constexpr const ParameterSymbol &symbol() const noexcept
        {
            return symbol_;
        }

        [[nodiscard]] constexpr bool is_lvalue() const noexcept override
        {
            return true;
        }

        [[nodiscard]] constexpr bool is_assignable() const noexcept override
        {
            return symbol_.is_mutable();
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::variable_access;
        }

      private:
        const ParameterSymbol &symbol_;
    };

    class BoundUnaryExpression final : public BoundExpression
    {
      public:
        constexpr BoundUnaryExpression(const ExpressionSyntax &syntax,
                                       const BoundExpression &operand,
                                       const UnaryOperation operation,
                                       const TypeSymbol &type)
            : BoundExpression{BoundNodeKind::unary_expression, syntax, type}, operand_{operand}, operation_{operation}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &operand() const noexcept
        {
            return operand_;
        }

        [[nodiscard]] constexpr UnaryOperation operation() const noexcept
        {
            return operation_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::unary_expression;
        }

      private:
        const BoundExpression &operand_;
        UnaryOperation operation_{};
    };

    class BoundBinaryExpression final : public BoundExpression
    {
      public:
        constexpr BoundBinaryExpression(const ExpressionSyntax &syntax,
                                        const BoundExpression &left,
                                        const BoundExpression &right,
                                        const BinaryOperation operation,
                                        const TypeSymbol &type)
            : BoundExpression{BoundNodeKind::binary_expression, syntax, type}, left_{left}, right_{right},
              operation_{operation}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &left() const noexcept
        {
            return left_;
        }

        [[nodiscard]] constexpr const BoundExpression &right() const noexcept
        {
            return right_;
        }

        [[nodiscard]] constexpr BinaryOperation operation() const noexcept
        {
            return operation_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::binary_expression;
        }

      private:
        const BoundExpression &left_;
        const BoundExpression &right_;
        BinaryOperation operation_{};
    };

    class BoundAssignmentExpression final : public BoundExpression
    {
      public:
        constexpr BoundAssignmentExpression(const ExpressionSyntax &syntax,
                                            const BoundExpression &left,
                                            const BoundExpression &right,
                                            const AssignmentOperation operation,
                                            const TypeSymbol &type)
            : BoundExpression{BoundNodeKind::assignment_expression, syntax, type}, left_{left}, right_{right},
              operation_{operation}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &left() const noexcept
        {
            return left_;
        }

        [[nodiscard]] constexpr const BoundExpression &right() const noexcept
        {
            return right_;
        }

        [[nodiscard]] constexpr AssignmentOperation operation() const noexcept
        {
            return operation_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::assignment_expression;
        }

      private:
        const BoundExpression &left_;
        const BoundExpression &right_;
        AssignmentOperation operation_{};
    };

    class BoundConditionalExpression final : public BoundExpression
    {
      public:
        constexpr BoundConditionalExpression(const ExpressionSyntax &syntax,
                                             const BoundExpression &condition,
                                             const BoundExpression &when_true,
                                             const BoundExpression &when_false,
                                             const TypeSymbol &type)
            : BoundExpression{BoundNodeKind::conditional_expression, syntax, type}, condition_{condition},
              when_true_{when_true}, when_false_{when_false}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &condition() const noexcept
        {
            return condition_;
        }

        [[nodiscard]] constexpr const BoundExpression &when_true() const noexcept
        {
            return when_true_;
        }

        [[nodiscard]] constexpr const BoundExpression &when_false() const noexcept
        {
            return when_false_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::conditional_expression;
        }

      private:
        const BoundExpression &condition_;
        const BoundExpression &when_true_;
        const BoundExpression &when_false_;
    };

    class BoundInvocationExpression final : public BoundExpression
    {
      public:
        constexpr BoundInvocationExpression(const ExpressionSyntax &syntax,
                                            const FunctionSymbol &symbol,
                                            const BoundSpan<BoundExpression> arguments)
            : BoundExpression{BoundNodeKind::call_expression, syntax, symbol.return_type()}, symbol_{symbol},
              arguments_{arguments}
        {
        }

        [[nodiscard]] constexpr const FunctionSymbol &symbol() const noexcept
        {
            return symbol_;
        }

        [[nodiscard]] constexpr BoundSpan<BoundExpression> arguments() const noexcept
        {
            return arguments_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::call_expression;
        }

      private:
        const FunctionSymbol &symbol_;
        BoundSpan<BoundExpression> arguments_{};
    };

    class BoundConversionExpression final : public BoundExpression
    {
      public:
        constexpr BoundConversionExpression(const ExpressionSyntax &syntax,
                                            const BoundExpression &operand,
                                            const TypeSymbol &type,
                                            const Conversion conversion)
            : BoundExpression{BoundNodeKind::conversion_expression, syntax, type}, operand_{operand},
              conversion_{conversion}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &operand() const noexcept
        {
            return operand_;
        }

        [[nodiscard]] constexpr Conversion conversion() const noexcept
        {
            return conversion_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::conversion_expression;
        }

      private:
        const BoundExpression &operand_;
        Conversion conversion_{};
    };
} // namespace prism
