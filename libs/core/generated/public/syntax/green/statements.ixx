export module prism.core:syntax.green.statements;

import :syntax.green.node;
import :syntax.green.token;
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

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::variable_declaration_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] GreenPtr<GreenVariableDeclarationStatement> with_declaration(
            GreenPtr<GreenVariableDeclaration> declaration) const;

        [[nodiscard]] GreenPtr<GreenVariableDeclarationStatement> update(
            GreenPtr<GreenVariableDeclaration> declaration) const;

      private:
        GreenPtr<GreenVariableDeclaration> declaration_;
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

        [[nodiscard]] constexpr GreenSyntaxList<GreenStatement> statements() const noexcept
        {
            return statements_;
        }

        [[nodiscard]] constexpr const GreenToken &close_brace() const noexcept
        {
            return *close_brace_;
        }

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::block;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] GreenPtr<GreenBlock> with_open_brace(GreenPtr<GreenToken> open_brace) const;

        [[nodiscard]] GreenPtr<GreenBlock> with_statements(GreenSyntaxList<GreenStatement> statements) const;

        [[nodiscard]] GreenPtr<GreenBlock> with_close_brace(GreenPtr<GreenToken> close_brace) const;

        [[nodiscard]] GreenPtr<GreenBlock> update(GreenPtr<GreenToken> open_brace,
                                                  GreenSyntaxList<GreenStatement> statements,
                                                  GreenPtr<GreenToken> close_brace) const;

      private:
        GreenPtr<GreenToken> open_brace_;
        GreenSyntaxList<GreenStatement> statements_;
        GreenPtr<GreenToken> close_brace_;
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

        [[nodiscard]] constexpr Optional<const GreenExpression &> expression() const noexcept
        {
            return expression_.get();
        }

        [[nodiscard]] constexpr const GreenToken &semicolon() const noexcept
        {
            return *semicolon_;
        }

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::return_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] GreenPtr<GreenReturnStatement> with_return_keyword(GreenPtr<GreenToken> return_keyword) const;

        [[nodiscard]] GreenPtr<GreenReturnStatement> with_expression(GreenPtr<GreenExpression> expression) const;

        [[nodiscard]] GreenPtr<GreenReturnStatement> with_semicolon(GreenPtr<GreenToken> semicolon) const;

        [[nodiscard]] GreenPtr<GreenReturnStatement> update(GreenPtr<GreenToken> return_keyword,
                                                            GreenPtr<GreenExpression> expression,
                                                            GreenPtr<GreenToken> semicolon) const;

      private:
        GreenPtr<GreenToken> return_keyword_;
        GreenPtr<GreenExpression> expression_;
        GreenPtr<GreenToken> semicolon_;
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

        [[nodiscard]] constexpr const GreenToken &semicolon() const noexcept
        {
            return *semicolon_;
        }

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::expression_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] GreenPtr<GreenExpressionStatement> with_expression(GreenPtr<GreenExpression> expression) const;

        [[nodiscard]] GreenPtr<GreenExpressionStatement> with_semicolon(GreenPtr<GreenToken> semicolon) const;

        [[nodiscard]] GreenPtr<GreenExpressionStatement> update(GreenPtr<GreenExpression> expression,
                                                                GreenPtr<GreenToken> semicolon) const;

      private:
        GreenPtr<GreenExpression> expression_;
        GreenPtr<GreenToken> semicolon_;
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

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::empty_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] GreenPtr<GreenEmptyStatement> with_semicolon(GreenPtr<GreenToken> semicolon) const;

        [[nodiscard]] GreenPtr<GreenEmptyStatement> update(GreenPtr<GreenToken> semicolon) const;

      private:
        GreenPtr<GreenToken> semicolon_;
    };
} // namespace prism
