export module prism.core:syntax.green.statements;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.trivia;
import :syntax.green.separated_list;

namespace prism
{
    class GreenElseClause;
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
            return node.kind() == SyntaxKind::empty_statement ||
                   node.kind() == SyntaxKind::variable_declaration_statement || node.kind() == SyntaxKind::block ||
                   node.kind() == SyntaxKind::return_statement || node.kind() == SyntaxKind::expression_statement ||
                   node.kind() == SyntaxKind::if_statement || node.kind() == SyntaxKind::while_statement ||
                   node.kind() == SyntaxKind::loop_statement || node.kind() == SyntaxKind::for_statement ||
                   node.kind() == SyntaxKind::break_statement || node.kind() == SyntaxKind::continue_statement;
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

    class GreenIfStatement final : public GreenStatement
    {
      public:
        GreenIfStatement(GreenPtr<GreenToken> if_keyword,
                         GreenPtr<GreenToken> open_paren,
                         GreenPtr<GreenExpression> condition,
                         GreenPtr<GreenToken> close_paren,
                         GreenPtr<GreenBlock> block,
                         GreenPtr<GreenElseClause> else_clause,
                         DiagnosticInfoList diagnostics = {});

        ~GreenIfStatement() override;

        [[nodiscard]] constexpr const GreenToken &if_keyword() const noexcept
        {
            return *if_keyword_;
        }

        void set_if_keyword(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &open_paren() const noexcept
        {
            return *open_paren_;
        }

        void set_open_paren(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenExpression &condition() const noexcept
        {
            return *condition_;
        }

        void set_condition(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &close_paren() const noexcept
        {
            return *close_paren_;
        }

        void set_close_paren(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenBlock &block() const noexcept
        {
            return *block_;
        }

        void set_block(GreenPtr<GreenBlock> value) noexcept;

        [[nodiscard]] constexpr Optional<const GreenElseClause &> else_clause() const noexcept
        {
            return else_clause_.get();
        }

        void set_else_clause(GreenPtr<GreenElseClause> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::if_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenIfStatement> with_if_keyword(GreenPtr<GreenToken> if_keyword) const;

        [[nodiscard]] GreenPtr<GreenIfStatement> with_open_paren(GreenPtr<GreenToken> open_paren) const;

        [[nodiscard]] GreenPtr<GreenIfStatement> with_condition(GreenPtr<GreenExpression> condition) const;

        [[nodiscard]] GreenPtr<GreenIfStatement> with_close_paren(GreenPtr<GreenToken> close_paren) const;

        [[nodiscard]] GreenPtr<GreenIfStatement> with_block(GreenPtr<GreenBlock> block) const;

        [[nodiscard]] GreenPtr<GreenIfStatement> with_else_clause(GreenPtr<GreenElseClause> else_clause) const;

        [[nodiscard]] GreenPtr<GreenIfStatement> update(GreenPtr<GreenToken> if_keyword,
                                                        GreenPtr<GreenToken> open_paren,
                                                        GreenPtr<GreenExpression> condition,
                                                        GreenPtr<GreenToken> close_paren,
                                                        GreenPtr<GreenBlock> block,
                                                        GreenPtr<GreenElseClause> else_clause) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> if_keyword_;
        GreenPtr<GreenToken> open_paren_;
        GreenPtr<GreenExpression> condition_;
        GreenPtr<GreenToken> close_paren_;
        GreenPtr<GreenBlock> block_;
        GreenPtr<GreenElseClause> else_clause_;
    };

    template <>
    struct GreenNodeTraits<GreenIfStatement>
    {
        static constexpr std::size_t slot_count = 6;

        using ChildTypes = std::tuple<GreenToken, GreenToken, GreenExpression, GreenToken, GreenBlock, GreenElseClause>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenIfStatement &node)
        {
            if constexpr (N == 0)
            {
                return node.if_keyword();
            }
            else if constexpr (N == 1)
            {
                return node.open_paren();
            }
            else if constexpr (N == 2)
            {
                return node.condition();
            }
            else if constexpr (N == 3)
            {
                return node.close_paren();
            }
            else if constexpr (N == 4)
            {
                return node.block();
            }
            else
            {
                static_assert(N == 5);
                return node.else_clause();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenIfStatement>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenIfStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_if_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_open_paren(std::forward<Arg>(value));
            }
            else if constexpr (N == 2)
            {
                node.set_condition(std::forward<Arg>(value));
            }
            else if constexpr (N == 3)
            {
                node.set_close_paren(std::forward<Arg>(value));
            }
            else if constexpr (N == 4)
            {
                node.set_block(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 5);
                node.set_else_clause(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenIfStatement>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenIfStatement> with(const GreenIfStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_if_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_open_paren(std::forward<Arg>(value));
            }
            else if constexpr (N == 2)
            {
                return node.with_condition(std::forward<Arg>(value));
            }
            else if constexpr (N == 3)
            {
                return node.with_close_paren(std::forward<Arg>(value));
            }
            else if constexpr (N == 4)
            {
                return node.with_block(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 5);
                return node.with_else_clause(std::forward<Arg>(value));
            }
        }
    };

    class GreenWhileStatement final : public GreenStatement
    {
      public:
        GreenWhileStatement(GreenPtr<GreenToken> while_keyword,
                            GreenPtr<GreenToken> open_paren,
                            GreenPtr<GreenExpression> condition,
                            GreenPtr<GreenToken> close_paren,
                            GreenPtr<GreenBlock> block,
                            DiagnosticInfoList diagnostics = {});

        ~GreenWhileStatement() override;

        [[nodiscard]] constexpr const GreenToken &while_keyword() const noexcept
        {
            return *while_keyword_;
        }

        void set_while_keyword(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &open_paren() const noexcept
        {
            return *open_paren_;
        }

        void set_open_paren(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenExpression &condition() const noexcept
        {
            return *condition_;
        }

        void set_condition(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &close_paren() const noexcept
        {
            return *close_paren_;
        }

        void set_close_paren(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenBlock &block() const noexcept
        {
            return *block_;
        }

        void set_block(GreenPtr<GreenBlock> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::while_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenWhileStatement> with_while_keyword(GreenPtr<GreenToken> while_keyword) const;

        [[nodiscard]] GreenPtr<GreenWhileStatement> with_open_paren(GreenPtr<GreenToken> open_paren) const;

        [[nodiscard]] GreenPtr<GreenWhileStatement> with_condition(GreenPtr<GreenExpression> condition) const;

        [[nodiscard]] GreenPtr<GreenWhileStatement> with_close_paren(GreenPtr<GreenToken> close_paren) const;

        [[nodiscard]] GreenPtr<GreenWhileStatement> with_block(GreenPtr<GreenBlock> block) const;

        [[nodiscard]] GreenPtr<GreenWhileStatement> update(GreenPtr<GreenToken> while_keyword,
                                                           GreenPtr<GreenToken> open_paren,
                                                           GreenPtr<GreenExpression> condition,
                                                           GreenPtr<GreenToken> close_paren,
                                                           GreenPtr<GreenBlock> block) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> while_keyword_;
        GreenPtr<GreenToken> open_paren_;
        GreenPtr<GreenExpression> condition_;
        GreenPtr<GreenToken> close_paren_;
        GreenPtr<GreenBlock> block_;
    };

    template <>
    struct GreenNodeTraits<GreenWhileStatement>
    {
        static constexpr std::size_t slot_count = 5;

        using ChildTypes = std::tuple<GreenToken, GreenToken, GreenExpression, GreenToken, GreenBlock>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenWhileStatement &node)
        {
            if constexpr (N == 0)
            {
                return node.while_keyword();
            }
            else if constexpr (N == 1)
            {
                return node.open_paren();
            }
            else if constexpr (N == 2)
            {
                return node.condition();
            }
            else if constexpr (N == 3)
            {
                return node.close_paren();
            }
            else
            {
                static_assert(N == 4);
                return node.block();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenWhileStatement>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenWhileStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_while_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_open_paren(std::forward<Arg>(value));
            }
            else if constexpr (N == 2)
            {
                node.set_condition(std::forward<Arg>(value));
            }
            else if constexpr (N == 3)
            {
                node.set_close_paren(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 4);
                node.set_block(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenWhileStatement>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenWhileStatement> with(const GreenWhileStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_while_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_open_paren(std::forward<Arg>(value));
            }
            else if constexpr (N == 2)
            {
                return node.with_condition(std::forward<Arg>(value));
            }
            else if constexpr (N == 3)
            {
                return node.with_close_paren(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 4);
                return node.with_block(std::forward<Arg>(value));
            }
        }
    };

    class GreenLoopStatement final : public GreenStatement
    {
      public:
        GreenLoopStatement(GreenPtr<GreenToken> loop_keyword,
                           GreenPtr<GreenBlock> block,
                           DiagnosticInfoList diagnostics = {});

        ~GreenLoopStatement() override;

        [[nodiscard]] constexpr const GreenToken &loop_keyword() const noexcept
        {
            return *loop_keyword_;
        }

        void set_loop_keyword(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenBlock &block() const noexcept
        {
            return *block_;
        }

        void set_block(GreenPtr<GreenBlock> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::loop_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenLoopStatement> with_loop_keyword(GreenPtr<GreenToken> loop_keyword) const;

        [[nodiscard]] GreenPtr<GreenLoopStatement> with_block(GreenPtr<GreenBlock> block) const;

        [[nodiscard]] GreenPtr<GreenLoopStatement> update(GreenPtr<GreenToken> loop_keyword,
                                                          GreenPtr<GreenBlock> block) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> loop_keyword_;
        GreenPtr<GreenBlock> block_;
    };

    template <>
    struct GreenNodeTraits<GreenLoopStatement>
    {
        static constexpr std::size_t slot_count = 2;

        using ChildTypes = std::tuple<GreenToken, GreenBlock>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenLoopStatement &node)
        {
            if constexpr (N == 0)
            {
                return node.loop_keyword();
            }
            else
            {
                static_assert(N == 1);
                return node.block();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenLoopStatement>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenLoopStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_loop_keyword(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                node.set_block(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenLoopStatement>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenLoopStatement> with(const GreenLoopStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_loop_keyword(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                return node.with_block(std::forward<Arg>(value));
            }
        }
    };

    class GreenForStatement final : public GreenStatement
    {
      public:
        GreenForStatement(GreenPtr<GreenToken> for_keyword,
                          GreenPtr<GreenToken> open_paren,
                          GreenPtr<GreenVariableDeclarationStatement> declaration,
                          GreenSeparatedList<GreenExpression> initializers,
                          GreenPtr<GreenToken> first_semicolon,
                          GreenPtr<GreenExpression> condition,
                          GreenPtr<GreenToken> second_semicolon,
                          GreenSeparatedList<GreenExpression> incrementors,
                          GreenPtr<GreenToken> close_paren,
                          GreenPtr<GreenBlock> block,
                          DiagnosticInfoList diagnostics = {});

        ~GreenForStatement() override;

        [[nodiscard]] constexpr const GreenToken &for_keyword() const noexcept
        {
            return *for_keyword_;
        }

        void set_for_keyword(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &open_paren() const noexcept
        {
            return *open_paren_;
        }

        void set_open_paren(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr Optional<const GreenVariableDeclarationStatement &> declaration() const noexcept
        {
            return declaration_.get();
        }

        void set_declaration(GreenPtr<GreenVariableDeclarationStatement> value) noexcept;

        [[nodiscard]] constexpr const GreenSeparatedList<GreenExpression> &initializers() const noexcept
        {
            return initializers_;
        }

        void set_initializers(GreenSeparatedList<GreenExpression> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &first_semicolon() const noexcept
        {
            return *first_semicolon_;
        }

        void set_first_semicolon(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr Optional<const GreenExpression &> condition() const noexcept
        {
            return condition_.get();
        }

        void set_condition(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &second_semicolon() const noexcept
        {
            return *second_semicolon_;
        }

        void set_second_semicolon(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenSeparatedList<GreenExpression> &incrementors() const noexcept
        {
            return incrementors_;
        }

        void set_incrementors(GreenSeparatedList<GreenExpression> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &close_paren() const noexcept
        {
            return *close_paren_;
        }

        void set_close_paren(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenBlock &block() const noexcept
        {
            return *block_;
        }

        void set_block(GreenPtr<GreenBlock> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::for_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenForStatement> with_for_keyword(GreenPtr<GreenToken> for_keyword) const;

        [[nodiscard]] GreenPtr<GreenForStatement> with_open_paren(GreenPtr<GreenToken> open_paren) const;

        [[nodiscard]] GreenPtr<GreenForStatement> with_declaration(
            GreenPtr<GreenVariableDeclarationStatement> declaration) const;

        [[nodiscard]] GreenPtr<GreenForStatement> with_initializers(
            GreenSeparatedList<GreenExpression> initializers) const;

        [[nodiscard]] GreenPtr<GreenForStatement> with_first_semicolon(GreenPtr<GreenToken> first_semicolon) const;

        [[nodiscard]] GreenPtr<GreenForStatement> with_condition(GreenPtr<GreenExpression> condition) const;

        [[nodiscard]] GreenPtr<GreenForStatement> with_second_semicolon(GreenPtr<GreenToken> second_semicolon) const;

        [[nodiscard]] GreenPtr<GreenForStatement> with_incrementors(
            GreenSeparatedList<GreenExpression> incrementors) const;

        [[nodiscard]] GreenPtr<GreenForStatement> with_close_paren(GreenPtr<GreenToken> close_paren) const;

        [[nodiscard]] GreenPtr<GreenForStatement> with_block(GreenPtr<GreenBlock> block) const;

        [[nodiscard]] GreenPtr<GreenForStatement> update(GreenPtr<GreenToken> for_keyword,
                                                         GreenPtr<GreenToken> open_paren,
                                                         GreenPtr<GreenVariableDeclarationStatement> declaration,
                                                         GreenSeparatedList<GreenExpression> initializers,
                                                         GreenPtr<GreenToken> first_semicolon,
                                                         GreenPtr<GreenExpression> condition,
                                                         GreenPtr<GreenToken> second_semicolon,
                                                         GreenSeparatedList<GreenExpression> incrementors,
                                                         GreenPtr<GreenToken> close_paren,
                                                         GreenPtr<GreenBlock> block) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> for_keyword_;
        GreenPtr<GreenToken> open_paren_;
        GreenPtr<GreenVariableDeclarationStatement> declaration_;
        GreenSeparatedList<GreenExpression> initializers_;
        GreenPtr<GreenToken> first_semicolon_;
        GreenPtr<GreenExpression> condition_;
        GreenPtr<GreenToken> second_semicolon_;
        GreenSeparatedList<GreenExpression> incrementors_;
        GreenPtr<GreenToken> close_paren_;
        GreenPtr<GreenBlock> block_;
    };

    template <>
    struct GreenNodeTraits<GreenForStatement>
    {
        static constexpr std::size_t slot_count = 10;

        using ChildTypes = std::tuple<GreenToken,
                                      GreenToken,
                                      GreenVariableDeclarationStatement,
                                      GreenSeparatedList<GreenExpression>,
                                      GreenToken,
                                      GreenExpression,
                                      GreenToken,
                                      GreenSeparatedList<GreenExpression>,
                                      GreenToken,
                                      GreenBlock>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenForStatement &node)
        {
            if constexpr (N == 0)
            {
                return node.for_keyword();
            }
            else if constexpr (N == 1)
            {
                return node.open_paren();
            }
            else if constexpr (N == 2)
            {
                return node.declaration();
            }
            else if constexpr (N == 3)
            {
                return node.initializers();
            }
            else if constexpr (N == 4)
            {
                return node.first_semicolon();
            }
            else if constexpr (N == 5)
            {
                return node.condition();
            }
            else if constexpr (N == 6)
            {
                return node.second_semicolon();
            }
            else if constexpr (N == 7)
            {
                return node.incrementors();
            }
            else if constexpr (N == 8)
            {
                return node.close_paren();
            }
            else
            {
                static_assert(N == 9);
                return node.block();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenForStatement>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenForStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_for_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_open_paren(std::forward<Arg>(value));
            }
            else if constexpr (N == 2)
            {
                node.set_declaration(std::forward<Arg>(value));
            }
            else if constexpr (N == 3)
            {
                node.set_initializers(std::forward<Arg>(value));
            }
            else if constexpr (N == 4)
            {
                node.set_first_semicolon(std::forward<Arg>(value));
            }
            else if constexpr (N == 5)
            {
                node.set_condition(std::forward<Arg>(value));
            }
            else if constexpr (N == 6)
            {
                node.set_second_semicolon(std::forward<Arg>(value));
            }
            else if constexpr (N == 7)
            {
                node.set_incrementors(std::forward<Arg>(value));
            }
            else if constexpr (N == 8)
            {
                node.set_close_paren(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 9);
                node.set_block(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenForStatement>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenForStatement> with(const GreenForStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_for_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_open_paren(std::forward<Arg>(value));
            }
            else if constexpr (N == 2)
            {
                return node.with_declaration(std::forward<Arg>(value));
            }
            else if constexpr (N == 3)
            {
                return node.with_initializers(std::forward<Arg>(value));
            }
            else if constexpr (N == 4)
            {
                return node.with_first_semicolon(std::forward<Arg>(value));
            }
            else if constexpr (N == 5)
            {
                return node.with_condition(std::forward<Arg>(value));
            }
            else if constexpr (N == 6)
            {
                return node.with_second_semicolon(std::forward<Arg>(value));
            }
            else if constexpr (N == 7)
            {
                return node.with_incrementors(std::forward<Arg>(value));
            }
            else if constexpr (N == 8)
            {
                return node.with_close_paren(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 9);
                return node.with_block(std::forward<Arg>(value));
            }
        }
    };

    class GreenBreakStatement final : public GreenStatement
    {
      public:
        GreenBreakStatement(GreenPtr<GreenToken> keyword,
                            GreenPtr<GreenToken> semicolon,
                            DiagnosticInfoList diagnostics = {});

        ~GreenBreakStatement() override;

        [[nodiscard]] constexpr const GreenToken &keyword() const noexcept
        {
            return *keyword_;
        }

        void set_keyword(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &semicolon() const noexcept
        {
            return *semicolon_;
        }

        void set_semicolon(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::break_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenBreakStatement> with_keyword(GreenPtr<GreenToken> keyword) const;

        [[nodiscard]] GreenPtr<GreenBreakStatement> with_semicolon(GreenPtr<GreenToken> semicolon) const;

        [[nodiscard]] GreenPtr<GreenBreakStatement> update(GreenPtr<GreenToken> keyword,
                                                           GreenPtr<GreenToken> semicolon) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> keyword_;
        GreenPtr<GreenToken> semicolon_;
    };

    template <>
    struct GreenNodeTraits<GreenBreakStatement>
    {
        static constexpr std::size_t slot_count = 2;

        using ChildTypes = std::tuple<GreenToken, GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenBreakStatement &node)
        {
            if constexpr (N == 0)
            {
                return node.keyword();
            }
            else
            {
                static_assert(N == 1);
                return node.semicolon();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenBreakStatement>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenBreakStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_keyword(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                node.set_semicolon(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenBreakStatement>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenBreakStatement> with(const GreenBreakStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_keyword(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                return node.with_semicolon(std::forward<Arg>(value));
            }
        }
    };

    class GreenContinueStatement final : public GreenStatement
    {
      public:
        GreenContinueStatement(GreenPtr<GreenToken> keyword,
                               GreenPtr<GreenToken> semicolon,
                               DiagnosticInfoList diagnostics = {});

        ~GreenContinueStatement() override;

        [[nodiscard]] constexpr const GreenToken &keyword() const noexcept
        {
            return *keyword_;
        }

        void set_keyword(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &semicolon() const noexcept
        {
            return *semicolon_;
        }

        void set_semicolon(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::continue_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenContinueStatement> with_keyword(GreenPtr<GreenToken> keyword) const;

        [[nodiscard]] GreenPtr<GreenContinueStatement> with_semicolon(GreenPtr<GreenToken> semicolon) const;

        [[nodiscard]] GreenPtr<GreenContinueStatement> update(GreenPtr<GreenToken> keyword,
                                                              GreenPtr<GreenToken> semicolon) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> keyword_;
        GreenPtr<GreenToken> semicolon_;
    };

    template <>
    struct GreenNodeTraits<GreenContinueStatement>
    {
        static constexpr std::size_t slot_count = 2;

        using ChildTypes = std::tuple<GreenToken, GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenContinueStatement &node)
        {
            if constexpr (N == 0)
            {
                return node.keyword();
            }
            else
            {
                static_assert(N == 1);
                return node.semicolon();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenContinueStatement>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenContinueStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_keyword(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                node.set_semicolon(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenContinueStatement>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenContinueStatement> with(const GreenContinueStatement &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_keyword(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                return node.with_semicolon(std::forward<Arg>(value));
            }
        }
    };
} // namespace prism
