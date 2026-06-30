/**
 * @file diagnostic.ixx
 * @author Francesco Corso
 * @date 6/26/2026
 * @brief
 */
export module prism.core.diagnostic;

import std;
import prism.core.source.source_file;
import prism.core.util;
import prism.core.lexer.token;

namespace prism
{
    export enum class DiagnosticSeverity : std::uint8_t
    {
        error,
        warning,
        info,
        hint,
    };

    export struct DiagnosticDescriptor
    {
        std::string_view id;
        std::string_view title;
        std::string_view description;
        DiagnosticSeverity severity = DiagnosticSeverity::error;
        bool enabled_by_default = true;
        std::string_view message_format;
    };

    export struct Diagnostic
    {
        std::reference_wrapper<const DiagnosticDescriptor> descriptor;
        SourceRange range;
        std::string message;
    };

    export class DiagnosticSink
    {
      public:
        [[nodiscard]] constexpr const std::vector<Diagnostic> &diagnostics() const noexcept
        {
            return diagnostics_;
        }

        template <typename... GivenArgs>
        constexpr void report(const DiagnosticDescriptor descriptor, SourceRange range, GivenArgs &&...args)
        {
            diagnostics_.emplace_back(descriptor,
                                      range,
                                      std::vformat(descriptor.message_format, std::make_format_args(args...)));
        }

      private:
        std::vector<Diagnostic> diagnostics_;
    };

    export constexpr DiagnosticDescriptor empty_statement{.id = "STX001",
                                                          .title = "Empty Statement",
                                                          .severity = DiagnosticSeverity::warning,
                                                          .message_format = "Empty statement"};

    export constexpr DiagnosticDescriptor unexpected_token{.id = "STX002",
                                                           .title = "Unexpected Token",
                                                           .severity = DiagnosticSeverity::error,
                                                           .message_format = "Unexpected token {}: expected {:n}"};
} // namespace prism
