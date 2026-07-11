/**
 * @file trivia.ixx
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
module;

#include <prism/core/exports.h>

export module prism.core:syntax.trivia;

import :syntax.token;
import :syntax.list_view;

namespace prism
{
    export class SyntaxTriviaList;

    export class SyntaxTrivia final
    {
        constexpr SyntaxTrivia(SyntaxToken parent, GreenPtr<GreenTrivia> trivia, const std::uint32_t position)
            : parent_{std::move(parent)}, green_{std::move(trivia)}, position_{position}
        {
        }

      public:
        [[nodiscard]] constexpr const SyntaxToken &parent() const noexcept
        {
            return parent_;
        }

        [[nodiscard]] constexpr TextSpan full_span() const noexcept
        {
            return {position_, green_->full_width()};
        }

        [[nodiscard]] constexpr TextSpan span() const
        {
            return {position_ + green_->leading_trivia_width(), green_->width()};
        }

        [[nodiscard]] constexpr bool contains_diagnostics() const noexcept
        {
            return green_->contains_diagnostics();
        }

      private:
        friend class SyntaxTriviaList;

        SyntaxToken parent_;
        GreenPtr<GreenTrivia> green_;
        std::uint32_t position_;
    };

    class SyntaxTriviaList PRISM_CORE_API final : public SyntaxListView<SyntaxTrivia>
    {
        constexpr explicit SyntaxTriviaList(SyntaxToken parent, GreenPtr<GreenTriviaList> trivia_list)
            : parent_{std::move(parent)}, green_{std::move(trivia_list)}, position_{parent.position_}
        {
        }

        constexpr explicit SyntaxTriviaList(SyntaxToken parent,
                                            GreenPtr<GreenTriviaList> trivia_list,
                                            std::uint32_t position)
            : parent_{std::move(parent)}, green_{std::move(trivia_list)}, position_{position}
        {
        }

      public:
        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            return green_ != nullptr ? green_->child_count() : 0;
        }

        [[nodiscard]] SyntaxTrivia operator[](std::size_t index) const;

      private:
        friend class SyntaxToken;

        SyntaxToken parent_;
        GreenPtr<GreenTriviaList> green_;
        std::uint32_t position_;
    };

    constexpr SyntaxTriviaList SyntaxToken::leading_trivia() const noexcept
    {
        auto trivia = green_->leading_trivia()
                          .transform([](const GreenTriviaList &list) { return list.shared_from_this(); })
                          .value_or_default();
        return SyntaxTriviaList{*this, std::move(trivia)};
    }

    constexpr SyntaxTriviaList SyntaxToken::trailing_trivia() const noexcept
    {
        auto trivia = green_->trailing_trivia()
                          .transform([](const GreenTriviaList &list) { return list.shared_from_this(); })
                          .value_or_default();
        return SyntaxTriviaList{*this, std::move(trivia)};
    }
} // namespace prism
