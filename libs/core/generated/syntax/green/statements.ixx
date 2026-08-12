export module prism.core:syntax.green.statements;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.trivia;
import :syntax.green.separated_list;

namespace prism
{
    class GreenExpression;
    class GreenVariableDeclaration;

    class GreenStatement : public GreenNode
    {
      protected:
        explicit constexpr GreenStatement(const SyntaxKind kind, DiagnosticInfoList diagnostics = {})
            : GreenNode{kind, std::move(diagnostics)}
        {
        }

      public:
        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::variable_declaration_statement || node.kind() == SyntaxKind::block ||
                   node.kind() == SyntaxKind::return_statement || node.kind() == SyntaxKind::expression_statement ||
                   node.kind() == SyntaxKind::empty_statement;
        }
    };

    class GreenVariableDeclarationStatement final : public GreenStatement
    {
      public:
        explicit GreenVariableDeclarationStatement(GreenPtr<GreenVariableDeclaration> declaration,
                                                   DiagnosticInfoList diagnostics = {});

        ~GreenVariableDeclarationStatement() override;

        [[nodiscard]] constexpr const GreenVariableDeclaration &declaration() const noexcept
        {
            return *declaration_;
        }

        void set_declaration(GreenPtr<GreenVariableDeclaration> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::variable_declaration_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenVariableDeclarationStatement> with_declaration(
            GreenPtr<GreenVariableDeclaration> declaration) const;

        [[nodiscard]] GreenPtr<GreenVariableDeclarationStatement> update(
            GreenPtr<GreenVariableDeclaration> declaration) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenVariableDeclaration> declaration_;
    };

    template <>
    struct GreenNodeTraits<GreenVariableDeclarationStatement>
    {
        static constexpr std::size_t slot_count = 1;

        using ChildTypes = std::tuple<GreenVariableDeclaration>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenVariableDeclarationStatement &node)
        {
            {
                static_assert(N == 0);
                return node.declaration();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenVariableDeclarationStatement>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenVariableDeclarationStatement &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                node.set_declaration(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenVariableDeclarationStatement>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenVariableDeclarationStatement> with(const GreenVariableDeclarationStatement &node,
                                                                          Arg &&value)
        {
            {
                static_assert(N == 0);
                return node.with_declaration(std::forward<Arg>(value));
            }
        }
    };

    class GreenBlock final : public GreenStatement
    {
      public:
        GreenBlock(GreenPtr<GreenToken> open_brace,
                   GreenSyntaxList<GreenStatement> statements,
                   GreenPtr<GreenToken> close_brace,
                   DiagnosticInfoList diagnostics = {});

        ~GreenBlock() override;

        [[nodiscard]] constexpr const GreenToken &open_brace() const noexcept
        {
            return *open_brace_;
        }

        void set_open_brace(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenSyntaxList<GreenStatement> &statements() const noexcept
        {
            return statements_;
        }

        void set_statements(GreenSyntaxList<GreenStatement> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &close_brace() const noexcept
        {
            return *close_brace_;
        }

        void set_close_brace(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::block;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenBlock> with_open_brace(GreenPtr<GreenToken> open_brace) const;

        [[nodiscard]] GreenPtr<GreenBlock> with_statements(GreenSyntaxList<GreenStatement> statements) const;

        [[nodiscard]] GreenPtr<GreenBlock> with_close_brace(GreenPtr<GreenToken> close_brace) const;

        [[nodiscard]] GreenPtr<GreenBlock> update(GreenPtr<GreenToken> open_brace,
                                                  GreenSyntaxList<GreenStatement> statements,
                                                  GreenPtr<GreenToken> close_brace) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> open_brace_;
        GreenSyntaxList<GreenStatement> statements_;
        GreenPtr<GreenToken> close_brace_;
    };

    template <>
    struct GreenNodeTraits<GreenBlock>
    {
        static constexpr std::size_t slot_count = 3;

        using ChildTypes = std::tuple<GreenToken, GreenSyntaxList<GreenStatement>, GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenBlock &node)
        {
            if constexpr (N == 0)
            {
                return node.open_brace();
            }
            else if constexpr (N == 1)
            {
                return node.statements();
            }
            else
            {
                static_assert(N == 2);
                return node.close_brace();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenBlock>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenBlock &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_open_brace(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_statements(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                node.set_close_brace(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenBlock>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenBlock> with(const GreenBlock &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_open_brace(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_statements(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                return node.with_close_brace(std::forward<Arg>(value));
            }
        }
    };

    class GreenReturnStatement final : public GreenStatement
    {
      public:
        GreenReturnStatement(GreenPtr<GreenToken> return_keyword,
                             GreenPtr<GreenExpression> expression,
                             GreenPtr<GreenToken> semicolon,
                             DiagnosticInfoList diagnostics = {});

        ~GreenReturnStatement() override;

        [[nodiscard]] constexpr const GreenToken &return_keyword() const noexcept
        {
            return *return_keyword_;
        }

        void set_return_keyword(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr Optional<const GreenExpression &> expression() const noexcept
        {
            return expression_.get();
        }

        void set_expression(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &semicolon() const noexcept
        {
            return *semicolon_;
        }

        void set_semicolon(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::return_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenReturnStatement> with_return_keyword(GreenPtr<GreenToken> return_keyword) const;

        [[nodiscard]] GreenPtr<GreenReturnStatement> with_expression(GreenPtr<GreenExpression> expression) const;

        [[nodiscard]] GreenPtr<GreenReturnStatement> with_semicolon(GreenPtr<GreenToken> semicolon) const;

        [[nodiscard]] GreenPtr<GreenReturnStatement> update(GreenPtr<GreenToken> return_keyword,
                                                            GreenPtr<GreenExpression> expression,
                                                            GreenPtr<GreenToken> semicolon) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> return_keyword_;
        GreenPtr<GreenExpression> expression_;
        GreenPtr<GreenToken> semicolon_;
    };

    template <>
    struct GreenNodeTraits<GreenReturnStatement>
    {
        static constexpr std::size_t slot_count = 3;

        using ChildTypes = std::tuple<GreenToken, GreenExpression, GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenReturnStatement &node)
        {
            if constexpr (N == 0)
            {
                return node.return_keyword();
            }
            else if constexpr (N == 1)
            {
                return node.expression();
            }
            else
            {
                static_assert(N == 2);
                return node.semicolon();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenReturnStatement>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenReturnStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_return_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_expression(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                node.set_semicolon(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenReturnStatement>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenReturnStatement> with(const GreenReturnStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_return_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_expression(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                return node.with_semicolon(std::forward<Arg>(value));
            }
        }
    };

    class GreenExpressionStatement final : public GreenStatement
    {
      public:
        GreenExpressionStatement(GreenPtr<GreenExpression> expression,
                                 GreenPtr<GreenToken> semicolon,
                                 DiagnosticInfoList diagnostics = {});

        ~GreenExpressionStatement() override;

        [[nodiscard]] constexpr const GreenExpression &expression() const noexcept
        {
            return *expression_;
        }

        void set_expression(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &semicolon() const noexcept
        {
            return *semicolon_;
        }

        void set_semicolon(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::expression_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenExpressionStatement> with_expression(GreenPtr<GreenExpression> expression) const;

        [[nodiscard]] GreenPtr<GreenExpressionStatement> with_semicolon(GreenPtr<GreenToken> semicolon) const;

        [[nodiscard]] GreenPtr<GreenExpressionStatement> update(GreenPtr<GreenExpression> expression,
                                                                GreenPtr<GreenToken> semicolon) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenExpression> expression_;
        GreenPtr<GreenToken> semicolon_;
    };

    template <>
    struct GreenNodeTraits<GreenExpressionStatement>
    {
        static constexpr std::size_t slot_count = 2;

        using ChildTypes = std::tuple<GreenExpression, GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenExpressionStatement &node)
        {
            if constexpr (N == 0)
            {
                return node.expression();
            }
            else
            {
                static_assert(N == 1);
                return node.semicolon();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenExpressionStatement>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenExpressionStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_expression(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                node.set_semicolon(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenExpressionStatement>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenExpressionStatement> with(const GreenExpressionStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_expression(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                return node.with_semicolon(std::forward<Arg>(value));
            }
        }
    };

    class GreenEmptyStatement final : public GreenStatement
    {
      public:
        explicit GreenEmptyStatement(GreenPtr<GreenToken> semicolon, DiagnosticInfoList diagnostics = {});

        ~GreenEmptyStatement() override;

        [[nodiscard]] constexpr const GreenToken &semicolon() const noexcept
        {
            return *semicolon_;
        }

        void set_semicolon(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::empty_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenEmptyStatement> with_semicolon(GreenPtr<GreenToken> semicolon) const;

        [[nodiscard]] GreenPtr<GreenEmptyStatement> update(GreenPtr<GreenToken> semicolon) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> semicolon_;
    };

    template <>
    struct GreenNodeTraits<GreenEmptyStatement>
    {
        static constexpr std::size_t slot_count = 1;

        using ChildTypes = std::tuple<GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenEmptyStatement &node)
        {
            {
                static_assert(N == 0);
                return node.semicolon();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenEmptyStatement>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenEmptyStatement &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                node.set_semicolon(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenEmptyStatement>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenEmptyStatement> with(const GreenEmptyStatement &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                return node.with_semicolon(std::forward<Arg>(value));
            }
        }
    };
} // namespace prism
