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

    std::shared_ptr<const SyntaxDiagnosticInfo> SyntaxDiagnosticInfo::with_offset(uint32_t offset) const
    {
        if (offset == offset_)
            return std::static_pointer_cast<const SyntaxDiagnosticInfo>(shared_from_this());

        return std::make_shared<const SyntaxDiagnosticInfo>(offset, width_, descriptor(), arguments());
    }

    std::shared_ptr<const DiagnosticInfo> SyntaxDiagnosticInfo::clone_with_severity(DiagnosticSeverity severity) const
    {
        return std::make_shared<SyntaxDiagnosticInfo>(clone_tag, *this, severity);
    }
} // namespace prism
