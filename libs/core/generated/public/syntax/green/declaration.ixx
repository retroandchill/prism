export module prism.core:syntax.green.declaration;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.separated_list;

namespace prism
{
    class GreenArgument;
    class GreenArgumentList;
    class GreenBlock;
    class GreenExpression;
    class GreenExpressionBody;
    class GreenInitializer;
    class GreenNamedParameter;
    class GreenParameter;
    class GreenParameterList;
    class GreenStatement;
    class GreenType;
    class GreenTypeSpecifier;
    class GreenVariableDeclaration;

    class GreenDeclaration : public GreenNode
    {
      protected:
        explicit constexpr GreenDeclaration(const SyntaxKind kind, DiagnosticInfoList diagnostics = {})
            : GreenNode{kind, std::move(diagnostics)}
        {
        }

      public:
        [[nodiscard]] virtual GreenSyntaxList<GreenToken> modifiers() const noexcept = 0;

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::variable_declaration || node.kind() == SyntaxKind::function_declaration;
        }
    };

    class GreenVariableDeclaration final : public GreenDeclaration
    {
      public:
        GreenVariableDeclaration(GreenSyntaxList<GreenToken> modifiers,
                                 GreenPtr<GreenToken> var_keyword,
                                 GreenPtr<GreenToken> mut_keyword,
                                 GreenPtr<GreenToken> identifier,
                                 GreenPtr<GreenTypeSpecifier> type,
                                 GreenPtr<GreenInitializer> initializer,
                                 GreenPtr<GreenToken> semicolon,
                                 DiagnosticInfoList diagnostics = {});

        ~GreenVariableDeclaration() override;

        [[nodiscard]] constexpr GreenSyntaxList<GreenToken> modifiers() const noexcept override
        {
            return modifiers_;
        }

        [[nodiscard]] constexpr const GreenToken &var_keyword() const noexcept
        {
            return *var_keyword_;
        }

        [[nodiscard]] constexpr Optional<const GreenToken &> mut_keyword() const noexcept
        {
            return mut_keyword_.get();
        }

        [[nodiscard]] constexpr const GreenToken &identifier() const noexcept
        {
            return *identifier_;
        }

        [[nodiscard]] constexpr const GreenTypeSpecifier &type() const noexcept
        {
            return *type_;
        }

        [[nodiscard]] constexpr const GreenInitializer &initializer() const noexcept
        {
            return *initializer_;
        }

        [[nodiscard]] constexpr const GreenToken &semicolon() const noexcept
        {
            return *semicolon_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::variable_declaration;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      private:
        GreenSyntaxList<GreenToken> modifiers_;
        GreenPtr<GreenToken> var_keyword_;
        GreenPtr<GreenToken> mut_keyword_;
        GreenPtr<GreenToken> identifier_;
        GreenPtr<GreenTypeSpecifier> type_;
        GreenPtr<GreenInitializer> initializer_;
        GreenPtr<GreenToken> semicolon_;
    };

    class GreenFunctionDeclaration final : public GreenDeclaration
    {
      public:
        GreenFunctionDeclaration(GreenSyntaxList<GreenToken> modifiers,
                                 GreenPtr<GreenToken> func_keyword,
                                 GreenPtr<GreenToken> identifier,
                                 GreenPtr<GreenParameterList> parameters,
                                 GreenPtr<GreenTypeSpecifier> return_type,
                                 GreenPtr<GreenBlock> body,
                                 GreenPtr<GreenExpressionBody> expression_body,
                                 GreenPtr<GreenToken> semicolon,
                                 DiagnosticInfoList diagnostics = {});

        ~GreenFunctionDeclaration() override;

        [[nodiscard]] constexpr GreenSyntaxList<GreenToken> modifiers() const noexcept override
        {
            return modifiers_;
        }

        [[nodiscard]] constexpr const GreenToken &func_keyword() const noexcept
        {
            return *func_keyword_;
        }

        [[nodiscard]] constexpr const GreenToken &identifier() const noexcept
        {
            return *identifier_;
        }

        [[nodiscard]] constexpr const GreenParameterList &parameters() const noexcept
        {
            return *parameters_;
        }

        [[nodiscard]] constexpr Optional<const GreenTypeSpecifier &> return_type() const noexcept
        {
            return return_type_.get();
        }

        [[nodiscard]] constexpr Optional<const GreenBlock &> body() const noexcept
        {
            return body_.get();
        }

        [[nodiscard]] constexpr Optional<const GreenExpressionBody &> expression_body() const noexcept
        {
            return expression_body_.get();
        }

        [[nodiscard]] constexpr Optional<const GreenToken &> semicolon() const noexcept
        {
            return semicolon_.get();
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::function_declaration;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      private:
        GreenSyntaxList<GreenToken> modifiers_;
        GreenPtr<GreenToken> func_keyword_;
        GreenPtr<GreenToken> identifier_;
        GreenPtr<GreenParameterList> parameters_;
        GreenPtr<GreenTypeSpecifier> return_type_;
        GreenPtr<GreenBlock> body_;
        GreenPtr<GreenExpressionBody> expression_body_;
        GreenPtr<GreenToken> semicolon_;
    };
} // namespace prism
