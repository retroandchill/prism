/**
 * @file token.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core:syntax.token;

import :syntax.literals;
import :text.text_span;

namespace prism
{
    class SyntaxNode;
    class SyntaxTriviaList;
    class SyntaxTree;
    export class SyntaxTokenList;

    template <LiteralData T>
    using LiteralDataResult = decltype(LiteralDataTraits<T>::get_value(std::declval<const GreenToken>()));

    export class PRISM_CORE_API SyntaxToken final
    {
      public:
        SyntaxToken(const GreenToken &token, const std::uint32_t position) : green_{&token}, position_{position}
        {
        }

        SyntaxToken(const GreenToken &token, const SyntaxNode *parent, const std::uint32_t position)
            : parent_{parent}, green_{&token}, position_{position}
        {
        }

        [[nodiscard]] constexpr SyntaxKind kind() const noexcept
        {
            return green_->kind();
        }

        [[nodiscard]] constexpr TextSpan full_span() const noexcept
        {
            return {.start = position_, .length = green_->full_width()};
        }

        [[nodiscard]] constexpr TextSpan span() const
        {
            return {.start = position_ + green_->leading_trivia_width(), .length = green_->width()};
        }

        [[nodiscard]] constexpr Optional<const SyntaxNode &> parent() const noexcept
        {
            return parent_;
        }

        [[nodiscard]] Optional<const SyntaxTree &> tree() const;

        [[nodiscard]] constexpr bool is_missing() const noexcept
        {
            return green_->is_missing();
        }

        [[nodiscard]] constexpr std::uint32_t contains_diagnostics() const noexcept
        {
            return green_->contains_diagnostics();
        }

        [[nodiscard]] SyntaxTriviaList leading_trivia() const noexcept;

        [[nodiscard]] constexpr bool has_leading_trivia() const noexcept
        {
            return green_->has_leading_trivia();
        }

        [[nodiscard]] SyntaxTriviaList trailing_trivia() const noexcept;

        [[nodiscard]] constexpr bool has_trailing_trivia() const noexcept
        {
            return green_->has_trailing_trivia();
        }

        template <LiteralData T>
        Optional<LiteralDataResult<T>> try_get_value() const
        {
            if (LiteralDataTraits<T>::valid_kind(kind()))
            {
                return LiteralDataTraits<T>::get_value(*green_);
            }

            return std::nullopt;
        }

        template <LiteralData T>
        LiteralDataResult<T> get_value() const
        {
            if (!LiteralDataResult<T>::valid_kind(kind()))
                throw std::invalid_argument{"Invalid token kind"};

            return get_value_unchecked<T>();
        }

        template <LiteralData T>
        LiteralDataResult<T> get_value_unchecked() const
        {
            return LiteralDataResult<T>::get_value(*green_);
        }

      private:
        friend class SyntaxTokenList;
        friend class SyntaxTriviaList;
        friend class Lexer;
        friend class ChildSyntaxList;
        friend class SyntaxNodeOrTokenList;
        template <typename T>
        friend class SeparatedSyntaxList;

        const SyntaxNode *parent_ = nullptr;
        const GreenToken *green_;
        std::uint32_t position_;
    };

    class SyntaxTokenList PRISM_CORE_API final : public SyntaxListView<SyntaxToken>
    {
      public:
        constexpr explicit SyntaxTokenList(const SyntaxNode *parent,
                                           const GreenTokenList &token_list,
                                           const std::uint32_t position)
            : parent_{parent}, green_{token_list}, position_{position}
        {
        }

        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            return green_.size();
        }

        [[nodiscard]] SyntaxToken operator[](std::size_t index) const;

      private:
        friend class SyntaxToken;

        const SyntaxNode *parent_;
        GreenSyntaxList<GreenToken, false> green_;
        std::uint32_t position_;
    };
} // namespace prism
