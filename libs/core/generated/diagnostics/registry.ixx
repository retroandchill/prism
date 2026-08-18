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

    export constexpr DiagnosticDescriptor symbol_inaccessible{to_string(DiagnosticCode::symbol_inaccessible),
                                                              "Symbol inaccessible"sv,
                                                              "Symbol {} is inaccessible",
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

    export constexpr DiagnosticDescriptor multiple_file_scoped_namespaces{
        to_string(DiagnosticCode::multiple_file_scoped_namespaces),
        "Multiple file scoped namespaces"sv,
        "Multiple file-scoped namespaces are not allowed",
        "Semantic"sv,
        DiagnosticSeverity::error,
        true,
        ""sv,
        ""sv,
        {}};

    export constexpr DiagnosticDescriptor file_scoped_and_normal_namespace{
        to_string(DiagnosticCode::file_scoped_and_normal_namespace),
        "File scoped and normal namespace"sv,
        "Cannot mix file-scoped and block-scoped namespaces",
        "Semantic"sv,
        DiagnosticSeverity::error,
        true,
        ""sv,
        ""sv,
        {}};

    export constexpr DiagnosticDescriptor file_scoped_namespace_not_before_all_members{
        to_string(DiagnosticCode::file_scoped_namespace_not_before_all_members),
        "File scoped namespace not before all members"sv,
        "File-scoped namespace must be declared before all other members",
        "Semantic"sv,
        DiagnosticSeverity::error,
        true,
        ""sv,
        ""sv,
        {}};

    export constexpr DiagnosticDescriptor bad_modifiers_on_namespace{
        to_string(DiagnosticCode::bad_modifiers_on_namespace),
        "Bad modifiers on namespace"sv,
        "Namespaces cannot have modifiers",
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
            case DiagnosticCode::symbol_inaccessible:
                return symbol_inaccessible;
            case DiagnosticCode::no_conversion:
                return no_conversion;
            case DiagnosticCode::conversion_is_explicit:
                return conversion_is_explicit;
            case DiagnosticCode::multiple_file_scoped_namespaces:
                return multiple_file_scoped_namespaces;
            case DiagnosticCode::file_scoped_and_normal_namespace:
                return file_scoped_and_normal_namespace;
            case DiagnosticCode::file_scoped_namespace_not_before_all_members:
                return file_scoped_namespace_not_before_all_members;
            case DiagnosticCode::bad_modifiers_on_namespace:
                return bad_modifiers_on_namespace;
            default:
                return std::nullopt;
        }
    }
} // namespace prism::diagnostics
