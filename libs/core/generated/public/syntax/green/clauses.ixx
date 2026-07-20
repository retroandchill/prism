export module prism.core:syntax.green.clauses;

import :syntax.green.node;
import :syntax.green.token;
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
        GreenInitializer(RefCountPtr<const GreenToken> equal_sign,
                         RefCountPtr<const GreenExpression> value,
                         DiagnosticInfoList diagnostics = {});

        ~GreenInitializer() override;

        [[nodiscard]] constexpr const GreenToken &equal_sign() const noexcept
        {
            return *equal_sign_;
        }

        [[nodiscard]] constexpr const GreenExpression &value() const noexcept
        {
            return *value_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::initializer;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] RefCountPtr<const GreenInitializer> with_equal_sign(
            RefCountPtr<const GreenToken> equal_sign) const;

        [[nodiscard]] RefCountPtr<const GreenInitializer> with_value(RefCountPtr<const GreenExpression> value) const;

        [[nodiscard]] RefCountPtr<const GreenInitializer> update(RefCountPtr<const GreenToken> equal_sign,
                                                                 RefCountPtr<const GreenExpression> value) const;

      private:
        RefCountPtr<const GreenToken> equal_sign_;
        RefCountPtr<const GreenExpression> value_;
    };

    class GreenTypeSpecifier final : public GreenNode
    {
      public:
        GreenTypeSpecifier(RefCountPtr<const GreenToken> colon,
                           RefCountPtr<const GreenType> type,
                           DiagnosticInfoList diagnostics = {});

        ~GreenTypeSpecifier() override;

        [[nodiscard]] constexpr const GreenToken &colon() const noexcept
        {
            return *colon_;
        }

        [[nodiscard]] constexpr const GreenType &type() const noexcept
        {
            return *type_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::type_specifier;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] RefCountPtr<const GreenTypeSpecifier> with_colon(RefCountPtr<const GreenToken> colon) const;

        [[nodiscard]] RefCountPtr<const GreenTypeSpecifier> with_type(RefCountPtr<const GreenType> type) const;

        [[nodiscard]] RefCountPtr<const GreenTypeSpecifier> update(RefCountPtr<const GreenToken> colon,
                                                                   RefCountPtr<const GreenType> type) const;

      private:
        RefCountPtr<const GreenToken> colon_;
        RefCountPtr<const GreenType> type_;
    };

    class GreenNamedParameter final : public GreenNode
    {
      public:
        GreenNamedParameter(RefCountPtr<const GreenToken> name,
                            RefCountPtr<const GreenToken> colon,
                            DiagnosticInfoList diagnostics = {});

        ~GreenNamedParameter() override;

        [[nodiscard]] constexpr const GreenToken &name() const noexcept
        {
            return *name_;
        }

        [[nodiscard]] constexpr const GreenToken &colon() const noexcept
        {
            return *colon_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::named_parameter;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] RefCountPtr<const GreenNamedParameter> with_name(RefCountPtr<const GreenToken> name) const;

        [[nodiscard]] RefCountPtr<const GreenNamedParameter> with_colon(RefCountPtr<const GreenToken> colon) const;

        [[nodiscard]] RefCountPtr<const GreenNamedParameter> update(RefCountPtr<const GreenToken> name,
                                                                    RefCountPtr<const GreenToken> colon) const;

      private:
        RefCountPtr<const GreenToken> name_;
        RefCountPtr<const GreenToken> colon_;
    };

    class GreenArgumentList final : public GreenNode
    {
      public:
        GreenArgumentList(RefCountPtr<const GreenToken> open_paren,
                          GreenSeparatedList<GreenArgument> arguments,
                          RefCountPtr<const GreenToken> close_paren,
                          DiagnosticInfoList diagnostics = {});

        ~GreenArgumentList() override;

        [[nodiscard]] constexpr const GreenToken &open_paren() const noexcept
        {
            return *open_paren_;
        }

        [[nodiscard]] constexpr GreenSeparatedList<GreenArgument> arguments() const noexcept
        {
            return arguments_;
        }

        [[nodiscard]] constexpr const GreenToken &close_paren() const noexcept
        {
            return *close_paren_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::argument_list;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] RefCountPtr<const GreenArgumentList> with_open_paren(
            RefCountPtr<const GreenToken> open_paren) const;

        [[nodiscard]] RefCountPtr<const GreenArgumentList> with_arguments(
            GreenSeparatedList<GreenArgument> arguments) const;

        [[nodiscard]] RefCountPtr<const GreenArgumentList> with_close_paren(
            RefCountPtr<const GreenToken> close_paren) const;

        [[nodiscard]] RefCountPtr<const GreenArgumentList> update(RefCountPtr<const GreenToken> open_paren,
                                                                  GreenSeparatedList<GreenArgument> arguments,
                                                                  RefCountPtr<const GreenToken> close_paren) const;

      private:
        RefCountPtr<const GreenToken> open_paren_;
        GreenSeparatedList<GreenArgument> arguments_;
        RefCountPtr<const GreenToken> close_paren_;
    };

    class GreenArgument final : public GreenNode
    {
      public:
        GreenArgument(RefCountPtr<const GreenNamedParameter> name,
                      RefCountPtr<const GreenExpression> value,
                      DiagnosticInfoList diagnostics = {});

        ~GreenArgument() override;

        [[nodiscard]] constexpr Optional<const GreenNamedParameter &> name() const noexcept
        {
            return name_.get();
        }

        [[nodiscard]] constexpr const GreenExpression &value() const noexcept
        {
            return *value_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::argument;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] RefCountPtr<const GreenArgument> with_name(RefCountPtr<const GreenNamedParameter> name) const;

        [[nodiscard]] RefCountPtr<const GreenArgument> with_value(RefCountPtr<const GreenExpression> value) const;

        [[nodiscard]] RefCountPtr<const GreenArgument> update(RefCountPtr<const GreenNamedParameter> name,
                                                              RefCountPtr<const GreenExpression> value) const;

      private:
        RefCountPtr<const GreenNamedParameter> name_;
        RefCountPtr<const GreenExpression> value_;
    };

    class GreenParameterList final : public GreenNode
    {
      public:
        GreenParameterList(RefCountPtr<const GreenToken> open_paren,
                           GreenSeparatedList<GreenParameter> parameters,
                           RefCountPtr<const GreenToken> close_paren,
                           DiagnosticInfoList diagnostics = {});

        ~GreenParameterList() override;

        [[nodiscard]] constexpr const GreenToken &open_paren() const noexcept
        {
            return *open_paren_;
        }

        [[nodiscard]] constexpr GreenSeparatedList<GreenParameter> parameters() const noexcept
        {
            return parameters_;
        }

        [[nodiscard]] constexpr const GreenToken &close_paren() const noexcept
        {
            return *close_paren_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::parameter_list;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] RefCountPtr<const GreenParameterList> with_open_paren(
            RefCountPtr<const GreenToken> open_paren) const;

        [[nodiscard]] RefCountPtr<const GreenParameterList> with_parameters(
            GreenSeparatedList<GreenParameter> parameters) const;

        [[nodiscard]] RefCountPtr<const GreenParameterList> with_close_paren(
            RefCountPtr<const GreenToken> close_paren) const;

        [[nodiscard]] RefCountPtr<const GreenParameterList> update(RefCountPtr<const GreenToken> open_paren,
                                                                   GreenSeparatedList<GreenParameter> parameters,
                                                                   RefCountPtr<const GreenToken> close_paren) const;

      private:
        RefCountPtr<const GreenToken> open_paren_;
        GreenSeparatedList<GreenParameter> parameters_;
        RefCountPtr<const GreenToken> close_paren_;
    };

    class GreenParameter final : public GreenNode
    {
      public:
        GreenParameter(RefCountPtr<const GreenToken> mut_keyword,
                       RefCountPtr<const GreenToken> name,
                       RefCountPtr<const GreenTypeSpecifier> type_specifier,
                       RefCountPtr<const GreenInitializer> default_value,
                       DiagnosticInfoList diagnostics = {});

        ~GreenParameter() override;

        [[nodiscard]] constexpr Optional<const GreenToken &> mut_keyword() const noexcept
        {
            return mut_keyword_.get();
        }

        [[nodiscard]] constexpr const GreenToken &name() const noexcept
        {
            return *name_;
        }

        [[nodiscard]] constexpr const GreenTypeSpecifier &type_specifier() const noexcept
        {
            return *type_specifier_;
        }

        [[nodiscard]] constexpr Optional<const GreenInitializer &> default_value() const noexcept
        {
            return default_value_.get();
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::parameter;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] RefCountPtr<const GreenParameter> with_mut_keyword(
            RefCountPtr<const GreenToken> mut_keyword) const;

        [[nodiscard]] RefCountPtr<const GreenParameter> with_name(RefCountPtr<const GreenToken> name) const;

        [[nodiscard]] RefCountPtr<const GreenParameter> with_type_specifier(
            RefCountPtr<const GreenTypeSpecifier> type_specifier) const;

        [[nodiscard]] RefCountPtr<const GreenParameter> with_default_value(
            RefCountPtr<const GreenInitializer> default_value) const;

        [[nodiscard]] RefCountPtr<const GreenParameter> update(RefCountPtr<const GreenToken> mut_keyword,
                                                               RefCountPtr<const GreenToken> name,
                                                               RefCountPtr<const GreenTypeSpecifier> type_specifier,
                                                               RefCountPtr<const GreenInitializer> default_value) const;

      private:
        RefCountPtr<const GreenToken> mut_keyword_;
        RefCountPtr<const GreenToken> name_;
        RefCountPtr<const GreenTypeSpecifier> type_specifier_;
        RefCountPtr<const GreenInitializer> default_value_;
    };

    class GreenExpressionBody final : public GreenNode
    {
      public:
        GreenExpressionBody(RefCountPtr<const GreenToken> arrow,
                            RefCountPtr<const GreenExpression> expression,
                            DiagnosticInfoList diagnostics = {});

        ~GreenExpressionBody() override;

        [[nodiscard]] constexpr const GreenToken &arrow() const noexcept
        {
            return *arrow_;
        }

        [[nodiscard]] constexpr const GreenExpression &expression() const noexcept
        {
            return *expression_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::expression_body;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] RefCountPtr<const GreenExpressionBody> with_arrow(RefCountPtr<const GreenToken> arrow) const;

        [[nodiscard]] RefCountPtr<const GreenExpressionBody> with_expression(
            RefCountPtr<const GreenExpression> expression) const;

        [[nodiscard]] RefCountPtr<const GreenExpressionBody> update(
            RefCountPtr<const GreenToken> arrow,
            RefCountPtr<const GreenExpression> expression) const;

      private:
        RefCountPtr<const GreenToken> arrow_;
        RefCountPtr<const GreenExpression> expression_;
    };
} // namespace prism
