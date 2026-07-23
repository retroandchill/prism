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

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::variable_declaration || node.kind() == SyntaxKind::function_declaration;
        }

        template <typename Self>
        [[nodiscard]] constexpr GreenPtr<std::decay_t<Self>> with_modifiers(const Self &self,
                                                                            GreenSyntaxList<GreenToken> modifiers)
        {
            return static_pointer_cast<std::decay_t<Self>>(self.with_modifiers_core(std::move(modifiers)));
        }

      protected:
        [[nodiscard]] virtual GreenPtr<GreenDeclaration> with_modifiers_core(
            GreenSyntaxList<GreenToken> modifiers) const = 0;
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

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::variable_declaration;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      protected:
        [[nodiscard]] GreenPtr<GreenDeclaration> with_modifiers_core(
            GreenSyntaxList<GreenToken> modifiers) const override;

      public:
        [[nodiscard]] GreenPtr<GreenVariableDeclaration> with_var_keyword(GreenPtr<GreenToken> var_keyword) const;

        [[nodiscard]] GreenPtr<GreenVariableDeclaration> with_mut_keyword(GreenPtr<GreenToken> mut_keyword) const;

        [[nodiscard]] GreenPtr<GreenVariableDeclaration> with_identifier(GreenPtr<GreenToken> identifier) const;

        [[nodiscard]] GreenPtr<GreenVariableDeclaration> with_type(GreenPtr<GreenTypeSpecifier> type) const;

        [[nodiscard]] GreenPtr<GreenVariableDeclaration> with_initializer(GreenPtr<GreenInitializer> initializer) const;

        [[nodiscard]] GreenPtr<GreenVariableDeclaration> with_semicolon(GreenPtr<GreenToken> semicolon) const;

        [[nodiscard]] GreenPtr<GreenVariableDeclaration> update(GreenSyntaxList<GreenToken> modifiers,
                                                                GreenPtr<GreenToken> var_keyword,
                                                                GreenPtr<GreenToken> mut_keyword,
                                                                GreenPtr<GreenToken> identifier,
                                                                GreenPtr<GreenTypeSpecifier> type,
                                                                GreenPtr<GreenInitializer> initializer,
                                                                GreenPtr<GreenToken> semicolon) const;

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

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::function_declaration;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      protected:
        [[nodiscard]] GreenPtr<GreenDeclaration> with_modifiers_core(
            GreenSyntaxList<GreenToken> modifiers) const override;

      public:
        [[nodiscard]] GreenPtr<GreenFunctionDeclaration> with_func_keyword(GreenPtr<GreenToken> func_keyword) const;

        [[nodiscard]] GreenPtr<GreenFunctionDeclaration> with_identifier(GreenPtr<GreenToken> identifier) const;

        [[nodiscard]] GreenPtr<GreenFunctionDeclaration> with_parameters(GreenPtr<GreenParameterList> parameters) const;

        [[nodiscard]] GreenPtr<GreenFunctionDeclaration> with_return_type(
            GreenPtr<GreenTypeSpecifier> return_type) const;

        [[nodiscard]] GreenPtr<GreenFunctionDeclaration> with_body(GreenPtr<GreenBlock> body) const;

        [[nodiscard]] GreenPtr<GreenFunctionDeclaration> with_expression_body(
            GreenPtr<GreenExpressionBody> expression_body) const;

        [[nodiscard]] GreenPtr<GreenFunctionDeclaration> with_semicolon(GreenPtr<GreenToken> semicolon) const;

        [[nodiscard]] GreenPtr<GreenFunctionDeclaration> update(GreenSyntaxList<GreenToken> modifiers,
                                                                GreenPtr<GreenToken> func_keyword,
                                                                GreenPtr<GreenToken> identifier,
                                                                GreenPtr<GreenParameterList> parameters,
                                                                GreenPtr<GreenTypeSpecifier> return_type,
                                                                GreenPtr<GreenBlock> body,
                                                                GreenPtr<GreenExpressionBody> expression_body,
                                                                GreenPtr<GreenToken> semicolon) const;

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
