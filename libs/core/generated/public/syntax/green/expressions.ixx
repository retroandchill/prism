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
        explicit GreenLiteralExpression(RefCountPtr<const GreenToken> value, DiagnosticInfoList diagnostics = {});

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

        [[nodiscard]] RefCountPtr<const GreenLiteralExpression> with_value(RefCountPtr<const GreenToken> value) const;

        [[nodiscard]] RefCountPtr<const GreenLiteralExpression> update(RefCountPtr<const GreenToken> value) const;

      private:
        RefCountPtr<const GreenToken> value_;
    };

    class GreenIdentifierExpression final : public GreenExpression
    {
      public:
        explicit GreenIdentifierExpression(RefCountPtr<const GreenToken> value, DiagnosticInfoList diagnostics = {});

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

        [[nodiscard]] RefCountPtr<const GreenIdentifierExpression> with_value(
            RefCountPtr<const GreenToken> value) const;

        [[nodiscard]] RefCountPtr<const GreenIdentifierExpression> update(RefCountPtr<const GreenToken> value) const;

      private:
        RefCountPtr<const GreenToken> value_;
    };

    class GreenParenthesizedExpression final : public GreenExpression
    {
      public:
        GreenParenthesizedExpression(RefCountPtr<const GreenToken> open,
                                     RefCountPtr<const GreenExpression> expression,
                                     RefCountPtr<const GreenToken> close,
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

        [[nodiscard]] RefCountPtr<const GreenParenthesizedExpression> with_open(
            RefCountPtr<const GreenToken> open) const;

        [[nodiscard]] RefCountPtr<const GreenParenthesizedExpression> with_expression(
            RefCountPtr<const GreenExpression> expression) const;

        [[nodiscard]] RefCountPtr<const GreenParenthesizedExpression> with_close(
            RefCountPtr<const GreenToken> close) const;

        [[nodiscard]] RefCountPtr<const GreenParenthesizedExpression> update(
            RefCountPtr<const GreenToken> open,
            RefCountPtr<const GreenExpression> expression,
            RefCountPtr<const GreenToken> close) const;

      private:
        RefCountPtr<const GreenToken> open_;
        RefCountPtr<const GreenExpression> expression_;
        RefCountPtr<const GreenToken> close_;
    };

    class GreenBinaryExpression final : public GreenExpression
    {
      public:
        GreenBinaryExpression(RefCountPtr<const GreenExpression> left,
                              RefCountPtr<const GreenToken> op,
                              RefCountPtr<const GreenExpression> right,
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

        [[nodiscard]] RefCountPtr<const GreenBinaryExpression> with_left(RefCountPtr<const GreenExpression> left) const;

        [[nodiscard]] RefCountPtr<const GreenBinaryExpression> with_op(RefCountPtr<const GreenToken> op) const;

        [[nodiscard]] RefCountPtr<const GreenBinaryExpression> with_right(
            RefCountPtr<const GreenExpression> right) const;

        [[nodiscard]] RefCountPtr<const GreenBinaryExpression> update(RefCountPtr<const GreenExpression> left,
                                                                      RefCountPtr<const GreenToken> op,
                                                                      RefCountPtr<const GreenExpression> right) const;

      private:
        RefCountPtr<const GreenExpression> left_;
        RefCountPtr<const GreenToken> op_;
        RefCountPtr<const GreenExpression> right_;
    };

    class GreenAssignmentExpression final : public GreenExpression
    {
      public:
        GreenAssignmentExpression(RefCountPtr<const GreenExpression> left,
                                  RefCountPtr<const GreenToken> op,
                                  RefCountPtr<const GreenExpression> right,
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

        [[nodiscard]] RefCountPtr<const GreenAssignmentExpression> with_left(
            RefCountPtr<const GreenExpression> left) const;

        [[nodiscard]] RefCountPtr<const GreenAssignmentExpression> with_op(RefCountPtr<const GreenToken> op) const;

        [[nodiscard]] RefCountPtr<const GreenAssignmentExpression> with_right(
            RefCountPtr<const GreenExpression> right) const;

        [[nodiscard]] RefCountPtr<const GreenAssignmentExpression> update(
            RefCountPtr<const GreenExpression> left,
            RefCountPtr<const GreenToken> op,
            RefCountPtr<const GreenExpression> right) const;

      private:
        RefCountPtr<const GreenExpression> left_;
        RefCountPtr<const GreenToken> op_;
        RefCountPtr<const GreenExpression> right_;
    };

    class GreenPrefixExpression final : public GreenExpression
    {
      public:
        GreenPrefixExpression(RefCountPtr<const GreenToken> op,
                              RefCountPtr<const GreenExpression> operand,
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

        [[nodiscard]] RefCountPtr<const GreenPrefixExpression> with_op(RefCountPtr<const GreenToken> op) const;

        [[nodiscard]] RefCountPtr<const GreenPrefixExpression> with_operand(
            RefCountPtr<const GreenExpression> operand) const;

        [[nodiscard]] RefCountPtr<const GreenPrefixExpression> update(RefCountPtr<const GreenToken> op,
                                                                      RefCountPtr<const GreenExpression> operand) const;

      private:
        RefCountPtr<const GreenToken> op_;
        RefCountPtr<const GreenExpression> operand_;
    };

    class GreenPostfixExpression final : public GreenExpression
    {
      public:
        GreenPostfixExpression(RefCountPtr<const GreenExpression> operand,
                               RefCountPtr<const GreenToken> op,
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

        [[nodiscard]] RefCountPtr<const GreenPostfixExpression> with_operand(
            RefCountPtr<const GreenExpression> operand) const;

        [[nodiscard]] RefCountPtr<const GreenPostfixExpression> with_op(RefCountPtr<const GreenToken> op) const;

        [[nodiscard]] RefCountPtr<const GreenPostfixExpression> update(RefCountPtr<const GreenExpression> operand,
                                                                       RefCountPtr<const GreenToken> op) const;

      private:
        RefCountPtr<const GreenExpression> operand_;
        RefCountPtr<const GreenToken> op_;
    };

    class GreenTernaryExpression final : public GreenExpression
    {
      public:
        GreenTernaryExpression(RefCountPtr<const GreenExpression> condition,
                               RefCountPtr<const GreenToken> question_mark,
                               RefCountPtr<const GreenExpression> when_true,
                               RefCountPtr<const GreenToken> colon,
                               RefCountPtr<const GreenExpression> when_false,
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

        [[nodiscard]] RefCountPtr<const GreenTernaryExpression> with_condition(
            RefCountPtr<const GreenExpression> condition) const;

        [[nodiscard]] RefCountPtr<const GreenTernaryExpression> with_question_mark(
            RefCountPtr<const GreenToken> question_mark) const;

        [[nodiscard]] RefCountPtr<const GreenTernaryExpression> with_when_true(
            RefCountPtr<const GreenExpression> when_true) const;

        [[nodiscard]] RefCountPtr<const GreenTernaryExpression> with_colon(RefCountPtr<const GreenToken> colon) const;

        [[nodiscard]] RefCountPtr<const GreenTernaryExpression> with_when_false(
            RefCountPtr<const GreenExpression> when_false) const;

        [[nodiscard]] RefCountPtr<const GreenTernaryExpression> update(
            RefCountPtr<const GreenExpression> condition,
            RefCountPtr<const GreenToken> question_mark,
            RefCountPtr<const GreenExpression> when_true,
            RefCountPtr<const GreenToken> colon,
            RefCountPtr<const GreenExpression> when_false) const;

      private:
        RefCountPtr<const GreenExpression> condition_;
        RefCountPtr<const GreenToken> question_mark_;
        RefCountPtr<const GreenExpression> when_true_;
        RefCountPtr<const GreenToken> colon_;
        RefCountPtr<const GreenExpression> when_false_;
    };

    class GreenInvocationExpression final : public GreenExpression
    {
      public:
        GreenInvocationExpression(RefCountPtr<const GreenExpression> callee,
                                  RefCountPtr<const GreenArgumentList> arguments,
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

        [[nodiscard]] RefCountPtr<const GreenInvocationExpression> with_callee(
            RefCountPtr<const GreenExpression> callee) const;

        [[nodiscard]] RefCountPtr<const GreenInvocationExpression> with_arguments(
            RefCountPtr<const GreenArgumentList> arguments) const;

        [[nodiscard]] RefCountPtr<const GreenInvocationExpression> update(
            RefCountPtr<const GreenExpression> callee,
            RefCountPtr<const GreenArgumentList> arguments) const;

      private:
        RefCountPtr<const GreenExpression> callee_;
        RefCountPtr<const GreenArgumentList> arguments_;
    };
} // namespace prism
