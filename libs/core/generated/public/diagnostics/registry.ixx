export module prism.core:diagnostics.registry;

import :diagnostics.code;
import :diagnostics.descriptor;
import :util.optional;

namespace prism::diagnostics
{
    using namespace std::string_view_literals;
    export constexpr DiagnosticDescriptor unexpected_token{to_string(DiagnosticCode::unexpected_token),
                                                           "Unexpected token"sv,
                                                           "Unexpected token {}",
                                                           "Syntax"sv,
                                                           DiagnosticSeverity::error,
                                                           true,
                                                           ""sv,
                                                           ""sv,
                                                           {}};

    export constexpr DiagnosticDescriptor unexpected_eof{to_string(DiagnosticCode::unexpected_eof),
                                                         "Unexpected EOF"sv,
                                                         "Unexpected end of file",
                                                         "Syntax"sv,
                                                         DiagnosticSeverity::error,
                                                         true,
                                                         ""sv,
                                                         ""sv,
                                                         {}};

    export constexpr DiagnosticDescriptor unexpected_escape{to_string(DiagnosticCode::unexpected_escape),
                                                            "Unexpected escape"sv,
                                                            "Unexpected escape sequence: '{}'",
                                                            "Syntax"sv,
                                                            DiagnosticSeverity::error,
                                                            true,
                                                            ""sv,
                                                            ""sv,
                                                            {}};

    export constexpr DiagnosticDescriptor unterminated_string_literal{
        to_string(DiagnosticCode::unterminated_string_literal),
        "Unterminated string literal"sv,
        "Unterminated string literal",
        "Syntax"sv,
        DiagnosticSeverity::error,
        true,
        ""sv,
        ""sv,
        {}};

    export constexpr DiagnosticDescriptor unterminated_character_literal{
        to_string(DiagnosticCode::unterminated_character_literal),
        "Unterminated character literal"sv,
        "Unterminated character literal",
        "Syntax"sv,
        DiagnosticSeverity::error,
        true,
        ""sv,
        ""sv,
        {}};

    export constexpr DiagnosticDescriptor unterminated_block_comment{
        to_string(DiagnosticCode::unterminated_block_comment),
        "Unterminated block comment"sv,
        "Unterminated block comment",
        "Syntax"sv,
        DiagnosticSeverity::error,
        true,
        ""sv,
        ""sv,
        {}};

    export constexpr DiagnosticDescriptor unresolved_symbol{to_string(DiagnosticCode::unresolved_symbol),
                                                            "Unresolved symbol"sv,
                                                            "Unresolved symbol {}",
                                                            "Semantic"sv,
                                                            DiagnosticSeverity::error,
                                                            true,
                                                            ""sv,
                                                            ""sv,
                                                            {}};

    export constexpr DiagnosticDescriptor invalid_symbol{to_string(DiagnosticCode::invalid_symbol),
                                                         "Invalid symbol"sv,
                                                         "Unexpected symbol {}, expected a {}",
                                                         "Semantic"sv,
                                                         DiagnosticSeverity::error,
                                                         true,
                                                         ""sv,
                                                         ""sv,
                                                         {}};

    export constexpr DiagnosticDescriptor expected_type_specifier{to_string(DiagnosticCode::expected_type_specifier),
                                                                  "Expected type specifier"sv,
                                                                  "Type specifier expected",
                                                                  "Semantic"sv,
                                                                  DiagnosticSeverity::error,
                                                                  true,
                                                                  ""sv,
                                                                  ""sv,
                                                                  {}};

    export constexpr DiagnosticDescriptor ambiguous_symbol{to_string(DiagnosticCode::ambiguous_symbol),
                                                           "Ambiguous symbol"sv,
                                                           "Ambiguous symbol reference {}",
                                                           "Semantic"sv,
                                                           DiagnosticSeverity::error,
                                                           true,
                                                           ""sv,
                                                           ""sv,
                                                           {}};

    export constexpr DiagnosticDescriptor no_conversion{to_string(DiagnosticCode::no_conversion),
                                                        "No conversion"sv,
                                                        "No conversion exists between type {} and type {}",
                                                        "Semantic"sv,
                                                        DiagnosticSeverity::error,
                                                        true,
                                                        ""sv,
                                                        ""sv,
                                                        {}};

    export constexpr DiagnosticDescriptor conversion_is_explicit{to_string(DiagnosticCode::conversion_is_explicit),
                                                                 "Conversion is explicit"sv,
                                                                 "Conversion from {} to type {} is explicit",
                                                                 "Semantic"sv,
                                                                 DiagnosticSeverity::error,
                                                                 true,
                                                                 ""sv,
                                                                 ""sv,
                                                                 {}};

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
            case DiagnosticCode::unresolved_symbol:
                return unresolved_symbol;
            case DiagnosticCode::invalid_symbol:
                return invalid_symbol;
            case DiagnosticCode::expected_type_specifier:
                return expected_type_specifier;
            case DiagnosticCode::ambiguous_symbol:
                return ambiguous_symbol;
            case DiagnosticCode::no_conversion:
                return no_conversion;
            case DiagnosticCode::conversion_is_explicit:
                return conversion_is_explicit;
            default:
                return std::nullopt;
        }
    }
} // namespace prism::diagnostics
