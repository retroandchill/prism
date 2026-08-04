/**
 * @file info.cpp
 * @author Francesco Corso
 * @date 7/10/2026
 * @brief
 */
module prism.core:diagnostics.diagnostic_info.impl;

import :diagnostics.info;
import :diagnostics.diagnostic;

namespace prism
{

    bool DiagnosticInfo::is_warning_as_error() const noexcept
    {
        return default_severity() == DiagnosticSeverity::warning && severity() == DiagnosticSeverity::error;
    }

    std::string DiagnosticInfo::get_message() const
    {
        const auto message = descriptor_.format_message();
        if (message.empty())
            return "";

        if (arguments_ == nullptr)
            return std::string{message};

        return arguments_->format(message);
    }

    std::string DiagnosticInfo::to_string() const
    {
        std::string result;
        StringWriter writer{result};
        write_message(writer);
        return result;
    }

    std::span<const Location> DiagnosticInfo::additional_locations() const
    {
        return {};
    }

    std::span<const std::string_view> DiagnosticInfo::custom_tags() const
    {
        return descriptor().tags();
    }

    std::shared_ptr<const DiagnosticInfo> DiagnosticInfo::clone_with_severity(DiagnosticSeverity severity) const
    {
        return std::make_shared<const DiagnosticInfo>(clone_tag, *this, severity);
    }
} // namespace prism
