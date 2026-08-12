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

    export using Float16 = boost::multiprecision::cpp_bin_float_single;

    export using BigInteger = boost::multiprecision::cpp_int;
    export using BigDecimal = boost::multiprecision::cpp_dec_float_50;

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
    [[nodiscard]] constexpr BigDecimal to_big_decimal(const T value)
    {
        if constexpr (std::integral<T>)
        {
            return BigDecimal{value};
        }
        else
        {
            // TODO: This should hopefully get rolled up at compile time
            return BigDecimal{std::format("{}", value)};
        }
    }

    template <Integral T>
    [[nodiscard]] constexpr bool fits_in(const BigDecimal &value)
    {
        static const auto max = to_big_decimal(std::numeric_limits<T>::max());
        static const auto min = to_big_decimal(std::numeric_limits<T>::min());
        return value <= max && value >= min;
    }

    template <std::floating_point T>
    [[nodiscard]] constexpr bool fits_in(const BigDecimal &value)
    {
        static const auto max = BigDecimal{std::numeric_limits<T>::max()};
        static const auto min = BigDecimal{std::numeric_limits<T>::min()};
        return value <= max && value >= -min;
    }

    [[nodiscard]] constexpr bool fits_in_f16(const BigDecimal &value)
    {
        static const BigDecimal max = []
        {
            constexpr int fraction_bits = 10;
            constexpr int exponent_bits = 5;
            constexpr int exponent_bias = (1 << (exponent_bits - 1)) - 1;
            constexpr int max_exponent_field = (1 << exponent_bits) - 2;
            constexpr int max_exponent_unbiased = max_exponent_field - exponent_bias;

            const double significand = 2.0 - std::ldexp(1.0, -fraction_bits);
            const double scale = std::ldexp(1.0, max_exponent_unbiased);
            return BigDecimal{significand * scale};
        }();
        static const BigDecimal min = -max;

        return value >= min && value <= max;
    }
} // namespace prism
