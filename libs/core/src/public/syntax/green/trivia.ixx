/**
 * @file trivia.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
module;

#include <cassert>

export module prism.core:syntax.green.trivia;

import :syntax.green.node;
import :syntax.green.list;
import :util.exceptions;

namespace prism
{
    class GreenTrivia final : public GreenNode
    {
      public:
        constexpr GreenTrivia(const SyntaxKind kind, std::string text)
            : GreenNode{kind, static_cast<std::uint32_t>(text.size())}, text_{std::move(text)}
        {
            assert(text_.size() <= std::numeric_limits<std::uint32_t>::max());
            assert(prism::is_trivia(kind));
        }

        static const GreenPtr<GreenTrivia> &carriage_return_line_feed();
        static const GreenPtr<GreenTrivia> &carriage_return();
        static const GreenPtr<GreenTrivia> &line_feed();

        [[nodiscard]] constexpr bool is_trivia() const noexcept override
        {
            return true;
        }

        [[nodiscard]] constexpr const std::string &text() const noexcept
        {
            return text_;
        }

        [[nodiscard]] constexpr std::uint32_t width() const override
        {
            return static_cast<std::uint32_t>(text_.size());
        }

        [[nodiscard]] constexpr std::uint32_t trailing_trivia_width() const override
        {
            return 0;
        }

        [[nodiscard]] constexpr std::uint32_t leading_trivia_width() const override
        {
            return 0;
        }

        Optional<const GreenNode &> get_child(std::size_t index) const override
        {
            // This should never get called because it has no slots
            throw UnsupportedOperationException{};
        }

        [[nodiscard]] const SyntaxNode &create_red(const SyntaxLifetime &,
                                                   const SyntaxNode *,
                                                   std::uint32_t) const override
        {
            throw UnsupportedOperationException{};
        }

      private:
        std::string text_;
    };

    using GreenTriviaList = GreenSyntaxList<GreenTrivia>;
} // namespace prism
