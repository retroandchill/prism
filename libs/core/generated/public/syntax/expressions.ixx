module;

#include "prism/core/exports.h"

export module prism.core:syntax.expressions;

import :syntax.node;
import :syntax.token;
import :syntax.list;
import :syntax.green.expressions;

namespace prism
{
    class ArgumentListSyntax;

    export class PRISM_CORE_API ExpressionSyntax : public SyntaxNode
    {
      protected:
        constexpr ExpressionSyntax(const GreenExpression &node, const SyntaxTree &tree, const std::uint32_t position)
            : SyntaxNode{node, tree, position}
        {
        }

        constexpr ExpressionSyntax(const GreenExpression &node, const SyntaxNode &parent, const std::uint32_t position)
            : SyntaxNode{node, parent, position}
        {
        }

        ~ExpressionSyntax() = default;

      public:
        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::literal_expression || node.kind() == SyntaxKind::identifier_expression ||
                   node.kind() == SyntaxKind::parenthesized_expression ||
                   node.kind() == SyntaxKind::binary_expression || node.kind() == SyntaxKind::assignment_expression ||
                   node.kind() == SyntaxKind::prefix_expression || node.kind() == SyntaxKind::postfix_expression ||
                   node.kind() == SyntaxKind::ternary_expression || node.kind() == SyntaxKind::invocation_expression;
        }
    };

    export class PRISM_CORE_API LiteralExpressionSyntax final : public ExpressionSyntax
    {
      public:
        constexpr LiteralExpressionSyntax(const GreenLiteralExpression &node,
                                          const SyntaxTree &tree,
                                          const std::uint32_t position)
            : ExpressionSyntax{node, tree, position}
        {
        }

        constexpr LiteralExpressionSyntax(const GreenLiteralExpression &node,
                                          const SyntaxNode &parent,
                                          const std::uint32_t position)
            : ExpressionSyntax{node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken value() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::literal_expression;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;
    };

    export class PRISM_CORE_API IdentifierExpressionSyntax final : public ExpressionSyntax
    {
      public:
        constexpr IdentifierExpressionSyntax(const GreenIdentifierExpression &node,
                                             const SyntaxTree &tree,
                                             const std::uint32_t position)
            : ExpressionSyntax{node, tree, position}
        {
        }

        constexpr IdentifierExpressionSyntax(const GreenIdentifierExpression &node,
                                             const SyntaxNode &parent,
                                             const std::uint32_t position)
            : ExpressionSyntax{node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken value() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::identifier_expression;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;
    };

    export class PRISM_CORE_API ParenthesizedExpressionSyntax final : public ExpressionSyntax
    {
      public:
        constexpr ParenthesizedExpressionSyntax(const GreenParenthesizedExpression &node,
                                                const SyntaxTree &tree,
                                                const std::uint32_t position)
            : ExpressionSyntax{node, tree, position}
        {
        }

        constexpr ParenthesizedExpressionSyntax(const GreenParenthesizedExpression &node,
                                                const SyntaxNode &parent,
                                                const std::uint32_t position)
            : ExpressionSyntax{node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken open() const;
        [[nodiscard]] const ExpressionSyntax &expression() const;
        [[nodiscard]] SyntaxToken close() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::parenthesized_expression;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable std::atomic<const ExpressionSyntax *> expression_;
    };

    export class PRISM_CORE_API BinaryExpressionSyntax final : public ExpressionSyntax
    {
      public:
        constexpr BinaryExpressionSyntax(const GreenBinaryExpression &node,
                                         const SyntaxTree &tree,
                                         const std::uint32_t position)
            : ExpressionSyntax{node, tree, position}
        {
        }

        constexpr BinaryExpressionSyntax(const GreenBinaryExpression &node,
                                         const SyntaxNode &parent,
                                         const std::uint32_t position)
            : ExpressionSyntax{node, parent, position}
        {
        }

        [[nodiscard]] const ExpressionSyntax &left() const;
        [[nodiscard]] SyntaxToken op() const;
        [[nodiscard]] const ExpressionSyntax &right() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::binary_expression;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable std::atomic<const ExpressionSyntax *> left_;
        mutable std::atomic<const ExpressionSyntax *> right_;
    };

    export class PRISM_CORE_API AssignmentExpressionSyntax final : public ExpressionSyntax
    {
      public:
        constexpr AssignmentExpressionSyntax(const GreenAssignmentExpression &node,
                                             const SyntaxTree &tree,
                                             const std::uint32_t position)
            : ExpressionSyntax{node, tree, position}
        {
        }

        constexpr AssignmentExpressionSyntax(const GreenAssignmentExpression &node,
                                             const SyntaxNode &parent,
                                             const std::uint32_t position)
            : ExpressionSyntax{node, parent, position}
        {
        }

        [[nodiscard]] const ExpressionSyntax &left() const;
        [[nodiscard]] SyntaxToken op() const;
        [[nodiscard]] const ExpressionSyntax &right() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::assignment_expression;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable std::atomic<const ExpressionSyntax *> left_;
        mutable std::atomic<const ExpressionSyntax *> right_;
    };

    export class PRISM_CORE_API PrefixExpressionSyntax final : public ExpressionSyntax
    {
      public:
        constexpr PrefixExpressionSyntax(const GreenPrefixExpression &node,
                                         const SyntaxTree &tree,
                                         const std::uint32_t position)
            : ExpressionSyntax{node, tree, position}
        {
        }

        constexpr PrefixExpressionSyntax(const GreenPrefixExpression &node,
                                         const SyntaxNode &parent,
                                         const std::uint32_t position)
            : ExpressionSyntax{node, parent, position}
        {
        }

        [[nodiscard]] SyntaxToken op() const;
        [[nodiscard]] const ExpressionSyntax &operand() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::prefix_expression;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable std::atomic<const ExpressionSyntax *> operand_;
    };

    export class PRISM_CORE_API PostfixExpressionSyntax final : public ExpressionSyntax
    {
      public:
        constexpr PostfixExpressionSyntax(const GreenPostfixExpression &node,
                                          const SyntaxTree &tree,
                                          const std::uint32_t position)
            : ExpressionSyntax{node, tree, position}
        {
        }

        constexpr PostfixExpressionSyntax(const GreenPostfixExpression &node,
                                          const SyntaxNode &parent,
                                          const std::uint32_t position)
            : ExpressionSyntax{node, parent, position}
        {
        }

        [[nodiscard]] const ExpressionSyntax &operand() const;
        [[nodiscard]] SyntaxToken op() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::postfix_expression;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable std::atomic<const ExpressionSyntax *> operand_;
    };

    export class PRISM_CORE_API TernaryExpressionSyntax final : public ExpressionSyntax
    {
      public:
        constexpr TernaryExpressionSyntax(const GreenTernaryExpression &node,
                                          const SyntaxTree &tree,
                                          const std::uint32_t position)
            : ExpressionSyntax{node, tree, position}
        {
        }

        constexpr TernaryExpressionSyntax(const GreenTernaryExpression &node,
                                          const SyntaxNode &parent,
                                          const std::uint32_t position)
            : ExpressionSyntax{node, parent, position}
        {
        }

        [[nodiscard]] const ExpressionSyntax &condition() const;
        [[nodiscard]] SyntaxToken question_mark() const;
        [[nodiscard]] const ExpressionSyntax &when_true() const;
        [[nodiscard]] SyntaxToken colon() const;
        [[nodiscard]] const ExpressionSyntax &when_false() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::ternary_expression;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable std::atomic<const ExpressionSyntax *> condition_;
        mutable std::atomic<const ExpressionSyntax *> when_true_;
        mutable std::atomic<const ExpressionSyntax *> when_false_;
    };

    export class PRISM_CORE_API InvocationExpressionSyntax final : public ExpressionSyntax
    {
      public:
        constexpr InvocationExpressionSyntax(const GreenInvocationExpression &node,
                                             const SyntaxTree &tree,
                                             const std::uint32_t position)
            : ExpressionSyntax{node, tree, position}
        {
        }

        constexpr InvocationExpressionSyntax(const GreenInvocationExpression &node,
                                             const SyntaxNode &parent,
                                             const std::uint32_t position)
            : ExpressionSyntax{node, parent, position}
        {
        }

        [[nodiscard]] const ExpressionSyntax &callee() const;
        [[nodiscard]] const ArgumentListSyntax &arguments() const;

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::invocation_expression;
        }

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;
        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        mutable std::atomic<const ExpressionSyntax *> callee_;
        mutable std::atomic<const ArgumentListSyntax *> arguments_;
    };
} // namespace prism
