/**
 * @file descriptor.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core:diagnostics.descriptor;

import std;
import :diagnotics.diagnostic_severity;
import :diagnostics.code;

namespace prism
{
    export struct DiagnosticDescriptor
    {
        DiagnosticCode code;
        DiagnosticCategory category;
        DiagnosticSeverity default_severity;

        std::string_view id;
        std::string_view symbol;

        std::string_view title;
        std::string_view format_message;

        std::string_view explanation;

        constexpr std::string_view category_name() const noexcept
        {
            return get_display_name(category);
        }
    };
} // namespace prism
