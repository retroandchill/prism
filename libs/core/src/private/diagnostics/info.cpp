/**
 * @file info.cpp
 * @author Francesco Corso
 * @date 7/10/2026
 * @brief
 */
module prism.core:diagnostics.diagnostic_info.impl;

import :diagnostics.info;

namespace prism
{

    std::string DiagnosticInfo::get_message() const
    {
        const auto message = get_message(code_);
        if (message.empty())
            return "";

        if (arguments_ == nullptr)
            return std::string{message};

        return arguments_->format(message);
    }

    RefCountPtr<const DiagnosticInfo> DiagnosticInfo::clone_with_severity(DiagnosticSeverity severity) const
    {
        return make_ref_counted<const DiagnosticInfo>(clone_tag, *this, severity);
    }
} // namespace prism
