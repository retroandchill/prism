/**
 * @file numerics.ixx
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:util.numerics;

import libassert;
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

    [[nodiscard]] constexpr std::int32_t count_decimal_digits(BigInteger value)
    {
        DEBUG_ASSERT(value >= 0);

        std::int32_t digits = 1;
        while (value >= 10)
        {
            value /= 10;
            ++digits;
        }

        return digits;
    }

    [[nodiscard]] constexpr BigInteger pow_bigint(std::uint32_t base, std::uint32_t exponent)
    {
        BigInteger result = 1;
        BigInteger factor = base;

        while (exponent > 0)
        {
            if ((exponent & 1u) != 0)
                result *= factor;

            exponent >>= 1;
            if (exponent != 0)
                factor *= factor;
        }

        return result;
    }

    [[nodiscard]] constexpr BigInteger shift_left(BigInteger value, const std::uint32_t amount)
    {
        value <<= amount;
        return value;
    }

    template <std::floating_point T>
    bool fits_in_finite_float_magnitude(const BigInteger &significand, std::int32_t exponent10)
    {
        static_assert(std::numeric_limits<T>::is_specialized);
        static_assert(std::numeric_limits<T>::radix == 2);

        if (significand == 0)
            return true;

        const std::int32_t literal_exponent10 = count_decimal_digits(significand) + exponent10 - 1;
        if (literal_exponent10 < std::numeric_limits<T>::max_exponent10)
            return true;

        if (literal_exponent10 > std::numeric_limits<T>::max_exponent10)
            return false;

        constexpr auto precision_bits = static_cast<std::uint32_t>(std::numeric_limits<T>::digits);
        constexpr auto max_exponent = std::numeric_limits<T>::max_exponent;

        const auto max_significand = shift_left(BigInteger{1}, precision_bits) - 1;

        if (exponent10 >= 0)
        {
            const auto exponent10_u = static_cast<std::uint32_t>(exponent10);
            auto lhs = significand * pow_bigint(5, exponent10_u);

            const auto binary_shift = max_exponent - static_cast<std::int32_t>(precision_bits) - exponent10;
            if (binary_shift >= 0)
            {
                const auto rhs = shift_left(max_significand, static_cast<std::uint32_t>(binary_shift));
                return lhs <= rhs;
            }

            lhs = shift_left(std::move(lhs), static_cast<std::uint32_t>(-binary_shift));
            return lhs <= max_significand;
        }

        const auto negative_exponent10 = static_cast<std::uint32_t>(-exponent10);
        BigInteger rhs = max_significand * pow_bigint(5, negative_exponent10);

        const auto binary_shift =
            max_exponent - static_cast<std::int32_t>(precision_bits) + static_cast<std::int32_t>(negative_exponent10);
        DEBUG_ASSERT(binary_shift >= 0);

        rhs = shift_left(std::move(rhs), static_cast<std::uint32_t>(binary_shift));
        return significand <= rhs;
    }

    template <std::floating_point T>
    [[nodiscard]] T parse_decimal_float(const BigInteger &significand, std::int32_t exponent10, bool is_negative)
    {
        DEBUG_ASSERT(significand >= 0);

        if (significand == 0)
            return T{0};

        const auto digits = significand.str();
        const auto scientific_exponent = exponent10 + static_cast<std::int32_t>(digits.size()) - 1;

        std::string text;
        static constexpr std::size_t padding = 32;
        text.reserve(digits.size() + padding);
        text.push_back(digits.front());
        if (digits.size() > 1)
        {
            text.push_back('.');
            text.append(digits.begin() + 1, digits.end());
        }

        text.push_back('e');
        if (scientific_exponent >= 0)
            text.push_back('+');

        text += std::to_string(scientific_exponent);

        T result{};
        const auto [ptr, ec] =
            std::from_chars(text.data(), text.data() + text.size(), result, std::chars_format::scientific);

        DEBUG_ASSERT(ec == std::errc{});
        DEBUG_ASSERT(ptr == text.data() + text.size());
        DEBUG_ASSERT(std::isfinite(result));

        return is_negative ? -result : result;
    }
} // namespace prism
