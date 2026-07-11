/**
 * @file trivia.ixx
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
export module prism.core:syntax.trivia;

import :syntax.token;

namespace prism
{
    export class SyntaxTrivia final
    {
        constexpr SyntaxTrivia(SyntaxToken parent, GreenPtr<GreenTrivia> trivia, std::uint32_t position)
            : parent_{std::move(parent)}, trivia_{std::move(trivia)}, position_{position}
        {
        }

      public:
        [[nodiscard]] constexpr const SyntaxToken &parent() const noexcept
        {
            return parent_;
        }

      private:
        SyntaxToken parent_;
        GreenPtr<GreenTrivia> trivia_;
        std::uint32_t position_;
    };
} // namespace prism
