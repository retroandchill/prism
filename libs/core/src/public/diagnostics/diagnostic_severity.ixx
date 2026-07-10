/**
 * @file diagnostic_severity.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core:diagnotics.diagnostic_severity;

import std;

namespace prism
{
    export enum class DiagnosticSeverity : std::uint8_t
    {
        error,
        warning,
        info,
        hint,
    };
} // namespace prism
