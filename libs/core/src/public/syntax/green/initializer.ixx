/**
 * @file initializer.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
export module prism.core:syntax.green.initializer;

import :syntax.green.node;

namespace prism
{
    class GreenExpression;
    class GreenToken;

    class GreenInitializer final : public GreenNode
    {
      public:
        GreenInitializer(GreenPtr<GreenToken> equal_sign, GreenPtr<GreenExpression> expression);

        [[nodiscard]] constexpr const GreenToken &equal_sign() const noexcept
        {
            return *equal_sign_;
        }

        [[nodiscard]] constexpr const GreenExpression &expression() const noexcept
        {
            return *expression_;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      private:
        GreenPtr<GreenToken> equal_sign_;
        GreenPtr<GreenExpression> expression_;
    };
} // namespace prism
