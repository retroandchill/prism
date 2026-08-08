export module prism.core:diagnostics.code;

import std;

namespace prism
{
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

        // -- Semantic (2000-2999) ---
        unresolved_symbol = 2000,
        invalid_symbol = 2001,
        expected_type_specifier = 2002,
        ambiguous_symbol = 2003,
    };

    export constexpr std::string_view to_string(const DiagnosticCode code)
    {
        switch (code)
        {
            case DiagnosticCode::unexpected_token:
                return "E1000";
            case DiagnosticCode::unexpected_eof:
                return "E1001";
            case DiagnosticCode::unexpected_escape:
                return "E1002";
            case DiagnosticCode::unterminated_string_literal:
                return "E1003";
            case DiagnosticCode::unterminated_character_literal:
                return "E1004";
            case DiagnosticCode::unterminated_block_comment:
                return "E1005";
            case DiagnosticCode::unresolved_symbol:
                return "E2000";
            case DiagnosticCode::invalid_symbol:
                return "E2001";
            case DiagnosticCode::expected_type_specifier:
                return "E2002";
            case DiagnosticCode::ambiguous_symbol:
                return "E2003";
            default:
                throw std::invalid_argument{"Invalid diagnostic code"};
        }
    }
} // namespace prism
