module prism.core:syntax.green.expressions.impl;

import :syntax.green.expressions;
import :syntax.green.clauses;

namespace prism
{
    GreenLiteralExpression::GreenLiteralExpression(GreenPtr<GreenToken> value, DiagnosticInfoList diagnostics)
        : GreenExpression{SyntaxKind::literal_expression, std::move(diagnostics)}, value_{std::move(value)}
    {
        set_child_count(1);
        adjust_flags_and_width(*value_);
    }

    GreenLiteralExpression::~GreenLiteralExpression() = default;

    Optional<const GreenNode &> GreenLiteralExpression::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *value_;
            default:
                return std::nullopt;
        }
    }
    GreenIdentifierExpression::GreenIdentifierExpression(GreenPtr<GreenToken> value, DiagnosticInfoList diagnostics)
        : GreenExpression{SyntaxKind::identifier_expression, std::move(diagnostics)}, value_{std::move(value)}
    {
        set_child_count(1);
        adjust_flags_and_width(*value_);
    }

    GreenIdentifierExpression::~GreenIdentifierExpression() = default;

    Optional<const GreenNode &> GreenIdentifierExpression::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *value_;
            default:
                return std::nullopt;
        }
    }
    GreenParenthesizedExpression::GreenParenthesizedExpression(GreenPtr<GreenToken> open,
                                                               GreenPtr<GreenExpression> expression,
                                                               GreenPtr<GreenToken> close,
                                                               DiagnosticInfoList diagnostics)
        : GreenExpression{SyntaxKind::parenthesized_expression, std::move(diagnostics)}, open_{std::move(open)},
          expression_{std::move(expression)}, close_{std::move(close)}
    {
        set_child_count(3);
        adjust_flags_and_width(*open_);
        adjust_flags_and_width(*expression_);
        adjust_flags_and_width(*close_);
    }

    GreenParenthesizedExpression::~GreenParenthesizedExpression() = default;

    Optional<const GreenNode &> GreenParenthesizedExpression::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *open_;
            case 1:
                return *expression_;
            case 2:
                return *close_;
            default:
                return std::nullopt;
        }
    }
    GreenBinaryExpression::GreenBinaryExpression(GreenPtr<GreenExpression> left,
                                                 GreenPtr<GreenToken> op,
                                                 GreenPtr<GreenExpression> right,
                                                 DiagnosticInfoList diagnostics)
        : GreenExpression{SyntaxKind::binary_expression, std::move(diagnostics)}, left_{std::move(left)},
          op_{std::move(op)}, right_{std::move(right)}
    {
        set_child_count(3);
        adjust_flags_and_width(*left_);
        adjust_flags_and_width(*op_);
        adjust_flags_and_width(*right_);
    }

    GreenBinaryExpression::~GreenBinaryExpression() = default;

    Optional<const GreenNode &> GreenBinaryExpression::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *left_;
            case 1:
                return *op_;
            case 2:
                return *right_;
            default:
                return std::nullopt;
        }
    }
    GreenAssignmentExpression::GreenAssignmentExpression(GreenPtr<GreenExpression> left,
                                                         GreenPtr<GreenToken> op,
                                                         GreenPtr<GreenExpression> right,
                                                         DiagnosticInfoList diagnostics)
        : GreenExpression{SyntaxKind::assignment_expression, std::move(diagnostics)}, left_{std::move(left)},
          op_{std::move(op)}, right_{std::move(right)}
    {
        set_child_count(3);
        adjust_flags_and_width(*left_);
        adjust_flags_and_width(*op_);
        adjust_flags_and_width(*right_);
    }

    GreenAssignmentExpression::~GreenAssignmentExpression() = default;

    Optional<const GreenNode &> GreenAssignmentExpression::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *left_;
            case 1:
                return *op_;
            case 2:
                return *right_;
            default:
                return std::nullopt;
        }
    }
    GreenPrefixExpression::GreenPrefixExpression(GreenPtr<GreenToken> op,
                                                 GreenPtr<GreenExpression> operand,
                                                 DiagnosticInfoList diagnostics)
        : GreenExpression{SyntaxKind::prefix_expression, std::move(diagnostics)}, op_{std::move(op)},
          operand_{std::move(operand)}
    {
        set_child_count(2);
        adjust_flags_and_width(*op_);
        adjust_flags_and_width(*operand_);
    }

    GreenPrefixExpression::~GreenPrefixExpression() = default;

    Optional<const GreenNode &> GreenPrefixExpression::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *op_;
            case 1:
                return *operand_;
            default:
                return std::nullopt;
        }
    }
    GreenPostfixExpression::GreenPostfixExpression(GreenPtr<GreenExpression> operand,
                                                   GreenPtr<GreenToken> op,
                                                   DiagnosticInfoList diagnostics)
        : GreenExpression{SyntaxKind::postfix_expression, std::move(diagnostics)}, operand_{std::move(operand)},
          op_{std::move(op)}
    {
        set_child_count(2);
        adjust_flags_and_width(*operand_);
        adjust_flags_and_width(*op_);
    }

    GreenPostfixExpression::~GreenPostfixExpression() = default;

    Optional<const GreenNode &> GreenPostfixExpression::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *operand_;
            case 1:
                return *op_;
            default:
                return std::nullopt;
        }
    }
    GreenTernaryExpression::GreenTernaryExpression(GreenPtr<GreenExpression> condition,
                                                   GreenPtr<GreenToken> question_mark,
                                                   GreenPtr<GreenExpression> when_true,
                                                   GreenPtr<GreenToken> colon,
                                                   GreenPtr<GreenExpression> when_false,
                                                   DiagnosticInfoList diagnostics)
        : GreenExpression{SyntaxKind::ternary_expression, std::move(diagnostics)}, condition_{std::move(condition)},
          question_mark_{std::move(question_mark)}, when_true_{std::move(when_true)}, colon_{std::move(colon)},
          when_false_{std::move(when_false)}
    {
        set_child_count(5);
        adjust_flags_and_width(*condition_);
        adjust_flags_and_width(*question_mark_);
        adjust_flags_and_width(*when_true_);
        adjust_flags_and_width(*colon_);
        adjust_flags_and_width(*when_false_);
    }

    GreenTernaryExpression::~GreenTernaryExpression() = default;

    Optional<const GreenNode &> GreenTernaryExpression::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *condition_;
            case 1:
                return *question_mark_;
            case 2:
                return *when_true_;
            case 3:
                return *colon_;
            case 4:
                return *when_false_;
            default:
                return std::nullopt;
        }
    }
    GreenInvocationExpression::GreenInvocationExpression(GreenPtr<GreenExpression> callee,
                                                         GreenPtr<GreenArgumentList> arguments,
                                                         DiagnosticInfoList diagnostics)
        : GreenExpression{SyntaxKind::invocation_expression, std::move(diagnostics)}, callee_{std::move(callee)},
          arguments_{std::move(arguments)}
    {
        set_child_count(2);
        adjust_flags_and_width(*callee_);
        adjust_flags_and_width(*arguments_);
    }

    GreenInvocationExpression::~GreenInvocationExpression() = default;

    Optional<const GreenNode &> GreenInvocationExpression::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *callee_;
            case 1:
                return *arguments_;
            default:
                return std::nullopt;
        }
    }
} // namespace prism
