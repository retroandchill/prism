/**
 * @file syntax_info.cpp
 * @author Francesco Corso
 * @date 7/25/2026
 * @brief
 */
module prism.core:diagnostics.syntax_info.impl;

import :diagnostics.syntax_info;

namespace prism
{

    RefCountPtr<const SyntaxDiagnosticInfo> SyntaxDiagnosticInfo::with_offset(uint32_t offset) const
    {
        if (offset == offset_)
            return shared_from_this();

        return make_ref_counted<const SyntaxDiagnosticInfo>(offset, width_, code(), arguments());
    }

    RefCountPtr<const DiagnosticInfo> SyntaxDiagnosticInfo::clone_with_severity(DiagnosticSeverity severity) const
    {
        return make_ref_counted<SyntaxDiagnosticInfo>(clone_tag, *this, severity);
    }
} // namespace prism
