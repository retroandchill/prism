export module prism.core:syntax.green.declarations;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.separated_list;

namespace prism
{
    class GreenBlock;
    class GreenExpressionBody;
    class GreenInitializer;
    class GreenParameterList;
    class GreenTypeSpecifier;

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

        template <typename Self>
        [[nodiscard]] constexpr RefCountPtr<const std::decay_t<Self>> with_modifiers(
            const Self &self,
            GreenSyntaxList<GreenToken> modifiers)
        {
            return static_pointer_cast<std::decay_t<Self>>(self.with_modifiers_core(std::move(modifiers)));
        }

      protected:
        [[nodiscard]] virtual RefCountPtr<const GreenDeclaration> with_modifiers_core(
            GreenSyntaxList<GreenToken> modifiers) const = 0;
    };

    class GreenVariableDeclaration final : public GreenDeclaration
    {
      public:
        GreenVariableDeclaration(GreenSyntaxList<GreenToken> modifiers,
                                 RefCountPtr<const GreenToken> var_keyword,
                                 RefCountPtr<const GreenToken> mut_keyword,
                                 RefCountPtr<const GreenToken> identifier,
                                 RefCountPtr<const GreenTypeSpecifier> type,
                                 RefCountPtr<const GreenInitializer> initializer,
                                 RefCountPtr<const GreenToken> semicolon,
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

      protected:
        [[nodiscard]] RefCountPtr<const GreenDeclaration> with_modifiers_core(
            GreenSyntaxList<GreenToken> modifiers) const override;

      public:
        [[nodiscard]] RefCountPtr<const GreenVariableDeclaration> with_var_keyword(
            RefCountPtr<const GreenToken> var_keyword) const;

        [[nodiscard]] RefCountPtr<const GreenVariableDeclaration> with_mut_keyword(
            RefCountPtr<const GreenToken> mut_keyword) const;

        [[nodiscard]] RefCountPtr<const GreenVariableDeclaration> with_identifier(
            RefCountPtr<const GreenToken> identifier) const;

        [[nodiscard]] RefCountPtr<const GreenVariableDeclaration> with_type(
            RefCountPtr<const GreenTypeSpecifier> type) const;

        [[nodiscard]] RefCountPtr<const GreenVariableDeclaration> with_initializer(
            RefCountPtr<const GreenInitializer> initializer) const;

        [[nodiscard]] RefCountPtr<const GreenVariableDeclaration> with_semicolon(
            RefCountPtr<const GreenToken> semicolon) const;

        [[nodiscard]] RefCountPtr<const GreenVariableDeclaration> update(
            GreenSyntaxList<GreenToken> modifiers,
            RefCountPtr<const GreenToken> var_keyword,
            RefCountPtr<const GreenToken> mut_keyword,
            RefCountPtr<const GreenToken> identifier,
            RefCountPtr<const GreenTypeSpecifier> type,
            RefCountPtr<const GreenInitializer> initializer,
            RefCountPtr<const GreenToken> semicolon) const;

      private:
        GreenSyntaxList<GreenToken> modifiers_;
        RefCountPtr<const GreenToken> var_keyword_;
        RefCountPtr<const GreenToken> mut_keyword_;
        RefCountPtr<const GreenToken> identifier_;
        RefCountPtr<const GreenTypeSpecifier> type_;
        RefCountPtr<const GreenInitializer> initializer_;
        RefCountPtr<const GreenToken> semicolon_;
    };

    class GreenFunctionDeclaration final : public GreenDeclaration
    {
      public:
        GreenFunctionDeclaration(GreenSyntaxList<GreenToken> modifiers,
                                 RefCountPtr<const GreenToken> func_keyword,
                                 RefCountPtr<const GreenToken> identifier,
                                 RefCountPtr<const GreenParameterList> parameters,
                                 RefCountPtr<const GreenTypeSpecifier> return_type,
                                 RefCountPtr<const GreenBlock> body,
                                 RefCountPtr<const GreenExpressionBody> expression_body,
                                 RefCountPtr<const GreenToken> semicolon,
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

      protected:
        [[nodiscard]] RefCountPtr<const GreenDeclaration> with_modifiers_core(
            GreenSyntaxList<GreenToken> modifiers) const override;

      public:
        [[nodiscard]] RefCountPtr<const GreenFunctionDeclaration> with_func_keyword(
            RefCountPtr<const GreenToken> func_keyword) const;

        [[nodiscard]] RefCountPtr<const GreenFunctionDeclaration> with_identifier(
            RefCountPtr<const GreenToken> identifier) const;

        [[nodiscard]] RefCountPtr<const GreenFunctionDeclaration> with_parameters(
            RefCountPtr<const GreenParameterList> parameters) const;

        [[nodiscard]] RefCountPtr<const GreenFunctionDeclaration> with_return_type(
            RefCountPtr<const GreenTypeSpecifier> return_type) const;

        [[nodiscard]] RefCountPtr<const GreenFunctionDeclaration> with_body(RefCountPtr<const GreenBlock> body) const;

        [[nodiscard]] RefCountPtr<const GreenFunctionDeclaration> with_expression_body(
            RefCountPtr<const GreenExpressionBody> expression_body) const;

        [[nodiscard]] RefCountPtr<const GreenFunctionDeclaration> with_semicolon(
            RefCountPtr<const GreenToken> semicolon) const;

        [[nodiscard]] RefCountPtr<const GreenFunctionDeclaration> update(
            GreenSyntaxList<GreenToken> modifiers,
            RefCountPtr<const GreenToken> func_keyword,
            RefCountPtr<const GreenToken> identifier,
            RefCountPtr<const GreenParameterList> parameters,
            RefCountPtr<const GreenTypeSpecifier> return_type,
            RefCountPtr<const GreenBlock> body,
            RefCountPtr<const GreenExpressionBody> expression_body,
            RefCountPtr<const GreenToken> semicolon) const;

      private:
        GreenSyntaxList<GreenToken> modifiers_;
        RefCountPtr<const GreenToken> func_keyword_;
        RefCountPtr<const GreenToken> identifier_;
        RefCountPtr<const GreenParameterList> parameters_;
        RefCountPtr<const GreenTypeSpecifier> return_type_;
        RefCountPtr<const GreenBlock> body_;
        RefCountPtr<const GreenExpressionBody> expression_body_;
        RefCountPtr<const GreenToken> semicolon_;
    };
} // namespace prism
