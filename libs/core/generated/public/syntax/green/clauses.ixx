export module prism.core:syntax.green.clauses;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.separated_list;

namespace prism
{
    class GreenArgument;
    class GreenExpression;
    class GreenStatement;
    class GreenType;
    class GreenVariableDeclaration;

    class GreenInitializer final : public GreenNode
    {
      public:
        explicit GreenInitializer(GreenPtr<GreenToken> equal_sign,
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

    class GreenTypeHint final : public GreenNode
    {
      public:
        explicit GreenTypeHint(GreenPtr<GreenToken> colon,
                               GreenPtr<GreenType> type,
                               DiagnosticInfoList diagnostics = {});
        ~GreenTypeHint() override;

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
            return node.kind() == SyntaxKind::type_hint;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      private:
        GreenPtr<GreenToken> colon_;
        GreenPtr<GreenType> type_;
    };

    class GreenNamedParameter final : public GreenNode
    {
      public:
        explicit GreenNamedParameter(GreenPtr<GreenToken> name,
                                     GreenPtr<GreenToken> colon,
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

      private:
        GreenPtr<GreenToken> name_;
        GreenPtr<GreenToken> colon_;
    };

    class GreenArgumentList final : public GreenNode
    {
      public:
        GreenArgumentList(GreenPtr<GreenToken> open_paren,
                          GreenSeparatedList<GreenArgument> argument,
                          GreenPtr<GreenToken> close_paren,
                          DiagnosticInfoList diagnostics = {});
        ~GreenArgumentList() override;

        [[nodiscard]] constexpr const GreenToken &open_paren() const noexcept
        {
            return *open_paren_;
        }

        [[nodiscard]] constexpr const GreenSeparatedList<GreenArgument> &argument() const noexcept
        {
            return argument_;
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
        GreenSeparatedList<GreenArgument> argument_;
        GreenPtr<GreenToken> close_paren_;
    };

    class GreenArgument final : public GreenNode
    {
      public:
        explicit GreenArgument(GreenPtr<GreenNamedParameter> name,
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

} // namespace prism
