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
        constexpr BoundLiteral(const ExpressionSyntax &syntax, ConstantValue value, const TypeSymbol &type)
            : BoundExpression{BoundNodeKind::literal, syntax, type}, value_{std::move(value)}
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

    class BoundOverloadSet final : public BoundExpression
    {
      public:
        constexpr BoundOverloadSet(const ExpressionSyntax &syntax, ImmutableArray<Ref<const FunctionSymbol>> functions)
            : BoundExpression{BoundNodeKind::overload_set, syntax, unnamed_error_type}, functions_{std::move(functions)}
        {
        }

        [[nodiscard]] constexpr std::span<const Ref<const FunctionSymbol>> functions() const noexcept
        {
            return functions_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::overload_set;
        }

      private:
        ImmutableArray<Ref<const FunctionSymbol>> functions_;
    };

    class BoundUnaryExpression final : public BoundExpression
    {
      public:
        constexpr BoundUnaryExpression(const ExpressionSyntax &syntax,
                                       BoundPtr<BoundExpression> operand,
                                       const UnaryOperation operation,
                                       const TypeSymbol &type)
            : BoundExpression{BoundNodeKind::unary_expression, syntax, type}, operand_{std::move(operand)},
              operation_{operation}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &operand() const noexcept
        {
            return *operand_;
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
        BoundPtr<BoundExpression> operand_;
        UnaryOperation operation_{};
    };

    class BoundBinaryExpression final : public BoundExpression
    {
      public:
        constexpr BoundBinaryExpression(const ExpressionSyntax &syntax,
                                        BoundPtr<BoundExpression> left,
                                        BoundPtr<BoundExpression> right,
                                        const BinaryOperation operation,
                                        const TypeSymbol &type)
            : BoundExpression{BoundNodeKind::binary_expression, syntax, type}, left_{std::move(left)},
              right_{std::move(right)}, operation_{operation}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &left() const noexcept
        {
            return *left_;
        }

        [[nodiscard]] constexpr const BoundExpression &right() const noexcept
        {
            return *right_;
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
        BoundPtr<BoundExpression> left_{};
        BoundPtr<BoundExpression> right_{};
        BinaryOperation operation_{};
    };

    class BoundAssignmentExpression final : public BoundExpression
    {
      public:
        constexpr BoundAssignmentExpression(const ExpressionSyntax &syntax,
                                            BoundPtr<BoundExpression> left,
                                            BoundPtr<BoundExpression> right,
                                            const AssignmentOperation operation,
                                            const TypeSymbol &type)
            : BoundExpression{BoundNodeKind::assignment_expression, syntax, type}, left_{std::move(left)},
              right_{std::move(right)}, operation_{operation}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &left() const noexcept
        {
            return *left_;
        }

        [[nodiscard]] constexpr const BoundExpression &right() const noexcept
        {
            return *right_;
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
        BoundPtr<BoundExpression> left_;
        BoundPtr<BoundExpression> right_;
        AssignmentOperation operation_{};
    };

    class BoundConditionalExpression final : public BoundExpression
    {
      public:
        constexpr BoundConditionalExpression(const ExpressionSyntax &syntax,
                                             BoundPtr<BoundExpression> condition,
                                             BoundPtr<BoundExpression> when_true,
                                             BoundPtr<BoundExpression> when_false,
                                             const TypeSymbol &type)
            : BoundExpression{BoundNodeKind::conditional_expression, syntax, type}, condition_{std::move(condition)},
              when_true_{std::move(when_true)}, when_false_{std::move(when_false)}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &condition() const noexcept
        {
            return *condition_;
        }

        [[nodiscard]] constexpr const BoundExpression &when_true() const noexcept
        {
            return *when_true_;
        }

        [[nodiscard]] constexpr const BoundExpression &when_false() const noexcept
        {
            return *when_false_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::conditional_expression;
        }

      private:
        BoundPtr<BoundExpression> condition_;
        BoundPtr<BoundExpression> when_true_;
        BoundPtr<BoundExpression> when_false_;
    };

    class BoundInvocationExpression final : public BoundExpression
    {
      public:
        constexpr BoundInvocationExpression(const ExpressionSyntax &syntax,
                                            const FunctionSymbol &symbol,
                                            BoundList<BoundExpression> arguments)
            : BoundExpression{BoundNodeKind::call_expression, syntax, symbol.return_type()}, symbol_{symbol},
              arguments_{std::move(arguments)}
        {
        }

        [[nodiscard]] constexpr const FunctionSymbol &symbol() const noexcept
        {
            return symbol_;
        }

        [[nodiscard]] constexpr const BoundList<BoundExpression> &arguments() const noexcept
        {
            return arguments_;
        }

        [[nodiscard]] constexpr static bool instance_of(const BoundNode &node)
        {
            return node.kind() == BoundNodeKind::call_expression;
        }

      private:
        const FunctionSymbol &symbol_;
        BoundList<BoundExpression> arguments_{};
    };

    class BoundConversionExpression final : public BoundExpression
    {
      public:
        constexpr BoundConversionExpression(const ExpressionSyntax &syntax,
                                            BoundPtr<BoundExpression> operand,
                                            const TypeSymbol &type,
                                            const Conversion conversion)
            : BoundExpression{BoundNodeKind::conversion_expression, syntax, type}, operand_{std::move(operand)},
              conversion_{conversion}
        {
        }

        [[nodiscard]] constexpr const BoundExpression &operand() const noexcept
        {
            return *operand_;
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
        BoundPtr<BoundExpression> operand_;
        Conversion conversion_{};
    };
} // namespace prism
