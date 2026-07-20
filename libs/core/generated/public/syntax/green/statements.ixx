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
        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::variable_declaration_statement || node.kind() == SyntaxKind::block ||
                   node.kind() == SyntaxKind::return_statement || node.kind() == SyntaxKind::expression_statement ||
                   node.kind() == SyntaxKind::empty_statement;
        }
    };

    class GreenVariableDeclarationStatement final : public GreenStatement
    {
      public:
        explicit GreenVariableDeclarationStatement(RefCountPtr<const GreenVariableDeclaration> declaration,
                                                   DiagnosticInfoList diagnostics = {});

        ~GreenVariableDeclarationStatement() override;

        [[nodiscard]] constexpr const GreenVariableDeclaration &declaration() const noexcept
        {
            return *declaration_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::variable_declaration_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] RefCountPtr<const GreenVariableDeclarationStatement> with_declaration(
            RefCountPtr<const GreenVariableDeclaration> declaration) const;

        [[nodiscard]] RefCountPtr<const GreenVariableDeclarationStatement> update(
            RefCountPtr<const GreenVariableDeclaration> declaration) const;

      private:
        RefCountPtr<const GreenVariableDeclaration> declaration_;
    };

    class GreenBlock final : public GreenStatement
    {
      public:
        GreenBlock(RefCountPtr<const GreenToken> open_brace,
                   GreenSyntaxList<GreenStatement> statements,
                   RefCountPtr<const GreenToken> close_brace,
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

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::block;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] RefCountPtr<const GreenBlock> with_open_brace(RefCountPtr<const GreenToken> open_brace) const;

        [[nodiscard]] RefCountPtr<const GreenBlock> with_statements(GreenSyntaxList<GreenStatement> statements) const;

        [[nodiscard]] RefCountPtr<const GreenBlock> with_close_brace(RefCountPtr<const GreenToken> close_brace) const;

        [[nodiscard]] RefCountPtr<const GreenBlock> update(RefCountPtr<const GreenToken> open_brace,
                                                           GreenSyntaxList<GreenStatement> statements,
                                                           RefCountPtr<const GreenToken> close_brace) const;

      private:
        RefCountPtr<const GreenToken> open_brace_;
        GreenSyntaxList<GreenStatement> statements_;
        RefCountPtr<const GreenToken> close_brace_;
    };

    class GreenReturnStatement final : public GreenStatement
    {
      public:
        GreenReturnStatement(RefCountPtr<const GreenToken> return_keyword,
                             RefCountPtr<const GreenExpression> expression,
                             RefCountPtr<const GreenToken> semicolon,
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

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::return_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] RefCountPtr<const GreenReturnStatement> with_return_keyword(
            RefCountPtr<const GreenToken> return_keyword) const;

        [[nodiscard]] RefCountPtr<const GreenReturnStatement> with_expression(
            RefCountPtr<const GreenExpression> expression) const;

        [[nodiscard]] RefCountPtr<const GreenReturnStatement> with_semicolon(
            RefCountPtr<const GreenToken> semicolon) const;

        [[nodiscard]] RefCountPtr<const GreenReturnStatement> update(RefCountPtr<const GreenToken> return_keyword,
                                                                     RefCountPtr<const GreenExpression> expression,
                                                                     RefCountPtr<const GreenToken> semicolon) const;

      private:
        RefCountPtr<const GreenToken> return_keyword_;
        RefCountPtr<const GreenExpression> expression_;
        RefCountPtr<const GreenToken> semicolon_;
    };

    class GreenExpressionStatement final : public GreenStatement
    {
      public:
        GreenExpressionStatement(RefCountPtr<const GreenExpression> expression,
                                 RefCountPtr<const GreenToken> semicolon,
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

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::expression_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] RefCountPtr<const GreenExpressionStatement> with_expression(
            RefCountPtr<const GreenExpression> expression) const;

        [[nodiscard]] RefCountPtr<const GreenExpressionStatement> with_semicolon(
            RefCountPtr<const GreenToken> semicolon) const;

        [[nodiscard]] RefCountPtr<const GreenExpressionStatement> update(RefCountPtr<const GreenExpression> expression,
                                                                         RefCountPtr<const GreenToken> semicolon) const;

      private:
        RefCountPtr<const GreenExpression> expression_;
        RefCountPtr<const GreenToken> semicolon_;
    };

    class GreenEmptyStatement final : public GreenStatement
    {
      public:
        explicit GreenEmptyStatement(RefCountPtr<const GreenToken> semicolon, DiagnosticInfoList diagnostics = {});

        ~GreenEmptyStatement() override;

        [[nodiscard]] constexpr const GreenToken &semicolon() const noexcept
        {
            return *semicolon_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::empty_statement;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] RefCountPtr<const GreenEmptyStatement> with_semicolon(
            RefCountPtr<const GreenToken> semicolon) const;

        [[nodiscard]] RefCountPtr<const GreenEmptyStatement> update(RefCountPtr<const GreenToken> semicolon) const;

      private:
        RefCountPtr<const GreenToken> semicolon_;
    };
} // namespace prism
