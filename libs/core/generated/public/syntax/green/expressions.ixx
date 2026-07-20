export module prism.core:syntax.green.expressions;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.separated_list;

namespace prism
{
    class GreenArgumentList;

    class GreenExpression : public GreenNode
    {
      protected:
        explicit constexpr GreenExpression(const SyntaxKind kind, DiagnosticInfoList diagnostics = {})
            : GreenNode{kind, std::move(diagnostics)}
        {
        }

      public:
        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::literal_expression || node.kind() == SyntaxKind::identifier_expression ||
                   node.kind() == SyntaxKind::parenthesized_expression ||
                   node.kind() == SyntaxKind::binary_expression || node.kind() == SyntaxKind::assignment_expression ||
                   node.kind() == SyntaxKind::prefix_expression || node.kind() == SyntaxKind::postfix_expression ||
                   node.kind() == SyntaxKind::ternary_expression || node.kind() == SyntaxKind::invocation_expression;
        }
    };

    class GreenLiteralExpression final : public GreenExpression
    {
      public:
        explicit GreenLiteralExpression(GreenPtr<GreenToken> value, DiagnosticInfoList diagnostics = {});

        ~GreenLiteralExpression() override;

        [[nodiscard]] constexpr const GreenToken &value() const noexcept
        {
            return *value_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::literal_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] GreenPtr<GreenLiteralExpression> with_value(GreenPtr<GreenToken> value) const;

        [[nodiscard]] GreenPtr<GreenLiteralExpression> update(GreenPtr<GreenToken> value) const;

      private:
        GreenPtr<GreenToken> value_;
    };

    class GreenIdentifierExpression final : public GreenExpression
    {
      public:
        explicit GreenIdentifierExpression(GreenPtr<GreenToken> value, DiagnosticInfoList diagnostics = {});

        ~GreenIdentifierExpression() override;

        [[nodiscard]] constexpr const GreenToken &value() const noexcept
        {
            return *value_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::identifier_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] GreenPtr<GreenIdentifierExpression> with_value(GreenPtr<GreenToken> value) const;

        [[nodiscard]] GreenPtr<GreenIdentifierExpression> update(GreenPtr<GreenToken> value) const;

      private:
        GreenPtr<GreenToken> value_;
    };

    class GreenParenthesizedExpression final : public GreenExpression
    {
      public:
        GreenParenthesizedExpression(GreenPtr<GreenToken> open,
                                     GreenPtr<GreenExpression> expression,
                                     GreenPtr<GreenToken> close,
                                     DiagnosticInfoList diagnostics = {});

        ~GreenParenthesizedExpression() override;

        [[nodiscard]] constexpr const GreenToken &open() const noexcept
        {
            return *open_;
        }

        [[nodiscard]] constexpr const GreenExpression &expression() const noexcept
        {
            return *expression_;
        }

        [[nodiscard]] constexpr const GreenToken &close() const noexcept
        {
            return *close_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::parenthesized_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] GreenPtr<GreenParenthesizedExpression> with_open(GreenPtr<GreenToken> open) const;

        [[nodiscard]] GreenPtr<GreenParenthesizedExpression> with_expression(
            GreenPtr<GreenExpression> expression) const;

        [[nodiscard]] GreenPtr<GreenParenthesizedExpression> with_close(GreenPtr<GreenToken> close) const;

        [[nodiscard]] GreenPtr<GreenParenthesizedExpression> update(GreenPtr<GreenToken> open,
                                                                    GreenPtr<GreenExpression> expression,
                                                                    GreenPtr<GreenToken> close) const;

      private:
        GreenPtr<GreenToken> open_;
        GreenPtr<GreenExpression> expression_;
        GreenPtr<GreenToken> close_;
    };

    class GreenBinaryExpression final : public GreenExpression
    {
      public:
        GreenBinaryExpression(GreenPtr<GreenExpression> left,
                              GreenPtr<GreenToken> op,
                              GreenPtr<GreenExpression> right,
                              DiagnosticInfoList diagnostics = {});

        ~GreenBinaryExpression() override;

        [[nodiscard]] constexpr const GreenExpression &left() const noexcept
        {
            return *left_;
        }

        [[nodiscard]] constexpr const GreenToken &op() const noexcept
        {
            return *op_;
        }

        [[nodiscard]] constexpr const GreenExpression &right() const noexcept
        {
            return *right_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::binary_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] GreenPtr<GreenBinaryExpression> with_left(GreenPtr<GreenExpression> left) const;

        [[nodiscard]] GreenPtr<GreenBinaryExpression> with_op(GreenPtr<GreenToken> op) const;

        [[nodiscard]] GreenPtr<GreenBinaryExpression> with_right(GreenPtr<GreenExpression> right) const;

        [[nodiscard]] GreenPtr<GreenBinaryExpression> update(GreenPtr<GreenExpression> left,
                                                             GreenPtr<GreenToken> op,
                                                             GreenPtr<GreenExpression> right) const;

      private:
        GreenPtr<GreenExpression> left_;
        GreenPtr<GreenToken> op_;
        GreenPtr<GreenExpression> right_;
    };

    class GreenAssignmentExpression final : public GreenExpression
    {
      public:
        GreenAssignmentExpression(GreenPtr<GreenExpression> left,
                                  GreenPtr<GreenToken> op,
                                  GreenPtr<GreenExpression> right,
                                  DiagnosticInfoList diagnostics = {});

        ~GreenAssignmentExpression() override;

        [[nodiscard]] constexpr const GreenExpression &left() const noexcept
        {
            return *left_;
        }

        [[nodiscard]] constexpr const GreenToken &op() const noexcept
        {
            return *op_;
        }

        [[nodiscard]] constexpr const GreenExpression &right() const noexcept
        {
            return *right_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::assignment_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] GreenPtr<GreenAssignmentExpression> with_left(GreenPtr<GreenExpression> left) const;

        [[nodiscard]] GreenPtr<GreenAssignmentExpression> with_op(GreenPtr<GreenToken> op) const;

        [[nodiscard]] GreenPtr<GreenAssignmentExpression> with_right(GreenPtr<GreenExpression> right) const;

        [[nodiscard]] GreenPtr<GreenAssignmentExpression> update(GreenPtr<GreenExpression> left,
                                                                 GreenPtr<GreenToken> op,
                                                                 GreenPtr<GreenExpression> right) const;

      private:
        GreenPtr<GreenExpression> left_;
        GreenPtr<GreenToken> op_;
        GreenPtr<GreenExpression> right_;
    };

    class GreenPrefixExpression final : public GreenExpression
    {
      public:
        GreenPrefixExpression(GreenPtr<GreenToken> op,
                              GreenPtr<GreenExpression> operand,
                              DiagnosticInfoList diagnostics = {});

        ~GreenPrefixExpression() override;

        [[nodiscard]] constexpr const GreenToken &op() const noexcept
        {
            return *op_;
        }

        [[nodiscard]] constexpr const GreenExpression &operand() const noexcept
        {
            return *operand_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::prefix_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] GreenPtr<GreenPrefixExpression> with_op(GreenPtr<GreenToken> op) const;

        [[nodiscard]] GreenPtr<GreenPrefixExpression> with_operand(GreenPtr<GreenExpression> operand) const;

        [[nodiscard]] GreenPtr<GreenPrefixExpression> update(GreenPtr<GreenToken> op,
                                                             GreenPtr<GreenExpression> operand) const;

      private:
        GreenPtr<GreenToken> op_;
        GreenPtr<GreenExpression> operand_;
    };

    class GreenPostfixExpression final : public GreenExpression
    {
      public:
        GreenPostfixExpression(GreenPtr<GreenExpression> operand,
                               GreenPtr<GreenToken> op,
                               DiagnosticInfoList diagnostics = {});

        ~GreenPostfixExpression() override;

        [[nodiscard]] constexpr const GreenExpression &operand() const noexcept
        {
            return *operand_;
        }

        [[nodiscard]] constexpr const GreenToken &op() const noexcept
        {
            return *op_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::postfix_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] GreenPtr<GreenPostfixExpression> with_operand(GreenPtr<GreenExpression> operand) const;

        [[nodiscard]] GreenPtr<GreenPostfixExpression> with_op(GreenPtr<GreenToken> op) const;

        [[nodiscard]] GreenPtr<GreenPostfixExpression> update(GreenPtr<GreenExpression> operand,
                                                              GreenPtr<GreenToken> op) const;

      private:
        GreenPtr<GreenExpression> operand_;
        GreenPtr<GreenToken> op_;
    };

    class GreenTernaryExpression final : public GreenExpression
    {
      public:
        GreenTernaryExpression(GreenPtr<GreenExpression> condition,
                               GreenPtr<GreenToken> question_mark,
                               GreenPtr<GreenExpression> when_true,
                               GreenPtr<GreenToken> colon,
                               GreenPtr<GreenExpression> when_false,
                               DiagnosticInfoList diagnostics = {});

        ~GreenTernaryExpression() override;

        [[nodiscard]] constexpr const GreenExpression &condition() const noexcept
        {
            return *condition_;
        }

        [[nodiscard]] constexpr const GreenToken &question_mark() const noexcept
        {
            return *question_mark_;
        }

        [[nodiscard]] constexpr const GreenExpression &when_true() const noexcept
        {
            return *when_true_;
        }

        [[nodiscard]] constexpr const GreenToken &colon() const noexcept
        {
            return *colon_;
        }

        [[nodiscard]] constexpr const GreenExpression &when_false() const noexcept
        {
            return *when_false_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::ternary_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] GreenPtr<GreenTernaryExpression> with_condition(GreenPtr<GreenExpression> condition) const;

        [[nodiscard]] GreenPtr<GreenTernaryExpression> with_question_mark(GreenPtr<GreenToken> question_mark) const;

        [[nodiscard]] GreenPtr<GreenTernaryExpression> with_when_true(GreenPtr<GreenExpression> when_true) const;

        [[nodiscard]] GreenPtr<GreenTernaryExpression> with_colon(GreenPtr<GreenToken> colon) const;

        [[nodiscard]] GreenPtr<GreenTernaryExpression> with_when_false(GreenPtr<GreenExpression> when_false) const;

        [[nodiscard]] GreenPtr<GreenTernaryExpression> update(GreenPtr<GreenExpression> condition,
                                                              GreenPtr<GreenToken> question_mark,
                                                              GreenPtr<GreenExpression> when_true,
                                                              GreenPtr<GreenToken> colon,
                                                              GreenPtr<GreenExpression> when_false) const;

      private:
        GreenPtr<GreenExpression> condition_;
        GreenPtr<GreenToken> question_mark_;
        GreenPtr<GreenExpression> when_true_;
        GreenPtr<GreenToken> colon_;
        GreenPtr<GreenExpression> when_false_;
    };

    class GreenInvocationExpression final : public GreenExpression
    {
      public:
        GreenInvocationExpression(GreenPtr<GreenExpression> callee,
                                  GreenPtr<GreenArgumentList> arguments,
                                  DiagnosticInfoList diagnostics = {});

        ~GreenInvocationExpression() override;

        [[nodiscard]] constexpr const GreenExpression &callee() const noexcept
        {
            return *callee_;
        }

        [[nodiscard]] constexpr const GreenArgumentList &arguments() const noexcept
        {
            return *arguments_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::invocation_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] GreenPtr<GreenInvocationExpression> with_callee(GreenPtr<GreenExpression> callee) const;

        [[nodiscard]] GreenPtr<GreenInvocationExpression> with_arguments(GreenPtr<GreenArgumentList> arguments) const;

        [[nodiscard]] GreenPtr<GreenInvocationExpression> update(GreenPtr<GreenExpression> callee,
                                                                 GreenPtr<GreenArgumentList> arguments) const;

      private:
        GreenPtr<GreenExpression> callee_;
        GreenPtr<GreenArgumentList> arguments_;
    };
} // namespace prism
