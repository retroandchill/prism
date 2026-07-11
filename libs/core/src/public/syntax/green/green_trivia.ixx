/**
 * @file green_trivia.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
module;

#include <cassert>

export module prism.core:syntax.green.green_trivia;

import :syntax.green.green_node;

namespace prism
{
    class GreenTrivia final : public GreenNode
    {
      public:
        static const GreenPtr<GreenTrivia> carriage_return_line_feed;
        static const GreenPtr<GreenTrivia> carriage_return;
        static const GreenPtr<GreenTrivia> line_feed;

        constexpr GreenTrivia(const SyntaxKind kind, std::string text)
            : GreenNode{kind, static_cast<std::uint32_t>(text.size())}, text_{std::move(text)}
        {
            assert(text_.size() <= std::numeric_limits<std::uint32_t>::max());
            assert(is_trivia(kind));
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
            std::unreachable();
        }

      private:
        std::string text_;
    };
} // namespace prism
