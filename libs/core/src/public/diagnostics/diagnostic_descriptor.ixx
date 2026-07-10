/**
 * @file diagnostic_descriptor.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core:diagnostics.diagnostic_descriptor;

import std;
import :diagnotics.diagnostic_severity;

namespace prism
{
    export class DiagnosticDescriptor
    {
      public:
        constexpr DiagnosticDescriptor(const std::string_view id,
                                       const std::string_view title,
                                       const std::string_view message_format,
                                       const std::string_view category,
                                       const DiagnosticSeverity severity) noexcept
            : id_{id}, title_{title}, message_format_{message_format}, category_{category}, severity_{severity}
        {
        }

        [[nodiscard]] constexpr std::string_view id() const noexcept
        {
            return id_;
        }

        [[nodiscard]] constexpr std::string_view title() const noexcept
        {
            return title_;
        }

        [[nodiscard]] constexpr std::string_view message_format() const noexcept
        {
            return message_format_;
        }

        [[nodiscard]] constexpr std::string_view category() const noexcept
        {
            return category_;
        }

        [[nodiscard]] constexpr DiagnosticSeverity severity() const noexcept
        {
            return severity_;
        }

      private:
        std::string_view id_;
        std::string_view title_;
        std::string_view message_format_;
        std::string_view category_;
        DiagnosticSeverity severity_;
    };
} // namespace prism
