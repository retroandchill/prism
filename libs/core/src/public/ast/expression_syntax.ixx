/**
 * @file expression_syntax.ixx
 * @author Francesco Corso
 * @date 6/24/2026
 * @brief
 */
module;

#include "ast/macros.hpp"

export module prism.core.ast:expression_syntax;

import std;
import :syntax_node;
import :identifier_syntax;
import prism.core.source.source_file;
import prism.core.util;

namespace prism
{
    export enum class BinaryOperator
    {
        add,
        sub,
        mul,
        div,
        mod,

        logical_and,
        logical_or,

        bit_and,
        bit_or,
        bit_xor,

        equal,
        not_equal,
        less,
        less_equal,
        greater,
        greater_equal,
        three_way,

        shift_left,
        shift_right,
        unsigned_shift_right,

        assign,
        add_assign,
        sub_assign,
        mul_assign,
        div_assign,
        mod_assign,
        bit_and_assign,
        bit_or_assign,
        bit_xor_assign,
        shift_left_assign,
        shift_right_assign,
        unsigned_shift_right_assign,
    };

    export enum class UnaryOperator
    {
        positive,
        negate,
        logical_not,
        bit_not,
        address_of,
        dereference,
        pre_increment,
        pre_decrement,
        post_increment,
        post_decrement,
    };

    export enum class LiteralSyntaxKind
    {
        bool_true,
        bool_false,
        integer,
        string
    };

    export class ExpressionSyntax : public SyntaxNode
    {
      protected:
        constexpr ExpressionSyntax(SyntaxKind kind,
                                   const SourceRange range,
                                   const SyntaxFlags flags = SyntaxFlags::none)
            : SyntaxNode{SyntaxCategory::expression, kind, range, flags}
        {
        }

        AST_NODE_BOILERPLATE(ExpressionSyntax)
    };

    export class LiteralExpressionSyntax final : public ExpressionSyntax
    {
      public:
        constexpr LiteralExpressionSyntax(const LiteralSyntaxKind kind,
                                          const SourceRange range,
                                          const SyntaxFlags flags = SyntaxFlags::none)
            : ExpressionSyntax{SyntaxKind::literal_expression, range, flags}, kind_{kind}
        {
        }

        VALUE_PROPERTY(LiteralSyntaxKind, kind)
    };

    export class IdentifierExpressionSyntax final : public ExpressionSyntax
    {
      public:
        constexpr IdentifierExpressionSyntax(const Name name,
                                             const SourceRange range,
                                             const SyntaxFlags flags = SyntaxFlags::none)
            : ExpressionSyntax{SyntaxKind::identifier_expression, range, flags}, name_{name}
        {
        }

        VALUE_PROPERTY(Name, name)
    };

    export class BinaryExpressionSyntax : public ExpressionSyntax
    {
      public:
        constexpr BinaryExpressionSyntax(const BinaryOperator op,
                                         const ExpressionSyntax &left,
                                         const ExpressionSyntax &right,
                                         const SourceRange range,
                                         const SyntaxFlags flags = SyntaxFlags::none)
            : ExpressionSyntax{SyntaxKind::binary_expression, range, flags}, op_{op}, left_{left}, right_{right}
        {
        }

        VALUE_PROPERTY(BinaryOperator, op)
        UNOWNED_REF_PROPERTY(ExpressionSyntax, left)
        UNOWNED_REF_PROPERTY(ExpressionSyntax, right)
    };

    export class UnaryExpressionSyntax : public ExpressionSyntax
    {
      public:
        constexpr UnaryExpressionSyntax(const UnaryOperator op,
                                        const ExpressionSyntax &operand,
                                        const SourceRange range,
                                        const SyntaxFlags flags = SyntaxFlags::none)
            : ExpressionSyntax{SyntaxKind::unary_expression, range, flags}, op_{op}, operand_{operand}
        {
        }

        VALUE_PROPERTY(UnaryOperator, op)
        UNOWNED_REF_PROPERTY(ExpressionSyntax, operand)
    };

    export class TernaryExpressionSyntax : public ExpressionSyntax
    {
      public:
        constexpr TernaryExpressionSyntax(const ExpressionSyntax &condition,
                                          const ExpressionSyntax &if_true,
                                          const ExpressionSyntax &if_false,
                                          const SourceRange range,
                                          const SyntaxFlags flags = SyntaxFlags::none)
            : ExpressionSyntax{SyntaxKind::ternary_expression, range, flags}, condition_{condition}, if_true_{if_true},
              if_false_{if_false}
        {
        }

        UNOWNED_REF_PROPERTY(ExpressionSyntax, condition)
        UNOWNED_REF_PROPERTY(ExpressionSyntax, if_true)
        UNOWNED_REF_PROPERTY(ExpressionSyntax, if_false)
    };

    export class InvocationExpressionSyntax : public ExpressionSyntax
    {
      public:
        InvocationExpressionSyntax(const ExpressionSyntax &callee,
                                   const std::span<const Ref<const ExpressionSyntax>> arguments,
                                   const SourceRange range,
                                   const SyntaxFlags flags = SyntaxFlags::none)
            : ExpressionSyntax{SyntaxKind::invocation_expression, range, flags}, callee_{callee}, arguments_{arguments}
        {
        }

        UNOWNED_REF_PROPERTY(ExpressionSyntax, callee)
        VALUE_PROPERTY(std::span<const Ref<const ExpressionSyntax>>, arguments)
    };
} // namespace prism
