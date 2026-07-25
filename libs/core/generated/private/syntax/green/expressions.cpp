module prism.core:syntax.green.expressions.impl;

import :syntax.lifetime;
import :syntax.green.expressions;
import :syntax.expressions;
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

    void GreenLiteralExpression::set_value(GreenPtr<GreenToken> value) noexcept
    {
        value_ = std::move(value);
    }

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

    [[nodiscard]] const SyntaxNode &GreenLiteralExpression::create_red(SyntaxLifetime &lifetime,
                                                                       const SyntaxNode *parent,
                                                                       std::uint32_t position) const
    {
        return lifetime.add<LiteralExpressionSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenLiteralExpression> GreenLiteralExpression::with_value(GreenPtr<GreenToken> value) const
    {
        return update(std::move(value));
    }

    GreenPtr<GreenLiteralExpression> GreenLiteralExpression::update(GreenPtr<GreenToken> value) const
    {
        if (value == value_)
            return shared_from_this();

        return make_ref_counted<const GreenLiteralExpression>(std::move(value));
    }

    RefCountPtr<GreenNode> GreenLiteralExpression::clone_internal() const
    {
        return make_ref_counted<GreenLiteralExpression>(value_);
    }

    GreenIdentifierExpression::GreenIdentifierExpression(GreenPtr<GreenToken> value, DiagnosticInfoList diagnostics)
        : GreenExpression{SyntaxKind::identifier_expression, std::move(diagnostics)}, value_{std::move(value)}
    {
        set_child_count(1);
        adjust_flags_and_width(*value_);
    }

    GreenIdentifierExpression::~GreenIdentifierExpression() = default;

    void GreenIdentifierExpression::set_value(GreenPtr<GreenToken> value) noexcept
    {
        value_ = std::move(value);
    }

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

    [[nodiscard]] const SyntaxNode &GreenIdentifierExpression::create_red(SyntaxLifetime &lifetime,
                                                                          const SyntaxNode *parent,
                                                                          std::uint32_t position) const
    {
        return lifetime.add<IdentifierExpressionSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenIdentifierExpression> GreenIdentifierExpression::with_value(
        GreenPtr<GreenToken> value) const
    {
        return update(std::move(value));
    }

    GreenPtr<GreenIdentifierExpression> GreenIdentifierExpression::update(GreenPtr<GreenToken> value) const
    {
        if (value == value_)
            return shared_from_this();

        return make_ref_counted<const GreenIdentifierExpression>(std::move(value));
    }

    RefCountPtr<GreenNode> GreenIdentifierExpression::clone_internal() const
    {
        return make_ref_counted<GreenIdentifierExpression>(value_);
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

    void GreenParenthesizedExpression::set_open(GreenPtr<GreenToken> value) noexcept
    {
        open_ = std::move(value);
    }

    void GreenParenthesizedExpression::set_expression(GreenPtr<GreenExpression> value) noexcept
    {
        expression_ = std::move(value);
    }

    void GreenParenthesizedExpression::set_close(GreenPtr<GreenToken> value) noexcept
    {
        close_ = std::move(value);
    }

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

    [[nodiscard]] const SyntaxNode &GreenParenthesizedExpression::create_red(SyntaxLifetime &lifetime,
                                                                             const SyntaxNode *parent,
                                                                             std::uint32_t position) const
    {
        return lifetime.add<ParenthesizedExpressionSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenParenthesizedExpression> GreenParenthesizedExpression::with_open(
        GreenPtr<GreenToken> open) const
    {
        return update(std::move(open), expression_, close_);
    }

    [[nodiscard]] GreenPtr<GreenParenthesizedExpression> GreenParenthesizedExpression::with_expression(
        GreenPtr<GreenExpression> expression) const
    {
        return update(open_, std::move(expression), close_);
    }

    [[nodiscard]] GreenPtr<GreenParenthesizedExpression> GreenParenthesizedExpression::with_close(
        GreenPtr<GreenToken> close) const
    {
        return update(open_, expression_, std::move(close));
    }

    GreenPtr<GreenParenthesizedExpression> GreenParenthesizedExpression::update(GreenPtr<GreenToken> open,
                                                                                GreenPtr<GreenExpression> expression,
                                                                                GreenPtr<GreenToken> close) const
    {
        if (open == open_ && expression == expression_ && close == close_)
            return shared_from_this();

        return make_ref_counted<const GreenParenthesizedExpression>(std::move(open),
                                                                    std::move(expression),
                                                                    std::move(close));
    }

    RefCountPtr<GreenNode> GreenParenthesizedExpression::clone_internal() const
    {
        return make_ref_counted<GreenParenthesizedExpression>(open_, expression_, close_);
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

    void GreenBinaryExpression::set_left(GreenPtr<GreenExpression> value) noexcept
    {
        left_ = std::move(value);
    }

    void GreenBinaryExpression::set_op(GreenPtr<GreenToken> value) noexcept
    {
        op_ = std::move(value);
    }

    void GreenBinaryExpression::set_right(GreenPtr<GreenExpression> value) noexcept
    {
        right_ = std::move(value);
    }

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

    [[nodiscard]] const SyntaxNode &GreenBinaryExpression::create_red(SyntaxLifetime &lifetime,
                                                                      const SyntaxNode *parent,
                                                                      std::uint32_t position) const
    {
        return lifetime.add<BinaryExpressionSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenBinaryExpression> GreenBinaryExpression::with_left(GreenPtr<GreenExpression> left) const
    {
        return update(std::move(left), op_, right_);
    }

    [[nodiscard]] GreenPtr<GreenBinaryExpression> GreenBinaryExpression::with_op(GreenPtr<GreenToken> op) const
    {
        return update(left_, std::move(op), right_);
    }

    [[nodiscard]] GreenPtr<GreenBinaryExpression> GreenBinaryExpression::with_right(
        GreenPtr<GreenExpression> right) const
    {
        return update(left_, op_, std::move(right));
    }

    GreenPtr<GreenBinaryExpression> GreenBinaryExpression::update(GreenPtr<GreenExpression> left,
                                                                  GreenPtr<GreenToken> op,
                                                                  GreenPtr<GreenExpression> right) const
    {
        if (left == left_ && op == op_ && right == right_)
            return shared_from_this();

        return make_ref_counted<const GreenBinaryExpression>(std::move(left), std::move(op), std::move(right));
    }

    RefCountPtr<GreenNode> GreenBinaryExpression::clone_internal() const
    {
        return make_ref_counted<GreenBinaryExpression>(left_, op_, right_);
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

    void GreenAssignmentExpression::set_left(GreenPtr<GreenExpression> value) noexcept
    {
        left_ = std::move(value);
    }

    void GreenAssignmentExpression::set_op(GreenPtr<GreenToken> value) noexcept
    {
        op_ = std::move(value);
    }

    void GreenAssignmentExpression::set_right(GreenPtr<GreenExpression> value) noexcept
    {
        right_ = std::move(value);
    }

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

    [[nodiscard]] const SyntaxNode &GreenAssignmentExpression::create_red(SyntaxLifetime &lifetime,
                                                                          const SyntaxNode *parent,
                                                                          std::uint32_t position) const
    {
        return lifetime.add<AssignmentExpressionSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenAssignmentExpression> GreenAssignmentExpression::with_left(
        GreenPtr<GreenExpression> left) const
    {
        return update(std::move(left), op_, right_);
    }

    [[nodiscard]] GreenPtr<GreenAssignmentExpression> GreenAssignmentExpression::with_op(GreenPtr<GreenToken> op) const
    {
        return update(left_, std::move(op), right_);
    }

    [[nodiscard]] GreenPtr<GreenAssignmentExpression> GreenAssignmentExpression::with_right(
        GreenPtr<GreenExpression> right) const
    {
        return update(left_, op_, std::move(right));
    }

    GreenPtr<GreenAssignmentExpression> GreenAssignmentExpression::update(GreenPtr<GreenExpression> left,
                                                                          GreenPtr<GreenToken> op,
                                                                          GreenPtr<GreenExpression> right) const
    {
        if (left == left_ && op == op_ && right == right_)
            return shared_from_this();

        return make_ref_counted<const GreenAssignmentExpression>(std::move(left), std::move(op), std::move(right));
    }

    RefCountPtr<GreenNode> GreenAssignmentExpression::clone_internal() const
    {
        return make_ref_counted<GreenAssignmentExpression>(left_, op_, right_);
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

    void GreenPrefixExpression::set_op(GreenPtr<GreenToken> value) noexcept
    {
        op_ = std::move(value);
    }

    void GreenPrefixExpression::set_operand(GreenPtr<GreenExpression> value) noexcept
    {
        operand_ = std::move(value);
    }

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

    [[nodiscard]] const SyntaxNode &GreenPrefixExpression::create_red(SyntaxLifetime &lifetime,
                                                                      const SyntaxNode *parent,
                                                                      std::uint32_t position) const
    {
        return lifetime.add<PrefixExpressionSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenPrefixExpression> GreenPrefixExpression::with_op(GreenPtr<GreenToken> op) const
    {
        return update(std::move(op), operand_);
    }

    [[nodiscard]] GreenPtr<GreenPrefixExpression> GreenPrefixExpression::with_operand(
        GreenPtr<GreenExpression> operand) const
    {
        return update(op_, std::move(operand));
    }

    GreenPtr<GreenPrefixExpression> GreenPrefixExpression::update(GreenPtr<GreenToken> op,
                                                                  GreenPtr<GreenExpression> operand) const
    {
        if (op == op_ && operand == operand_)
            return shared_from_this();

        return make_ref_counted<const GreenPrefixExpression>(std::move(op), std::move(operand));
    }

    RefCountPtr<GreenNode> GreenPrefixExpression::clone_internal() const
    {
        return make_ref_counted<GreenPrefixExpression>(op_, operand_);
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

    void GreenPostfixExpression::set_operand(GreenPtr<GreenExpression> value) noexcept
    {
        operand_ = std::move(value);
    }

    void GreenPostfixExpression::set_op(GreenPtr<GreenToken> value) noexcept
    {
        op_ = std::move(value);
    }

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

    [[nodiscard]] const SyntaxNode &GreenPostfixExpression::create_red(SyntaxLifetime &lifetime,
                                                                       const SyntaxNode *parent,
                                                                       std::uint32_t position) const
    {
        return lifetime.add<PostfixExpressionSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenPostfixExpression> GreenPostfixExpression::with_operand(
        GreenPtr<GreenExpression> operand) const
    {
        return update(std::move(operand), op_);
    }

    [[nodiscard]] GreenPtr<GreenPostfixExpression> GreenPostfixExpression::with_op(GreenPtr<GreenToken> op) const
    {
        return update(operand_, std::move(op));
    }

    GreenPtr<GreenPostfixExpression> GreenPostfixExpression::update(GreenPtr<GreenExpression> operand,
                                                                    GreenPtr<GreenToken> op) const
    {
        if (operand == operand_ && op == op_)
            return shared_from_this();

        return make_ref_counted<const GreenPostfixExpression>(std::move(operand), std::move(op));
    }

    RefCountPtr<GreenNode> GreenPostfixExpression::clone_internal() const
    {
        return make_ref_counted<GreenPostfixExpression>(operand_, op_);
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

    void GreenTernaryExpression::set_condition(GreenPtr<GreenExpression> value) noexcept
    {
        condition_ = std::move(value);
    }

    void GreenTernaryExpression::set_question_mark(GreenPtr<GreenToken> value) noexcept
    {
        question_mark_ = std::move(value);
    }

    void GreenTernaryExpression::set_when_true(GreenPtr<GreenExpression> value) noexcept
    {
        when_true_ = std::move(value);
    }

    void GreenTernaryExpression::set_colon(GreenPtr<GreenToken> value) noexcept
    {
        colon_ = std::move(value);
    }

    void GreenTernaryExpression::set_when_false(GreenPtr<GreenExpression> value) noexcept
    {
        when_false_ = std::move(value);
    }

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

    [[nodiscard]] const SyntaxNode &GreenTernaryExpression::create_red(SyntaxLifetime &lifetime,
                                                                       const SyntaxNode *parent,
                                                                       std::uint32_t position) const
    {
        return lifetime.add<TernaryExpressionSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenTernaryExpression> GreenTernaryExpression::with_condition(
        GreenPtr<GreenExpression> condition) const
    {
        return update(std::move(condition), question_mark_, when_true_, colon_, when_false_);
    }

    [[nodiscard]] GreenPtr<GreenTernaryExpression> GreenTernaryExpression::with_question_mark(
        GreenPtr<GreenToken> question_mark) const
    {
        return update(condition_, std::move(question_mark), when_true_, colon_, when_false_);
    }

    [[nodiscard]] GreenPtr<GreenTernaryExpression> GreenTernaryExpression::with_when_true(
        GreenPtr<GreenExpression> when_true) const
    {
        return update(condition_, question_mark_, std::move(when_true), colon_, when_false_);
    }

    [[nodiscard]] GreenPtr<GreenTernaryExpression> GreenTernaryExpression::with_colon(GreenPtr<GreenToken> colon) const
    {
        return update(condition_, question_mark_, when_true_, std::move(colon), when_false_);
    }

    [[nodiscard]] GreenPtr<GreenTernaryExpression> GreenTernaryExpression::with_when_false(
        GreenPtr<GreenExpression> when_false) const
    {
        return update(condition_, question_mark_, when_true_, colon_, std::move(when_false));
    }

    GreenPtr<GreenTernaryExpression> GreenTernaryExpression::update(GreenPtr<GreenExpression> condition,
                                                                    GreenPtr<GreenToken> question_mark,
                                                                    GreenPtr<GreenExpression> when_true,
                                                                    GreenPtr<GreenToken> colon,
                                                                    GreenPtr<GreenExpression> when_false) const
    {
        if (condition == condition_ && question_mark == question_mark_ && when_true == when_true_ && colon == colon_ &&
            when_false == when_false_)
            return shared_from_this();

        return make_ref_counted<const GreenTernaryExpression>(std::move(condition),
                                                              std::move(question_mark),
                                                              std::move(when_true),
                                                              std::move(colon),
                                                              std::move(when_false));
    }

    RefCountPtr<GreenNode> GreenTernaryExpression::clone_internal() const
    {
        return make_ref_counted<GreenTernaryExpression>(condition_, question_mark_, when_true_, colon_, when_false_);
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

    void GreenInvocationExpression::set_callee(GreenPtr<GreenExpression> value) noexcept
    {
        callee_ = std::move(value);
    }

    void GreenInvocationExpression::set_arguments(GreenPtr<GreenArgumentList> value) noexcept
    {
        arguments_ = std::move(value);
    }

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

    [[nodiscard]] const SyntaxNode &GreenInvocationExpression::create_red(SyntaxLifetime &lifetime,
                                                                          const SyntaxNode *parent,
                                                                          std::uint32_t position) const
    {
        return lifetime.add<InvocationExpressionSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenInvocationExpression> GreenInvocationExpression::with_callee(
        GreenPtr<GreenExpression> callee) const
    {
        return update(std::move(callee), arguments_);
    }

    [[nodiscard]] GreenPtr<GreenInvocationExpression> GreenInvocationExpression::with_arguments(
        GreenPtr<GreenArgumentList> arguments) const
    {
        return update(callee_, std::move(arguments));
    }

    GreenPtr<GreenInvocationExpression> GreenInvocationExpression::update(GreenPtr<GreenExpression> callee,
                                                                          GreenPtr<GreenArgumentList> arguments) const
    {
        if (callee == callee_ && arguments == arguments_)
            return shared_from_this();

        return make_ref_counted<const GreenInvocationExpression>(std::move(callee), std::move(arguments));
    }

    RefCountPtr<GreenNode> GreenInvocationExpression::clone_internal() const
    {
        return make_ref_counted<GreenInvocationExpression>(callee_, arguments_);
    }
} // namespace prism
