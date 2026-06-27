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

    export using ExpressionSyntax = std::variant<IdentifierSyntax,
                                                 LiteralSyntax,
                                                 BinaryExpressionSyntax,
                                                 UnaryExpressionSyntax,
                                                 TernaryExpressionSyntax,
                                                 InvocationSyntax>;

    struct LiteralSyntax
    {
        // TODO: This should be refined to not erase the actual contents of the literal
        std::variant<bool, std::uint64_t, double, std::string> value;
        SourceRange range;
    };

    struct BinaryExpressionSyntax
    {
        BinaryOperator op{};
        std::unique_ptr<ExpressionSyntax> left;
        std::unique_ptr<ExpressionSyntax> right;
    };

    struct UnaryExpressionSyntax
    {
        std::unique_ptr<ExpressionSyntax> operand;
        UnaryOperator op{};
    };

    struct TernaryExpressionSyntax
    {
        std::unique_ptr<ExpressionSyntax> condition;
        std::unique_ptr<ExpressionSyntax> if_true;
        std::unique_ptr<ExpressionSyntax> if_false;
    };

    struct InvocationSyntax
    {
        std::unique_ptr<ExpressionSyntax> callee;
        std::vector<ExpressionSyntax> arguments;
    };
} // namespace prism
