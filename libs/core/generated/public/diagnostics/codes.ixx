export module prism.core:diagnostics.code;

import std;

namespace prism
{
    export enum class DiagnosticCategory : std::uint8_t
    {
        general = 0,
        syntax,
    };

    export [[nodiscard]] constexpr std::string_view get_display_name(const DiagnosticCategory category)
    {
        switch (category)
        {
            case DiagnosticCategory::general:
                return "General";
            case DiagnosticCategory::syntax:
                return "Syntax";
            default:
                throw std::invalid_argument("Unknown diagnostic category");
        }
    }

    export enum class DiagnosticCode : std::uint32_t
    {
        unknown = 0,

        // -- Syntax (1000-1999) ---
        unexpected_token = 1000,
        unexpected_eof = 1001,
        unexpected_escape = 1002,
        unterminated_string_literal = 1003,
        unterminated_character_literal = 1004,
        unterminated_block_comment = 1005,
    };
} // namespace prism
