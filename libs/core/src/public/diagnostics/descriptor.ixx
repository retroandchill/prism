/**
 * @file descriptor.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core:diagnostics.descriptor;

import std;
import :diagnotics.diagnostic_severity;

namespace prism
{
    export using DiagnosticCustomTags = std::span<const std::string_view>;

    export class DiagnosticDescriptor
    {
      public:
        constexpr DiagnosticDescriptor(const std::string_view id,
                                       const std::string_view title,
                                       const std::string_view message_format,
                                       const std::string_view category,
                                       const DiagnosticSeverity default_severity,
                                       const bool enabled_by_default,
                                       const std::string_view description = "",
                                       const std::string_view help_link = "",
                                       const DiagnosticCustomTags custom_tags = {}) noexcept
            : id_{id}, title_{title}, format_message_{message_format}, explanation_{description}, help_link_{help_link},
              category_{category}, default_severity_{default_severity}, enabled_by_default_{enabled_by_default},
              tags_{custom_tags}
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

        [[nodiscard]] constexpr std::string_view explanation() const noexcept
        {
            return explanation_;
        }

        [[nodiscard]] constexpr std::string_view help_link() const noexcept
        {
            return help_link_;
        }

        [[nodiscard]] constexpr std::string_view format_message() const noexcept
        {
            return format_message_;
        }

        [[nodiscard]] constexpr std::string_view category() const noexcept
        {
            return category_;
        }

        [[nodiscard]] constexpr DiagnosticSeverity default_severity() const noexcept
        {
            return default_severity_;
        }

        [[nodiscard]] constexpr bool enabled_by_default() const noexcept
        {
            return enabled_by_default_;
        }

        [[nodiscard]] constexpr const DiagnosticCustomTags &tags() const noexcept
        {
            return tags_;
        }

      private:
        std::string_view id_;
        std::string_view title_;
        std::string_view format_message_;
        std::string_view explanation_;
        std::string_view help_link_;
        std::string_view category_;
        DiagnosticSeverity default_severity_ = DiagnosticSeverity::error;
        bool enabled_by_default_ = true;
        DiagnosticCustomTags tags_;
    };
} // namespace prism
