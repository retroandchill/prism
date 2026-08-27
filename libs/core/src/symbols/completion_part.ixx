/**
 * @file completion_part.ixx
 * @author Francesco Corso
 * @date 8/27/2026
 * @brief
 */

export module prism.core:symbols.completion_part;

import std;
import :util.enum_class_flags;

namespace prism
{
    enum class CompletionPart : std::uint32_t
    {
        none = 0,

        // common
        attributes = 1 << 0,
        parameters = 1 << 1,
        type = 1 << 2,
        type_parameters = 1 << 3,

        // explicit staged graph work
        base_types_started = 1 << 5,
        base_types_completed = 1 << 6,

        // optional later if/when needed
        interfaces_started = 1 << 7,
        interfaces_completed = 1 << 8,

        // single-shot published/computed phases
        members = 1 << 9,
        constant_value = 1 << 10,
        start_checks = 1 << 11,
        finish_checks = 1 << 12,

        // terminal bit
        members_completed = 1 << 13,

        all = (1 << 14) - 1,

        assembly_all = members,
        namespace_all = members | members_completed,
        variable_all = type | constant_value,
        function_all = parameters | type | start_checks | finish_checks,
        parameter_all = type
    };

    template <>
    constexpr bool is_flag_enum<CompletionPart> = true;
} // namespace prism
