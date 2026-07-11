/**
 * @file numerics.ixx
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
export module prism.core:util.numerics;

import boost;

namespace prism
{
    export using BigInteger = boost::multiprecision::cpp_int;
    export using BigDecimal = boost::multiprecision::cpp_dec_float_50;
} // namespace prism
