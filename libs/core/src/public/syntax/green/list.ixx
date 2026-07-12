/**
 * @file list.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
export module prism.core:syntax.green.list;

import :syntax.green.node;

namespace prism
{
    template <typename T>
    class GreenSyntaxList final : public GreenNode
    {
      public:
        constexpr GreenSyntaxList() : GreenNode{SyntaxKind::list}
        {
        }

        explicit constexpr GreenSyntaxList(std::vector<GreenPtr<T>> children)
            : GreenNode{SyntaxKind::list}, children_{std::move(children)}
        {
            set_child_count(static_cast<std::uint32_t>(children_.size()));
            for (const auto &child : children_)
            {
                adjust_flags_and_width(*child);
            }
        }

        [[nodiscard]] constexpr bool empty() const noexcept
        {
            return children_.empty();
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(const std::size_t index) const override
        {
            return children_[index].get();
        }

      private:
        std::vector<GreenPtr<T>> children_;
    };
} // namespace prism
