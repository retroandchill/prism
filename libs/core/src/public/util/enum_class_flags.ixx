/**
 * @file enum_class_flags.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core:util.enum_class_flags;

import std;

namespace prism
{
    export template <typename T>
        requires std::is_scoped_enum_v<T>
    constexpr bool is_flag_enum = false;

    export template <typename T>
        requires is_flag_enum<T>
    constexpr T operator|(T lhs, T rhs)
    {
        return static_cast<T>(static_cast<std::underlying_type_t<T>>(lhs) |
                              static_cast<std::underlying_type_t<T>>(rhs));
    }

    export template <typename T>
        requires is_flag_enum<T>
    constexpr T &operator|=(T &lhs, T rhs)
    {
        lhs = lhs | rhs;
        return lhs;
    }

    export template <typename T>
        requires is_flag_enum<T>
    constexpr T operator&(T lhs, T rhs)
    {
        return static_cast<T>(static_cast<std::underlying_type_t<T>>(lhs) &
                              static_cast<std::underlying_type_t<T>>(rhs));
    }

    export template <typename T>
        requires is_flag_enum<T>
    constexpr T &operator&=(T &lhs, T rhs)
    {
        lhs = lhs & rhs;
        return lhs;
    }

    export template <typename T>
        requires is_flag_enum<T>
    constexpr T operator^(T lhs, T rhs)
    {
        return static_cast<T>(static_cast<std::underlying_type_t<T>>(lhs) ^
                              static_cast<std::underlying_type_t<T>>(rhs));
    }

    export template <typename T>
        requires is_flag_enum<T>
    constexpr T &operator^=(T &lhs, T rhs)
    {
        lhs = lhs ^ rhs;
        return lhs;
    }

    export template <typename T>
        requires is_flag_enum<T>
    constexpr T operator~(T value)
    {
        return static_cast<T>(~static_cast<std::underlying_type_t<T>>(value));
    }

    export template <typename T>
    concept FlagEnum = requires(T &lhs, T rhs) {
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
    constexpr bool has_flag(T enum_value, T flag)
    {
        return (enum_value & flag) == flag;
    }

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
