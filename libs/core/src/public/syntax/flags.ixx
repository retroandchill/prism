/**
 * @file flags.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core:syntax.flags;

import std;
import :util.enum_class_flags;

namespace prism
{
    enum class SyntaxFlags : std::uint16_t
    {
        none = 0,
        not_missing = 1 << 0,
        contains_diagnostics = 1 << 1,

        inherit_mask = not_missing | contains_diagnostics,
    };

    template <>
    constexpr bool is_flag_enum<SyntaxFlags> = true;
} // namespace prism
