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
    };

    export constexpr std::string_view to_string(const DiagnosticCode code)
    {
        switch (code)
        {
            case DiagnosticCode::unexpected_token:
                return "E10004";
            case DiagnosticCode::unexpected_eof:
                return "E10014";
            case DiagnosticCode::unexpected_escape:
                return "E10024";
            case DiagnosticCode::unterminated_string_literal:
                return "E10034";
            case DiagnosticCode::unterminated_character_literal:
                return "E10044";
            case DiagnosticCode::unterminated_block_comment:
                return "E10054";
            default:
                throw std::invalid_argument{"Invalid diagnostic code"};
        }
    }
} // namespace prism
