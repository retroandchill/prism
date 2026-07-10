/**
 * @file token.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core:syntax.token;

import :syntax.literals;

namespace prism
{
    template <LiteralData T>
    using LiteralDataResult = decltype(LiteralDataTraits<T>::get_value(std::declval<const GreenToken>()));

    export class SyntaxToken
    {
        SyntaxToken(RefCountPtr<const GreenToken> token, std::uint32_t position)
            : token_{std::move(token)}, position_{position}
        {
        }

      public:
        [[nodiscard]] constexpr SyntaxKind kind() const noexcept
        {
            return token_->kind();
        }

        [[nodiscard]] constexpr bool is_missing() const noexcept
        {
            return token_->is_missing();
        }

        [[nodiscard]] constexpr std::uint32_t contains_diagnostics() const noexcept
        {
            return token_->contains_diagnostics();
        }

        template <LiteralData T>
        Optional<LiteralDataResult<T>> try_get_value() const
        {
            if (LiteralDataTraits<T>::valid_kind(kind()))
            {
                return LiteralDataTraits<T>::get_value(*token_);
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
            return LiteralDataResult<T>::get_value(*token_);
        }

      private:
        RefCountPtr<const GreenToken> token_;
        std::uint32_t position_;
    };
} // namespace prism
