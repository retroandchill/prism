/**
 * @file util.ixx
 * @author Francesco Corso
 * @date 6/26/2026
 * @brief
 */
export module prism.core.util;

import std;
import boost.flyweight;

namespace prism
{
    export template <typename T>
    concept LValueReference = std::is_lvalue_reference_v<T>;

    export template <typename T>
    concept Nullable = requires(const T &value) {
        {
            value == nullptr
        } -> std::convertible_to<bool>;
        {
            *value
        } -> LValueReference;
    };

    export template <Nullable T>
    using ReferenceType = decltype(*std::declval<T>());

    export template <Nullable T1, Nullable T2 = T1>
        requires std::equality_comparable_with<ReferenceType<T1>, ReferenceType<T2>>
    constexpr bool values_equal(const T1 &lhs, const T2 &rhs) noexcept
    {
        if (lhs == nullptr)
        {
            return rhs == nullptr;
        }

        return rhs != nullptr && *lhs == *rhs;
    }

    export using SharedString = boost::flyweights::flyweight<std::string>;

    export template <typename T>
        requires std::is_scoped_enum_v<T>
    constexpr bool is_flag_enum = false;

    export template <typename T>
        requires std::is_scoped_enum_v<T> && is_flag_enum<T>
    constexpr T operator|(T lhs, T rhs)
    {
        return static_cast<T>(static_cast<std::underlying_type_t<T>>(lhs) |
                              static_cast<std::underlying_type_t<T>>(rhs));
    }

    export template <typename T>
        requires std::is_scoped_enum_v<T> && is_flag_enum<T>
    constexpr T &operator|=(T &lhs, T rhs)
    {
        lhs = lhs | rhs;
        return lhs;
    }

    export template <typename T>
        requires std::is_scoped_enum_v<T> && is_flag_enum<T>
    constexpr T operator&(T lhs, T rhs)
    {
        return static_cast<T>(static_cast<std::underlying_type_t<T>>(lhs) &
                              static_cast<std::underlying_type_t<T>>(rhs));
    }

    export template <typename T>
        requires std::is_scoped_enum_v<T> && is_flag_enum<T>
    constexpr T &operator&=(T &lhs, T rhs)
    {
        lhs = lhs & rhs;
        return lhs;
    }

    export template <typename T>
        requires std::is_scoped_enum_v<T> && is_flag_enum<T>
    constexpr T operator^(T lhs, T rhs)
    {
        return static_cast<T>(static_cast<std::underlying_type_t<T>>(lhs) ^
                              static_cast<std::underlying_type_t<T>>(rhs));
    }

    export template <typename T>
        requires std::is_scoped_enum_v<T> && is_flag_enum<T>
    constexpr T &operator^=(T &lhs, T rhs)
    {
        lhs = lhs ^ rhs;
        return lhs;
    }

    export template <typename T>
        requires std::is_scoped_enum_v<T> && is_flag_enum<T>
    constexpr T operator~(T value)
    {
        return static_cast<T>(~static_cast<std::underlying_type_t<T>>(value));
    }

    export template <typename T>
    concept FlagEnum = std::is_scoped_enum_v<T> && requires(T &lhs, T rhs) {
        {
            lhs | rhs
        } -> std::convertible_to<T>;
        {
            lhs |= rhs
        } -> std::same_as<T &>;
        {
            lhs &rhs
        } -> std::convertible_to<T>;
        {
            lhs &= rhs
        } -> std::same_as<T &>;
        {
            lhs ^ rhs
        } -> std::convertible_to<T>;
        {
            lhs ^= rhs
        } -> std::same_as<T &>;
        {
            ~lhs
        } -> std::convertible_to<T>;
    };

    export template <FlagEnum T>
    constexpr bool has_all_flags(T enum_value, T flags)
    {
        return (enum_value & flags) == flags;
    }

    export template <FlagEnum T>
    constexpr bool has_any_flags(T enum_value, T flags)
    {
        return (enum_value & flags) != static_cast<T>(0);
    }
} // namespace prism
