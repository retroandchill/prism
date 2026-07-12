/**
 * @file type_hint.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
export module prism.core:syntax.green.type_hint;

import :syntax.green.node;
import :syntax.green.token;

namespace prism
{
    class GreenType;

    class GreenTypeHint final : public GreenNode
    {
      public:
        GreenTypeHint(GreenPtr<GreenToken> colon, GreenPtr<GreenType> type);

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
