/**
 * @file flags.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core.syntax:flags;

import std;
import prism.core.util.enum_class_flags;

namespace prism
{
    enum class SyntaxFlags : std::uint16_t
    {
        none = 0,
        missing = 1 << 0,
        contains_diagnostics = 1 << 1,
    };

    template <>
    constexpr bool is_flag_enum<SyntaxFlags> = true;
} // namespace prism
