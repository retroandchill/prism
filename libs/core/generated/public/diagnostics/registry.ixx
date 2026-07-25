export module prism.core:diagnostics.registry;

import :diagnostics.descriptor;
import :util.optional;

namespace prism::diagnostics
{
    export constexpr DiagnosticDescriptor unexpected_token{
        .code = DiagnosticCode::unexpected_token,
        .category = DiagnosticCategory::lexer,
        .default_severity = DiagnosticSeverity::error,
        .id = "E10004",
        .symbol = "UnexpectedToken",
        .title = "Unexpected token",
        .format_message = "Unexpected token {}",
    };

    export constexpr DiagnosticDescriptor unexpected_eof{
        .code = DiagnosticCode::unexpected_eof,
        .category = DiagnosticCategory::lexer,
        .default_severity = DiagnosticSeverity::error,
        .id = "E10014",
        .symbol = "UnexpectedEOF",
        .title = "Unexpected EOF",
        .format_message = "Unexpected end of file",
    };

    export constexpr DiagnosticDescriptor unexpected_escape{
        .code = DiagnosticCode::unexpected_escape,
        .category = DiagnosticCategory::lexer,
        .default_severity = DiagnosticSeverity::error,
        .id = "E10024",
        .symbol = "UnexpectedEscape",
        .title = "Unexpected escape",
        .format_message = "Unexpected escape sequence: '{}'",
    };

    export constexpr DiagnosticDescriptor unterminated_string_literal{
        .code = DiagnosticCode::unterminated_string_literal,
        .category = DiagnosticCategory::lexer,
        .default_severity = DiagnosticSeverity::error,
        .id = "E10034",
        .symbol = "UnterminatedStringLiteral",
        .title = "Unterminated string literal",
        .format_message = "Unterminated string literal",
    };

    export constexpr DiagnosticDescriptor unterminated_character_literal{
        .code = DiagnosticCode::unterminated_character_literal,
        .category = DiagnosticCategory::lexer,
        .default_severity = DiagnosticSeverity::error,
        .id = "E10044",
        .symbol = "UnterminatedCharacterLiteral",
        .title = "Unterminated character literal",
        .format_message = "Unterminated character literal",
    };

    export constexpr DiagnosticDescriptor unterminated_block_comment{
        .code = DiagnosticCode::unterminated_block_comment,
        .category = DiagnosticCategory::lexer,
        .default_severity = DiagnosticSeverity::error,
        .id = "E10054",
        .symbol = "UnterminatedBlockComment",
        .title = "Unterminated block comment",
        .format_message = "Unterminated block comment",
    };

    export [[nodiscard]] constexpr Optional<const DiagnosticDescriptor &> get_descriptor(const DiagnosticCode code)
    {
        switch (code)
        {
            case DiagnosticCode::unexpected_token:
                return unexpected_token;
            case DiagnosticCode::unexpected_eof:
                return unexpected_eof;
            case DiagnosticCode::unexpected_escape:
                return unexpected_escape;
            case DiagnosticCode::unterminated_string_literal:
                return unterminated_string_literal;
            case DiagnosticCode::unterminated_character_literal:
                return unterminated_character_literal;
            case DiagnosticCode::unterminated_block_comment:
                return unterminated_block_comment;
            default:
                return std::nullopt;
        }
    }
} // namespace prism::diagnostics
