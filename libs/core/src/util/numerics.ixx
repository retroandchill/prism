/**
 * @file numerics.ixx
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
export module prism.core:util.numerics;

import std;
import boost;

namespace prism
{
    export template <std::integral T>
    constexpr T index_none = static_cast<T>(-1);

    export using Int128 = boost::int128::int128_t;
    export using UInt128 = boost::int128::uint128_t;

    export using BigInteger = boost::multiprecision::cpp_int;

    export constexpr bool is_hex_digit(const char c) noexcept
    {
        return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
    }

    export constexpr std::uint32_t hex_digit_value(const char c) noexcept
    {
        if (c >= '0' && c <= '9')
        {
            return c - '0';
        }

        if (c >= 'a' && c <= 'f')
        {
            return c - 'a' + 10;
        }

        if (c >= 'A' && c <= 'F')
        {
            return c - 'A' + 10;
        }

        return 0;
    }

    export constexpr bool is_binary_digit(const char c) noexcept
    {
        return c == '0' || c == '1';
    }

    export template <typename T>
    concept Integral = std::numeric_limits<T>::is_integer;

    template <Integral T>
    [[nodiscard]] constexpr BigInteger to_big_integer(const T value)
    {
        if constexpr (std::integral<T>)
        {
            return BigInteger{value};
        }
        else
        {
            // TODO: This should hopefully get rolled up at compile time
            return BigInteger{std::format("{}", value)};
        }
    }

    template <Integral T>
    [[nodiscard]] constexpr bool fits_in(const BigInteger &value)
    {
        static const auto max = to_big_integer(std::numeric_limits<T>::max());
        static const auto min = to_big_integer(std::numeric_limits<T>::min());
        return value <= max && value >= min;
    }

    template <std::floating_point T>
    [[nodiscard]] constexpr bool fits_in(const BigInteger &value)
    {
        static const auto max = BigInteger{std::numeric_limits<T>::max()};
        static const auto min = BigInteger{std::numeric_limits<T>::min()};
        return value <= max && value >= -min;
    }
} // namespace prism
