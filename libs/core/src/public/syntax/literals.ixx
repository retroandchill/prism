/**
 * @file literals.ixx
 * @author Francesco Corso
 * @date 7/10/2026
 * @brief
 */
export module prism.core:syntax.literals;

import std;
import :syntax.kind;
import :syntax.green.green_value_token;

namespace prism
{
    template <typename>
    struct LiteralDataTraits;

    template <StandardLiteralData T>
    struct LiteralDataTraits<T>
    {
        static constexpr bool valid_kind(SyntaxKind kind) noexcept
        {
            return kind == T::kind;
        }

        static constexpr const T &get_value(const GreenToken &token) noexcept
        {
            return static_cast<const GreenValueToken<T> &>(token).value();
        }
    };

    template <>
    struct LiteralDataTraits<bool>
    {
        static constexpr bool valid_kind(const SyntaxKind kind) noexcept
        {
            return kind == SyntaxKind::true_keyword || kind == SyntaxKind::false_keyword;
        }

        static constexpr bool get_value(const GreenToken &token) noexcept
        {
            return token.kind() == SyntaxKind::true_keyword;
        }
    };

    template <typename T>
    concept LiteralData = requires(T t, SyntaxKind kind, const GreenToken &token) {
        {
            LiteralDataTraits<T>::valid_kind(kind)
        } -> std::convertible_to<bool>;
        {
            LiteralDataTraits<T>::get_value(token)
        } -> std::convertible_to<T>;
    };
} // namespace prism
