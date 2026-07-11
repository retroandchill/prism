/**
 * @file numerics.ixx
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
export module prism.core:util.numerics;

import boost;
import std;

namespace prism
{
    export using BigInteger = boost::multiprecision::cpp_int;
    export using BigDecimal = boost::multiprecision::cpp_dec_float_50;

    export constexpr bool is_hex_digit(const char32_t c) noexcept
    {
        return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
    }

    export constexpr std::uint32_t hex_digit_value(const char32_t c) noexcept
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
} // namespace prism
