export module prism.core:syntax.green.expressions;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.trivia;
import :syntax.green.separated_list;

namespace prism
{
    class GreenArgumentList;
    class GreenName;
    class GreenType;

    class GreenExpression : public GreenNode
    {
      protected:
        explicit constexpr GreenExpression(const SyntaxKind kind, DiagnosticInfoList diagnostics = {})
            : GreenNode{kind, std::move(diagnostics)}
        {
        }

      public:
        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::literal_expression || node.kind() == SyntaxKind::identifier_expression ||
                   node.kind() == SyntaxKind::parenthesized_expression ||
                   node.kind() == SyntaxKind::binary_expression || node.kind() == SyntaxKind::assignment_expression ||
                   node.kind() == SyntaxKind::prefix_expression || node.kind() == SyntaxKind::postfix_expression ||
                   node.kind() == SyntaxKind::ternary_expression || node.kind() == SyntaxKind::invocation_expression ||
                   node.kind() == SyntaxKind::cast_expression;
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

        void set_value(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::literal_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenLiteralExpression> with_value(GreenPtr<GreenToken> value) const;

        [[nodiscard]] GreenPtr<GreenLiteralExpression> update(GreenPtr<GreenToken> value) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> value_;
    };

    template <>
    struct GreenNodeTraits<GreenLiteralExpression>
    {
        static constexpr std::size_t slot_count = 1;

        using ChildTypes = std::tuple<GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenLiteralExpression &node)
        {
            {
                static_assert(N == 0);
                return node.value();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenLiteralExpression>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenLiteralExpression &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                node.set_value(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenLiteralExpression>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenLiteralExpression> with(const GreenLiteralExpression &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                return node.with_value(std::forward<Arg>(value));
            }
        }
    };

    class GreenIdentifierExpression final : public GreenExpression
    {
      public:
        explicit GreenIdentifierExpression(GreenPtr<GreenName> value, DiagnosticInfoList diagnostics = {});

        ~GreenIdentifierExpression() override;

        [[nodiscard]] constexpr const GreenName &value() const noexcept
        {
            return *value_;
        }

        void set_value(GreenPtr<GreenName> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::identifier_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenIdentifierExpression> with_value(GreenPtr<GreenName> value) const;

        [[nodiscard]] GreenPtr<GreenIdentifierExpression> update(GreenPtr<GreenName> value) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenName> value_;
    };

    template <>
    struct GreenNodeTraits<GreenIdentifierExpression>
    {
        static constexpr std::size_t slot_count = 1;

        using ChildTypes = std::tuple<GreenName>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenIdentifierExpression &node)
        {
            {
                static_assert(N == 0);
                return node.value();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenIdentifierExpression>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenIdentifierExpression &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                node.set_value(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenIdentifierExpression>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenIdentifierExpression> with(const GreenIdentifierExpression &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                return node.with_value(std::forward<Arg>(value));
            }
        }
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

        void set_open(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenExpression &expression() const noexcept
        {
            return *expression_;
        }

        void set_expression(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &close() const noexcept
        {
            return *close_;
        }

        void set_close(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::parenthesized_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenParenthesizedExpression> with_open(GreenPtr<GreenToken> open) const;

        [[nodiscard]] GreenPtr<GreenParenthesizedExpression> with_expression(
            GreenPtr<GreenExpression> expression) const;

        [[nodiscard]] GreenPtr<GreenParenthesizedExpression> with_close(GreenPtr<GreenToken> close) const;

        [[nodiscard]] GreenPtr<GreenParenthesizedExpression> update(GreenPtr<GreenToken> open,
                                                                    GreenPtr<GreenExpression> expression,
                                                                    GreenPtr<GreenToken> close) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> open_;
        GreenPtr<GreenExpression> expression_;
        GreenPtr<GreenToken> close_;
    };

    template <>
    struct GreenNodeTraits<GreenParenthesizedExpression>
    {
        static constexpr std::size_t slot_count = 3;

        using ChildTypes = std::tuple<GreenToken, GreenExpression, GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenParenthesizedExpression &node)
        {
            if constexpr (N == 0)
            {
                return node.open();
            }
            else if constexpr (N == 1)
            {
                return node.expression();
            }
            else
            {
                static_assert(N == 2);
                return node.close();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenParenthesizedExpression>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenParenthesizedExpression &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_open(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_expression(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                node.set_close(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenParenthesizedExpression>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenParenthesizedExpression> with(const GreenParenthesizedExpression &node,
                                                                     Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_open(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_expression(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                return node.with_close(std::forward<Arg>(value));
            }
        }
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

        void set_left(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &op() const noexcept
        {
            return *op_;
        }

        void set_op(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenExpression &right() const noexcept
        {
            return *right_;
        }

        void set_right(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::binary_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenBinaryExpression> with_left(GreenPtr<GreenExpression> left) const;

        [[nodiscard]] GreenPtr<GreenBinaryExpression> with_op(GreenPtr<GreenToken> op) const;

        [[nodiscard]] GreenPtr<GreenBinaryExpression> with_right(GreenPtr<GreenExpression> right) const;

        [[nodiscard]] GreenPtr<GreenBinaryExpression> update(GreenPtr<GreenExpression> left,
                                                             GreenPtr<GreenToken> op,
                                                             GreenPtr<GreenExpression> right) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenExpression> left_;
        GreenPtr<GreenToken> op_;
        GreenPtr<GreenExpression> right_;
    };

    template <>
    struct GreenNodeTraits<GreenBinaryExpression>
    {
        static constexpr std::size_t slot_count = 3;

        using ChildTypes = std::tuple<GreenExpression, GreenToken, GreenExpression>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenBinaryExpression &node)
        {
            if constexpr (N == 0)
            {
                return node.left();
            }
            else if constexpr (N == 1)
            {
                return node.op();
            }
            else
            {
                static_assert(N == 2);
                return node.right();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenBinaryExpression>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenBinaryExpression &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_left(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_op(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                node.set_right(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenBinaryExpression>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenBinaryExpression> with(const GreenBinaryExpression &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_left(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_op(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                return node.with_right(std::forward<Arg>(value));
            }
        }
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

        void set_left(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &op() const noexcept
        {
            return *op_;
        }

        void set_op(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenExpression &right() const noexcept
        {
            return *right_;
        }

        void set_right(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::assignment_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenAssignmentExpression> with_left(GreenPtr<GreenExpression> left) const;

        [[nodiscard]] GreenPtr<GreenAssignmentExpression> with_op(GreenPtr<GreenToken> op) const;

        [[nodiscard]] GreenPtr<GreenAssignmentExpression> with_right(GreenPtr<GreenExpression> right) const;

        [[nodiscard]] GreenPtr<GreenAssignmentExpression> update(GreenPtr<GreenExpression> left,
                                                                 GreenPtr<GreenToken> op,
                                                                 GreenPtr<GreenExpression> right) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenExpression> left_;
        GreenPtr<GreenToken> op_;
        GreenPtr<GreenExpression> right_;
    };

    template <>
    struct GreenNodeTraits<GreenAssignmentExpression>
    {
        static constexpr std::size_t slot_count = 3;

        using ChildTypes = std::tuple<GreenExpression, GreenToken, GreenExpression>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenAssignmentExpression &node)
        {
            if constexpr (N == 0)
            {
                return node.left();
            }
            else if constexpr (N == 1)
            {
                return node.op();
            }
            else
            {
                static_assert(N == 2);
                return node.right();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenAssignmentExpression>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenAssignmentExpression &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_left(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_op(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                node.set_right(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenAssignmentExpression>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenAssignmentExpression> with(const GreenAssignmentExpression &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_left(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_op(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                return node.with_right(std::forward<Arg>(value));
            }
        }
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

        void set_op(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenExpression &operand() const noexcept
        {
            return *operand_;
        }

        void set_operand(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::prefix_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenPrefixExpression> with_op(GreenPtr<GreenToken> op) const;

        [[nodiscard]] GreenPtr<GreenPrefixExpression> with_operand(GreenPtr<GreenExpression> operand) const;

        [[nodiscard]] GreenPtr<GreenPrefixExpression> update(GreenPtr<GreenToken> op,
                                                             GreenPtr<GreenExpression> operand) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> op_;
        GreenPtr<GreenExpression> operand_;
    };

    template <>
    struct GreenNodeTraits<GreenPrefixExpression>
    {
        static constexpr std::size_t slot_count = 2;

        using ChildTypes = std::tuple<GreenToken, GreenExpression>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenPrefixExpression &node)
        {
            if constexpr (N == 0)
            {
                return node.op();
            }
            else
            {
                static_assert(N == 1);
                return node.operand();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenPrefixExpression>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenPrefixExpression &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_op(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                node.set_operand(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenPrefixExpression>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenPrefixExpression> with(const GreenPrefixExpression &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_op(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                return node.with_operand(std::forward<Arg>(value));
            }
        }
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

        void set_operand(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &op() const noexcept
        {
            return *op_;
        }

        void set_op(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::postfix_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenPostfixExpression> with_operand(GreenPtr<GreenExpression> operand) const;

        [[nodiscard]] GreenPtr<GreenPostfixExpression> with_op(GreenPtr<GreenToken> op) const;

        [[nodiscard]] GreenPtr<GreenPostfixExpression> update(GreenPtr<GreenExpression> operand,
                                                              GreenPtr<GreenToken> op) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenExpression> operand_;
        GreenPtr<GreenToken> op_;
    };

    template <>
    struct GreenNodeTraits<GreenPostfixExpression>
    {
        static constexpr std::size_t slot_count = 2;

        using ChildTypes = std::tuple<GreenExpression, GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenPostfixExpression &node)
        {
            if constexpr (N == 0)
            {
                return node.operand();
            }
            else
            {
                static_assert(N == 1);
                return node.op();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenPostfixExpression>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenPostfixExpression &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_operand(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                node.set_op(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenPostfixExpression>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenPostfixExpression> with(const GreenPostfixExpression &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_operand(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                return node.with_op(std::forward<Arg>(value));
            }
        }
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

        void set_condition(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &question_mark() const noexcept
        {
            return *question_mark_;
        }

        void set_question_mark(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenExpression &when_true() const noexcept
        {
            return *when_true_;
        }

        void set_when_true(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &colon() const noexcept
        {
            return *colon_;
        }

        void set_colon(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenExpression &when_false() const noexcept
        {
            return *when_false_;
        }

        void set_when_false(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::ternary_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

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
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenExpression> condition_;
        GreenPtr<GreenToken> question_mark_;
        GreenPtr<GreenExpression> when_true_;
        GreenPtr<GreenToken> colon_;
        GreenPtr<GreenExpression> when_false_;
    };

    template <>
    struct GreenNodeTraits<GreenTernaryExpression>
    {
        static constexpr std::size_t slot_count = 5;

        using ChildTypes = std::tuple<GreenExpression, GreenToken, GreenExpression, GreenToken, GreenExpression>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenTernaryExpression &node)
        {
            if constexpr (N == 0)
            {
                return node.condition();
            }
            else if constexpr (N == 1)
            {
                return node.question_mark();
            }
            else if constexpr (N == 2)
            {
                return node.when_true();
            }
            else if constexpr (N == 3)
            {
                return node.colon();
            }
            else
            {
                static_assert(N == 4);
                return node.when_false();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenTernaryExpression>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenTernaryExpression &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_condition(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_question_mark(std::forward<Arg>(value));
            }
            else if constexpr (N == 2)
            {
                node.set_when_true(std::forward<Arg>(value));
            }
            else if constexpr (N == 3)
            {
                node.set_colon(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 4);
                node.set_when_false(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenTernaryExpression>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenTernaryExpression> with(const GreenTernaryExpression &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_condition(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_question_mark(std::forward<Arg>(value));
            }
            else if constexpr (N == 2)
            {
                return node.with_when_true(std::forward<Arg>(value));
            }
            else if constexpr (N == 3)
            {
                return node.with_colon(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 4);
                return node.with_when_false(std::forward<Arg>(value));
            }
        }
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

        void set_callee(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] constexpr const GreenArgumentList &arguments() const noexcept
        {
            return *arguments_;
        }

        void set_arguments(GreenPtr<GreenArgumentList> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::invocation_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenInvocationExpression> with_callee(GreenPtr<GreenExpression> callee) const;

        [[nodiscard]] GreenPtr<GreenInvocationExpression> with_arguments(GreenPtr<GreenArgumentList> arguments) const;

        [[nodiscard]] GreenPtr<GreenInvocationExpression> update(GreenPtr<GreenExpression> callee,
                                                                 GreenPtr<GreenArgumentList> arguments) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenExpression> callee_;
        GreenPtr<GreenArgumentList> arguments_;
    };

    template <>
    struct GreenNodeTraits<GreenInvocationExpression>
    {
        static constexpr std::size_t slot_count = 2;

        using ChildTypes = std::tuple<GreenExpression, GreenArgumentList>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenInvocationExpression &node)
        {
            if constexpr (N == 0)
            {
                return node.callee();
            }
            else
            {
                static_assert(N == 1);
                return node.arguments();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenInvocationExpression>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenInvocationExpression &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_callee(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                node.set_arguments(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenInvocationExpression>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenInvocationExpression> with(const GreenInvocationExpression &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_callee(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                return node.with_arguments(std::forward<Arg>(value));
            }
        }
    };

    class GreenCastExpression final : public GreenExpression
    {
      public:
        GreenCastExpression(GreenPtr<GreenExpression> operand,
                            GreenPtr<GreenToken> as,
                            GreenPtr<GreenType> type,
                            DiagnosticInfoList diagnostics = {});

        ~GreenCastExpression() override;

        [[nodiscard]] constexpr const GreenExpression &operand() const noexcept
        {
            return *operand_;
        }

        void set_operand(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &as() const noexcept
        {
            return *as_;
        }

        void set_as(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenType &type() const noexcept
        {
            return *type_;
        }

        void set_type(GreenPtr<GreenType> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::cast_expression;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenCastExpression> with_operand(GreenPtr<GreenExpression> operand) const;

        [[nodiscard]] GreenPtr<GreenCastExpression> with_as(GreenPtr<GreenToken> as) const;

        [[nodiscard]] GreenPtr<GreenCastExpression> with_type(GreenPtr<GreenType> type) const;

        [[nodiscard]] GreenPtr<GreenCastExpression> update(GreenPtr<GreenExpression> operand,
                                                           GreenPtr<GreenToken> as,
                                                           GreenPtr<GreenType> type) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenExpression> operand_;
        GreenPtr<GreenToken> as_;
        GreenPtr<GreenType> type_;
    };

    template <>
    struct GreenNodeTraits<GreenCastExpression>
    {
        static constexpr std::size_t slot_count = 3;

        using ChildTypes = std::tuple<GreenExpression, GreenToken, GreenType>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenCastExpression &node)
        {
            if constexpr (N == 0)
            {
                return node.operand();
            }
            else if constexpr (N == 1)
            {
                return node.as();
            }
            else
            {
                static_assert(N == 2);
                return node.type();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenCastExpression>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenCastExpression &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_operand(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_as(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                node.set_type(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenCastExpression>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenCastExpression> with(const GreenCastExpression &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_operand(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_as(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                return node.with_type(std::forward<Arg>(value));
            }
        }
    };
} // namespace prism
