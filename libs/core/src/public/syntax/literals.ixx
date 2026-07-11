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
import :util.numerics;

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

    export enum class IntegerBase : std::uint8_t
    {
        decimal,
        hex,
        binary
    };

    export enum class IntegerSuffix : std::uint8_t
    {
        none,
        i8,
        i16,
        i32,
        i64,
        i128,
        iz,
        u8,
        u16,
        u32,
        u64,
        u128,
        uz,
    };

    export struct IntegerLiteralData
    {
        static constexpr auto kind = SyntaxKind::integer_token;

        BigInteger value;
        IntegerBase base;
        IntegerSuffix suffix;
    };

    export enum class FloatSuffix : std::uint8_t
    {
        none,
        f16,
        f32,
        f64
    };

    export struct FloatLiteralData
    {
        static constexpr auto kind = SyntaxKind::float_token;

        BigDecimal value;
        FloatSuffix suffix;
    };

    export enum class CharacterEncoding : std::uint8_t
    {
        utf8,
        utf16,
        utf32,
    };

    export struct CharacterLiteralData
    {
        static constexpr auto kind = SyntaxKind::character_token;

        char32_t value;
        CharacterEncoding encoding;
    };

    export struct StringLiteralData
    {
        static constexpr auto kind = SyntaxKind::string_literal_token;

        std::string value;
        CharacterEncoding encoding;
    };
} // namespace prism
