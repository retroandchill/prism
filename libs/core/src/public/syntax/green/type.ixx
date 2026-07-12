/**
 * @file green_type.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
export module prism.core:syntax.green.type;

import :syntax.green.node;
import :syntax.green.token;

namespace prism
{
    class GreenType : public GreenNode
    {
      protected:
        using GreenNode::GreenNode;
    };

    class GreenNamedType : public GreenType
    {
      protected:
        using GreenType::GreenType;
    };

    class GreenSimpleNamedType : public GreenNamedType
    {
      protected:
        using GreenNamedType::GreenNamedType;
    };

    class GreenIdentifierNamedType final : public GreenSimpleNamedType
    {
      public:
        explicit GreenIdentifierNamedType(GreenPtr<GreenToken> identifier);

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      private:
        GreenPtr<GreenToken> identifier_;
    };
} // namespace prism
