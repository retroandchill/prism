export module prism.core:syntax.green.clauses;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.separated_list;

namespace prism
{
    class GreenArgument;
    class GreenBlock;
    class GreenExpression;
    class GreenParameter;
    class GreenStatement;
    class GreenType;
    class GreenVariableDeclaration;

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

        [[nodiscard]] constexpr const GreenExpression &value() const noexcept
        {
            return *value_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::initializer;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

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

        [[nodiscard]] constexpr const GreenType &type() const noexcept
        {
            return *type_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::type_specifier;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

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

        [[nodiscard]] constexpr const GreenToken &colon() const noexcept
        {
            return *colon_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::named_parameter;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

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

        [[nodiscard]] constexpr const GreenExpression &value() const noexcept
        {
            return *value_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::argument;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

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

        [[nodiscard]] constexpr const GreenExpression &expression() const noexcept
        {
            return *expression_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::expression_body;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      private:
        GreenPtr<GreenToken> arrow_;
        GreenPtr<GreenExpression> expression_;
    };
} // namespace prism
