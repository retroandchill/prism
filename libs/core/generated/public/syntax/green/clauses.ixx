export module prism.core:syntax.green.clauses;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.trivia;
import :syntax.green.separated_list;

namespace prism
{
    class GreenArgument;
    class GreenExpression;
    class GreenParameter;
    class GreenType;

    class GreenInitializer final : public GreenNode
    {
      public:
        GreenInitializer(GreenPtr<GreenToken> equal_sign,
                         GreenPtr<GreenExpression> value,
                         DiagnosticInfoList diagnostics = {});

        ~GreenInitializer() override;

        [[nodiscard]] constexpr const GreenToken &equal_sign() const noexcept
        {
            return *equal_sign_;
        }

        void set_equal_sign(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenExpression &value() const noexcept
        {
            return *value_;
        }

        void set_value(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::initializer;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenInitializer> with_equal_sign(GreenPtr<GreenToken> equal_sign) const;

        [[nodiscard]] GreenPtr<GreenInitializer> with_value(GreenPtr<GreenExpression> value) const;

        [[nodiscard]] GreenPtr<GreenInitializer> update(GreenPtr<GreenToken> equal_sign,
                                                        GreenPtr<GreenExpression> value) const;

      protected:
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> equal_sign_;
        GreenPtr<GreenExpression> value_;
    };

    class GreenTypeSpecifier final : public GreenNode
    {
      public:
        GreenTypeSpecifier(GreenPtr<GreenToken> colon, GreenPtr<GreenType> type, DiagnosticInfoList diagnostics = {});

        ~GreenTypeSpecifier() override;

        [[nodiscard]] constexpr const GreenToken &colon() const noexcept
        {
            return *colon_;
        }

        void set_colon(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenType &type() const noexcept
        {
            return *type_;
        }

        void set_type(GreenPtr<GreenType> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::type_specifier;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenTypeSpecifier> with_colon(GreenPtr<GreenToken> colon) const;

        [[nodiscard]] GreenPtr<GreenTypeSpecifier> with_type(GreenPtr<GreenType> type) const;

        [[nodiscard]] GreenPtr<GreenTypeSpecifier> update(GreenPtr<GreenToken> colon, GreenPtr<GreenType> type) const;

      protected:
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> colon_;
        GreenPtr<GreenType> type_;
    };

    class GreenNamedParameter final : public GreenNode
    {
      public:
        GreenNamedParameter(GreenPtr<GreenToken> name, GreenPtr<GreenToken> colon, DiagnosticInfoList diagnostics = {});

        ~GreenNamedParameter() override;

        [[nodiscard]] constexpr const GreenToken &name() const noexcept
        {
            return *name_;
        }

        void set_name(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &colon() const noexcept
        {
            return *colon_;
        }

        void set_colon(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::named_parameter;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenNamedParameter> with_name(GreenPtr<GreenToken> name) const;

        [[nodiscard]] GreenPtr<GreenNamedParameter> with_colon(GreenPtr<GreenToken> colon) const;

        [[nodiscard]] GreenPtr<GreenNamedParameter> update(GreenPtr<GreenToken> name, GreenPtr<GreenToken> colon) const;

      protected:
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> name_;
        GreenPtr<GreenToken> colon_;
    };

    class GreenArgumentList final : public GreenNode
    {
      public:
        GreenArgumentList(GreenPtr<GreenToken> open_paren,
                          GreenSeparatedList<GreenArgument> arguments,
                          GreenPtr<GreenToken> close_paren,
                          DiagnosticInfoList diagnostics = {});

        ~GreenArgumentList() override;

        [[nodiscard]] constexpr const GreenToken &open_paren() const noexcept
        {
            return *open_paren_;
        }

        void set_open_paren(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr GreenSeparatedList<GreenArgument> arguments() const noexcept
        {
            return arguments_;
        }

        void set_arguments(GreenSeparatedList<GreenArgument> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &close_paren() const noexcept
        {
            return *close_paren_;
        }

        void set_close_paren(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::argument_list;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenArgumentList> with_open_paren(GreenPtr<GreenToken> open_paren) const;

        [[nodiscard]] GreenPtr<GreenArgumentList> with_arguments(GreenSeparatedList<GreenArgument> arguments) const;

        [[nodiscard]] GreenPtr<GreenArgumentList> with_close_paren(GreenPtr<GreenToken> close_paren) const;

        [[nodiscard]] GreenPtr<GreenArgumentList> update(GreenPtr<GreenToken> open_paren,
                                                         GreenSeparatedList<GreenArgument> arguments,
                                                         GreenPtr<GreenToken> close_paren) const;

      protected:
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> open_paren_;
        GreenSeparatedList<GreenArgument> arguments_;
        GreenPtr<GreenToken> close_paren_;
    };

    class GreenArgument final : public GreenNode
    {
      public:
        GreenArgument(GreenPtr<GreenNamedParameter> name,
                      GreenPtr<GreenExpression> value,
                      DiagnosticInfoList diagnostics = {});

        ~GreenArgument() override;

        [[nodiscard]] constexpr Optional<const GreenNamedParameter &> name() const noexcept
        {
            return name_.get();
        }

        void set_name(GreenPtr<GreenNamedParameter> value) noexcept;

        [[nodiscard]] constexpr const GreenExpression &value() const noexcept
        {
            return *value_;
        }

        void set_value(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::argument;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenArgument> with_name(GreenPtr<GreenNamedParameter> name) const;

        [[nodiscard]] GreenPtr<GreenArgument> with_value(GreenPtr<GreenExpression> value) const;

        [[nodiscard]] GreenPtr<GreenArgument> update(GreenPtr<GreenNamedParameter> name,
                                                     GreenPtr<GreenExpression> value) const;

      protected:
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenNamedParameter> name_;
        GreenPtr<GreenExpression> value_;
    };

    class GreenParameterList final : public GreenNode
    {
      public:
        GreenParameterList(GreenPtr<GreenToken> open_paren,
                           GreenSeparatedList<GreenParameter> parameters,
                           GreenPtr<GreenToken> close_paren,
                           DiagnosticInfoList diagnostics = {});

        ~GreenParameterList() override;

        [[nodiscard]] constexpr const GreenToken &open_paren() const noexcept
        {
            return *open_paren_;
        }

        void set_open_paren(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr GreenSeparatedList<GreenParameter> parameters() const noexcept
        {
            return parameters_;
        }

        void set_parameters(GreenSeparatedList<GreenParameter> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &close_paren() const noexcept
        {
            return *close_paren_;
        }

        void set_close_paren(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::parameter_list;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenParameterList> with_open_paren(GreenPtr<GreenToken> open_paren) const;

        [[nodiscard]] GreenPtr<GreenParameterList> with_parameters(GreenSeparatedList<GreenParameter> parameters) const;

        [[nodiscard]] GreenPtr<GreenParameterList> with_close_paren(GreenPtr<GreenToken> close_paren) const;

        [[nodiscard]] GreenPtr<GreenParameterList> update(GreenPtr<GreenToken> open_paren,
                                                          GreenSeparatedList<GreenParameter> parameters,
                                                          GreenPtr<GreenToken> close_paren) const;

      protected:
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> open_paren_;
        GreenSeparatedList<GreenParameter> parameters_;
        GreenPtr<GreenToken> close_paren_;
    };

    class GreenParameter final : public GreenNode
    {
      public:
        GreenParameter(GreenPtr<GreenToken> mut_keyword,
                       GreenPtr<GreenToken> name,
                       GreenPtr<GreenTypeSpecifier> type_specifier,
                       GreenPtr<GreenInitializer> default_value,
                       DiagnosticInfoList diagnostics = {});

        ~GreenParameter() override;

        [[nodiscard]] constexpr Optional<const GreenToken &> mut_keyword() const noexcept
        {
            return mut_keyword_.get();
        }

        void set_mut_keyword(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &name() const noexcept
        {
            return *name_;
        }

        void set_name(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenTypeSpecifier &type_specifier() const noexcept
        {
            return *type_specifier_;
        }

        void set_type_specifier(GreenPtr<GreenTypeSpecifier> value) noexcept;

        [[nodiscard]] constexpr Optional<const GreenInitializer &> default_value() const noexcept
        {
            return default_value_.get();
        }

        void set_default_value(GreenPtr<GreenInitializer> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::parameter;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenParameter> with_mut_keyword(GreenPtr<GreenToken> mut_keyword) const;

        [[nodiscard]] GreenPtr<GreenParameter> with_name(GreenPtr<GreenToken> name) const;

        [[nodiscard]] GreenPtr<GreenParameter> with_type_specifier(GreenPtr<GreenTypeSpecifier> type_specifier) const;

        [[nodiscard]] GreenPtr<GreenParameter> with_default_value(GreenPtr<GreenInitializer> default_value) const;

        [[nodiscard]] GreenPtr<GreenParameter> update(GreenPtr<GreenToken> mut_keyword,
                                                      GreenPtr<GreenToken> name,
                                                      GreenPtr<GreenTypeSpecifier> type_specifier,
                                                      GreenPtr<GreenInitializer> default_value) const;

      protected:
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> mut_keyword_;
        GreenPtr<GreenToken> name_;
        GreenPtr<GreenTypeSpecifier> type_specifier_;
        GreenPtr<GreenInitializer> default_value_;
    };

    class GreenExpressionBody final : public GreenNode
    {
      public:
        GreenExpressionBody(GreenPtr<GreenToken> arrow,
                            GreenPtr<GreenExpression> expression,
                            DiagnosticInfoList diagnostics = {});

        ~GreenExpressionBody() override;

        [[nodiscard]] constexpr const GreenToken &arrow() const noexcept
        {
            return *arrow_;
        }

        void set_arrow(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenExpression &expression() const noexcept
        {
            return *expression_;
        }

        void set_expression(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::expression_body;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenExpressionBody> with_arrow(GreenPtr<GreenToken> arrow) const;

        [[nodiscard]] GreenPtr<GreenExpressionBody> with_expression(GreenPtr<GreenExpression> expression) const;

        [[nodiscard]] GreenPtr<GreenExpressionBody> update(GreenPtr<GreenToken> arrow,
                                                           GreenPtr<GreenExpression> expression) const;

      protected:
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> arrow_;
        GreenPtr<GreenExpression> expression_;
    };
} // namespace prism
