export module prism.core:syntax.green.statement;

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
        ~GreenStatement() override;
    };

    class GreenVariableDeclarationStatement final : public GreenStatement
    {
      public:
        GreenVariableDeclarationStatement(SyntaxKind kind,
                                          GreenPtr<GreenVariableDeclaration> declaration,
                                          DiagnosticInfoList diagnostics = {});
        ~GreenVariableDeclarationStatement() override;

        [[nodiscard]] constexpr const GreenVariableDeclaration &declaration() const noexcept
        {
            return *declaration_;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      private:
        GreenPtr<GreenVariableDeclaration> declaration_;
    };

    class GreenBlock final : public GreenStatement
    {
      public:
        GreenBlock(SyntaxKind kind, GreenSyntaxList<GreenStatement> statements, DiagnosticInfoList diagnostics = {});
        ~GreenBlock() override;

        [[nodiscard]] constexpr const GreenSyntaxList<GreenStatement> &statements() const noexcept
        {
            return statements_;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      private:
        GreenSyntaxList<GreenStatement> statements_;
    };

    class GreenReturnStatement final : public GreenStatement
    {
      public:
        GreenReturnStatement(SyntaxKind kind,
                             GreenPtr<GreenToken> return_keyword,
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

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      private:
        GreenPtr<GreenToken> return_keyword_;
        GreenPtr<GreenExpression> expression_;
        GreenPtr<GreenToken> semicolon_;
    };

    class GreenExpressionStatement final : public GreenStatement
    {
      public:
        GreenExpressionStatement(SyntaxKind kind,
                                 GreenPtr<GreenExpression> expression,
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

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      private:
        GreenPtr<GreenExpression> expression_;
        GreenPtr<GreenToken> semicolon_;
    };

    class GreenEmptyStatement final : public GreenStatement
    {
      public:
        GreenEmptyStatement(SyntaxKind kind, GreenPtr<GreenToken> semicolon, DiagnosticInfoList diagnostics = {});
        ~GreenEmptyStatement() override;

        [[nodiscard]] constexpr const GreenToken &semicolon() const noexcept
        {
            return *semicolon_;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      private:
        GreenPtr<GreenToken> semicolon_;
    };

} // namespace prism
