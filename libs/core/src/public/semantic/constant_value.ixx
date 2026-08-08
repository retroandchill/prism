/**
 * @file constant_value.ixx
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
export module prism.core:semantic.constant_value;

import std;
import :util.numerics;
import :syntax.literals;

namespace prism
{
    using ConstantValue =
        std::variant<std::nullptr_t, bool, NumericLiteralData, CharacterLiteralData, StringLiteralView>;
}
