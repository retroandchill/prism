/**
 * @file trivia.ixx
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core:syntax.trivia;

import :syntax.token;
import :syntax.list_view;

namespace prism
{
    export class SyntaxTriviaList;

    export class PRISM_CORE_API SyntaxTrivia final
    {
        constexpr SyntaxTrivia(SyntaxToken parent, const GreenTrivia &trivia, const std::uint32_t position)
            : token_{std::move(parent)}, green_{&trivia}, position_{position}
        {
        }

      public:
        [[nodiscard]] constexpr const SyntaxToken &token() const noexcept
        {
            return token_;
        }

        [[nodiscard]] constexpr Optional<const SyntaxTree &> tree() const
        {
            return token_.tree();
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

        SyntaxToken token_;
        const GreenTrivia *green_;
        std::uint32_t position_;
    };

    class SyntaxTriviaList PRISM_CORE_API final : public SyntaxListView<SyntaxTrivia>
    {
        constexpr explicit SyntaxTriviaList(SyntaxToken parent, const GreenTriviaList &trivia_list)
            : parent_{std::move(parent)}, green_{trivia_list}, position_{parent.position_}
        {
        }

        constexpr explicit SyntaxTriviaList(SyntaxToken parent,
                                            const GreenTriviaList &trivia_list,
                                            std::uint32_t position)
            : parent_{std::move(parent)}, green_{trivia_list}, position_{position}
        {
        }

      public:
        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            return green_.size();
        }

        [[nodiscard]] SyntaxTrivia operator[](std::size_t index) const;

      private:
        friend class SyntaxToken;

        SyntaxToken parent_;
        GreenSyntaxList<GreenTrivia, false> green_;
        std::uint32_t position_;
    };
} // namespace prism
