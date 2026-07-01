/**
 * @file expression_syntax.ixx
 * @author Francesco Corso
 * @date 6/24/2026
 * @brief
 */
export module prism.core.ast.expression_syntax;

import std;
import prism.core.ast.common_syntax;
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

    export struct LiteralSyntax;
    export struct BinaryExpressionSyntax;
    export struct UnaryExpressionSyntax;
    export struct TernaryExpressionSyntax;
    export struct InvocationSyntax;

    export using ExpressionSyntaxKind = std::variant<IdentifierSyntax,
                                                     LiteralSyntax,
                                                     BinaryExpressionSyntax,
                                                     UnaryExpressionSyntax,
                                                     TernaryExpressionSyntax,
                                                     InvocationSyntax,
                                                     ErrorSyntax>;

    export using ExpressionSyntax = SyntaxNode<ExpressionSyntaxKind>;

    export enum class LiteralSyntaxKind
    {
        bool_true,
        bool_false,
        integer,
        string
    };

    struct LiteralSyntax
    {
        LiteralSyntaxKind kind{};
    };

    struct BinaryExpressionSyntax
    {
        BinaryOperator op{};
        Ref<const ExpressionSyntax> left;
        Ref<const ExpressionSyntax> right;
    };

    struct UnaryExpressionSyntax
    {
        Ref<const ExpressionSyntax> operand;
        UnaryOperator op{};
    };

    struct TernaryExpressionSyntax
    {
        Ref<const ExpressionSyntax> condition;
        Ref<const ExpressionSyntax> if_true;
        Ref<const ExpressionSyntax> if_false;
    };

    struct InvocationSyntax
    {
        Ref<const ExpressionSyntax> callee;
        std::vector<ExpressionSyntax> arguments;
    };
} // namespace prism
