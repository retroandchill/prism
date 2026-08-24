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
import :diagnostics.location;

namespace prism
{
    export class SyntaxNode;
    export class SyntaxTriviaList;
    export class SyntaxTree;
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

        [[nodiscard]] Location location() const;

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
            if (!LiteralDataTraits<T>::valid_kind(kind()))
                throw std::invalid_argument{"Invalid token kind"};

            return get_value_unchecked<T>();
        }

        template <LiteralData T>
        LiteralDataResult<T> get_value_unchecked() const
        {
            return LiteralDataTraits<T>::get_value(*green_);
        }

        template <LiteralData T>
        [[nodiscard]] constexpr bool holds_value() const noexcept
        {
            return LiteralDataTraits<T>::valid_kind(kind());
        }

        template <LiteralData... Ts>
        [[nodiscard]] constexpr bool holds_any_of() const noexcept
        {
            return (holds_value<Ts>() || ...);
        }

      private:
        friend struct SyntaxTokenInternal;

        const SyntaxNode *parent_ = nullptr;
        const GreenToken *green_;
        std::uint32_t position_;
    };

    struct SyntaxTokenInternal
    {
        [[nodiscard]] static constexpr const GreenToken &get_green(const SyntaxToken &token) noexcept
        {
            return *token.green_;
        }

        [[nodiscard]] static constexpr std::uint32_t get_position(const SyntaxToken &token) noexcept
        {
            return token.position_;
        }
    };

    class PRISM_CORE_API SyntaxTokenList final : public SyntaxListView<SyntaxToken>
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
