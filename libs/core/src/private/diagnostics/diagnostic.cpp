/**
 * @file diagnostic.cpp
 * @author Francesco Corso
 * @date 8/3/2026
 * @brief
 */
module prism.core:diagnostics.diagnostic.impl;

import :diagnostics.diagnostic;

namespace prism
{

    bool Diagnostic::is_warning_as_error() const
    {
        return default_severity() == DiagnosticSeverity::warning && severity() == DiagnosticSeverity::error;
    }
} // namespace prism
