/**
 * @file green_trivia_list.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core:syntax.green.green_trivia_list;

import :syntax.green.green_trivia;

namespace prism
{
    class GreenTriviaList final : public GreenNode
    {
      public:
        constexpr GreenTriviaList() : GreenNode{SyntaxKind::list, 0}
        {
        }

        explicit constexpr GreenTriviaList(std::vector<RefCountPtr<const GreenTrivia>> trivia)
            : GreenNode{SyntaxKind::list, calculate_width(trivia)}, trivia_{std::move(trivia)}
        {
        }

        [[nodiscard]] constexpr bool empty() const noexcept
        {
            return trivia_.empty();
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(const std::size_t index) const override
        {
            return trivia_[index].get();
        }

      private:
        static std::uint32_t calculate_width(const std::vector<RefCountPtr<const GreenTrivia>> &trivia);

        std::vector<RefCountPtr<const GreenTrivia>> trivia_;
    };

    RefCountPtr<const GreenTriviaList> normalize_trivia(RefCountPtr<const GreenTriviaList> trivia);
} // namespace prism
