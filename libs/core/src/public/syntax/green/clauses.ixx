export module prism.core:syntax.green.clauses;

import :syntax.green.node;
import :syntax.green.token;

namespace prism
{
    class GreenExpression;
    class GreenType;

    class GreenInitializer final : public GreenNode
    {
      public:
        GreenInitializer(SyntaxKind kind,
                         GreenPtr<GreenToken> equal_sign,
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

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      private:
        GreenPtr<GreenToken> equal_sign_;
        GreenPtr<GreenExpression> value_;
    };

    class GreenTypeHint final : public GreenNode
    {
      public:
        GreenTypeHint(SyntaxKind kind,
                      GreenPtr<GreenToken> colon,
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

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      private:
        GreenPtr<GreenToken> colon_;
        GreenPtr<GreenType> type_;
    };

} // namespace prism
