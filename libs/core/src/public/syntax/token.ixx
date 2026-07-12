/**
 * @file token.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core:syntax.token;

import :syntax.literals;
import :text.text_span;

namespace prism
{
    class SyntaxNode;
    class SyntaxTriviaList;

    template <LiteralData T>
    using LiteralDataResult = decltype(LiteralDataTraits<T>::get_value(std::declval<const GreenToken>()));

    export class SyntaxToken
    {
        SyntaxToken(const GreenToken &token, const std::uint32_t position) : green_{&token}, position_{position}
        {
        }

      public:
        [[nodiscard]] constexpr SyntaxKind kind() const noexcept
        {
            return green_->kind();
        }

        [[nodiscard]] constexpr TextSpan full_span() const noexcept
        {
            return {position_, green_->full_width()};
        }

        [[nodiscard]] constexpr TextSpan span() const
        {
            return {position_ + green_->leading_trivia_width(), green_->width()};
        }

        [[nodiscard]] constexpr bool is_missing() const noexcept
        {
            return green_->is_missing();
        }

        [[nodiscard]] constexpr std::uint32_t contains_diagnostics() const noexcept
        {
            return green_->contains_diagnostics();
        }

        [[nodiscard]] constexpr SyntaxTriviaList leading_trivia() const noexcept;

        [[nodiscard]] constexpr bool has_leading_trivia() const noexcept
        {
            return green_->has_leading_trivia();
        }

        [[nodiscard]] constexpr SyntaxTriviaList trailing_trivia() const noexcept;

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
        friend class SyntaxTriviaList;
        friend class Lexer;

        SyntaxNode *parent_ = nullptr;
        const GreenToken *green_;
        std::uint32_t position_;
    };
} // namespace prism
