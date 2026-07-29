export module prism.core:syntax.green.declarations;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.trivia;
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
        [[nodiscard]] virtual const GreenSyntaxList<GreenToken> &modifiers() const noexcept = 0;
        virtual void set_modifiers(GreenSyntaxList<GreenToken> value) noexcept = 0;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::incomplete_declaration ||
                   node.kind() == SyntaxKind::variable_declaration || node.kind() == SyntaxKind::function_declaration;
        }

        template <typename Self>
        [[nodiscard]] constexpr GreenPtr<std::decay_t<Self>> with_modifiers(this const Self &self,
                                                                            GreenSyntaxList<GreenToken> modifiers)
        {
            return static_pointer_cast<const std::decay_t<Self>>(self.with_modifiers_core(std::move(modifiers)));
        }

        [[nodiscard]] virtual GreenPtr<GreenDeclaration> with_modifiers_core(
            GreenSyntaxList<GreenToken> modifiers) const = 0;
    };

    class GreenIncompleteDeclaration final : public GreenDeclaration
    {
      public:
        explicit GreenIncompleteDeclaration(GreenSyntaxList<GreenToken> modifiers, DiagnosticInfoList diagnostics = {});

        ~GreenIncompleteDeclaration() override;

        [[nodiscard]] constexpr const GreenSyntaxList<GreenToken> &modifiers() const noexcept override
        {
            return modifiers_;
        }

        void set_modifiers(GreenSyntaxList<GreenToken> value) noexcept override;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::incomplete_declaration;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenDeclaration> with_modifiers_core(
            GreenSyntaxList<GreenToken> modifiers) const override;

        [[nodiscard]] GreenPtr<GreenIncompleteDeclaration> update(GreenSyntaxList<GreenToken> modifiers) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenSyntaxList<GreenToken> modifiers_;
    };

    template <>
    struct GreenNodeTraits<GreenIncompleteDeclaration>
    {
        static constexpr std::size_t slot_count = 1;

        using ChildTypes = std::tuple<GreenSyntaxList<GreenToken>>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenIncompleteDeclaration &node)
        {
            {
                static_assert(N == 0);
                return node.modifiers();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenIncompleteDeclaration>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenIncompleteDeclaration &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                node.set_modifiers(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenIncompleteDeclaration>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenIncompleteDeclaration> with(const GreenIncompleteDeclaration &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                return node.with_modifiers(std::forward<Arg>(value));
            }
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

        [[nodiscard]] constexpr const GreenSyntaxList<GreenToken> &modifiers() const noexcept override
        {
            return modifiers_;
        }

        void set_modifiers(GreenSyntaxList<GreenToken> value) noexcept override;

        [[nodiscard]] constexpr const GreenToken &var_keyword() const noexcept
        {
            return *var_keyword_;
        }

        void set_var_keyword(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr Optional<const GreenToken &> mut_keyword() const noexcept
        {
            return mut_keyword_.get();
        }

        void set_mut_keyword(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &identifier() const noexcept
        {
            return *identifier_;
        }

        void set_identifier(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr Optional<const GreenTypeSpecifier &> type() const noexcept
        {
            return type_.get();
        }

        void set_type(GreenPtr<GreenTypeSpecifier> value) noexcept;

        [[nodiscard]] constexpr Optional<const GreenInitializer &> initializer() const noexcept
        {
            return initializer_.get();
        }

        void set_initializer(GreenPtr<GreenInitializer> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &semicolon() const noexcept
        {
            return *semicolon_;
        }

        void set_semicolon(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::variable_declaration;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenDeclaration> with_modifiers_core(
            GreenSyntaxList<GreenToken> modifiers) const override;

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
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenSyntaxList<GreenToken> modifiers_;
        GreenPtr<GreenToken> var_keyword_;
        GreenPtr<GreenToken> mut_keyword_;
        GreenPtr<GreenToken> identifier_;
        GreenPtr<GreenTypeSpecifier> type_;
        GreenPtr<GreenInitializer> initializer_;
        GreenPtr<GreenToken> semicolon_;
    };

    template <>
    struct GreenNodeTraits<GreenVariableDeclaration>
    {
        static constexpr std::size_t slot_count = 7;

        using ChildTypes = std::tuple<GreenSyntaxList<GreenToken>,
                                      GreenToken,
                                      GreenToken,
                                      GreenToken,
                                      GreenTypeSpecifier,
                                      GreenInitializer,
                                      GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenVariableDeclaration &node)
        {
            if constexpr (N == 0)
            {
                return node.modifiers();
            }
            else if constexpr (N == 1)
            {
                return node.var_keyword();
            }
            else if constexpr (N == 2)
            {
                return node.mut_keyword();
            }
            else if constexpr (N == 3)
            {
                return node.identifier();
            }
            else if constexpr (N == 4)
            {
                return node.type();
            }
            else if constexpr (N == 5)
            {
                return node.initializer();
            }
            else
            {
                static_assert(N == 6);
                return node.semicolon();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenVariableDeclaration>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenVariableDeclaration &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_modifiers(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_var_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 2)
            {
                node.set_mut_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 3)
            {
                node.set_identifier(std::forward<Arg>(value));
            }
            else if constexpr (N == 4)
            {
                node.set_type(std::forward<Arg>(value));
            }
            else if constexpr (N == 5)
            {
                node.set_initializer(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 6);
                node.set_semicolon(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenVariableDeclaration>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenVariableDeclaration> with(const GreenVariableDeclaration &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_modifiers(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_var_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 2)
            {
                return node.with_mut_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 3)
            {
                return node.with_identifier(std::forward<Arg>(value));
            }
            else if constexpr (N == 4)
            {
                return node.with_type(std::forward<Arg>(value));
            }
            else if constexpr (N == 5)
            {
                return node.with_initializer(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 6);
                return node.with_semicolon(std::forward<Arg>(value));
            }
        }
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

        [[nodiscard]] constexpr const GreenSyntaxList<GreenToken> &modifiers() const noexcept override
        {
            return modifiers_;
        }

        void set_modifiers(GreenSyntaxList<GreenToken> value) noexcept override;

        [[nodiscard]] constexpr const GreenToken &func_keyword() const noexcept
        {
            return *func_keyword_;
        }

        void set_func_keyword(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &identifier() const noexcept
        {
            return *identifier_;
        }

        void set_identifier(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenParameterList &parameters() const noexcept
        {
            return *parameters_;
        }

        void set_parameters(GreenPtr<GreenParameterList> value) noexcept;

        [[nodiscard]] constexpr Optional<const GreenTypeSpecifier &> return_type() const noexcept
        {
            return return_type_.get();
        }

        void set_return_type(GreenPtr<GreenTypeSpecifier> value) noexcept;

        [[nodiscard]] constexpr Optional<const GreenBlock &> body() const noexcept
        {
            return body_.get();
        }

        void set_body(GreenPtr<GreenBlock> value) noexcept;

        [[nodiscard]] constexpr Optional<const GreenExpressionBody &> expression_body() const noexcept
        {
            return expression_body_.get();
        }

        void set_expression_body(GreenPtr<GreenExpressionBody> value) noexcept;

        [[nodiscard]] constexpr Optional<const GreenToken &> semicolon() const noexcept
        {
            return semicolon_.get();
        }

        void set_semicolon(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::function_declaration;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenDeclaration> with_modifiers_core(
            GreenSyntaxList<GreenToken> modifiers) const override;

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
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

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

    template <>
    struct GreenNodeTraits<GreenFunctionDeclaration>
    {
        static constexpr std::size_t slot_count = 8;

        using ChildTypes = std::tuple<GreenSyntaxList<GreenToken>,
                                      GreenToken,
                                      GreenToken,
                                      GreenParameterList,
                                      GreenTypeSpecifier,
                                      GreenBlock,
                                      GreenExpressionBody,
                                      GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenFunctionDeclaration &node)
        {
            if constexpr (N == 0)
            {
                return node.modifiers();
            }
            else if constexpr (N == 1)
            {
                return node.func_keyword();
            }
            else if constexpr (N == 2)
            {
                return node.identifier();
            }
            else if constexpr (N == 3)
            {
                return node.parameters();
            }
            else if constexpr (N == 4)
            {
                return node.return_type();
            }
            else if constexpr (N == 5)
            {
                return node.body();
            }
            else if constexpr (N == 6)
            {
                return node.expression_body();
            }
            else
            {
                static_assert(N == 7);
                return node.semicolon();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenFunctionDeclaration>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenFunctionDeclaration &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_modifiers(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_func_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 2)
            {
                node.set_identifier(std::forward<Arg>(value));
            }
            else if constexpr (N == 3)
            {
                node.set_parameters(std::forward<Arg>(value));
            }
            else if constexpr (N == 4)
            {
                node.set_return_type(std::forward<Arg>(value));
            }
            else if constexpr (N == 5)
            {
                node.set_body(std::forward<Arg>(value));
            }
            else if constexpr (N == 6)
            {
                node.set_expression_body(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 7);
                node.set_semicolon(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenFunctionDeclaration>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenFunctionDeclaration> with(const GreenFunctionDeclaration &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_modifiers(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_func_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 2)
            {
                return node.with_identifier(std::forward<Arg>(value));
            }
            else if constexpr (N == 3)
            {
                return node.with_parameters(std::forward<Arg>(value));
            }
            else if constexpr (N == 4)
            {
                return node.with_return_type(std::forward<Arg>(value));
            }
            else if constexpr (N == 5)
            {
                return node.with_body(std::forward<Arg>(value));
            }
            else if constexpr (N == 6)
            {
                return node.with_expression_body(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 7);
                return node.with_semicolon(std::forward<Arg>(value));
            }
        }
    };
} // namespace prism
