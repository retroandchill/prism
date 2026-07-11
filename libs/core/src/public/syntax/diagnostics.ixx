/**
 * @file diagnostics.ixx
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
export module prism.core:syntax.diagnostics;

import :diagnostics.diagnostic_info;

namespace prism
{
    constexpr std::string_view syntax_category = "Syntax";

    export constexpr DiagnosticDescriptor unexpected_token{"PRISM0001",
                                                           "Unexpected Token",
                                                           "Unexpected Token '{0}'",
                                                           syntax_category,
                                                           DiagnosticSeverity::error};

    export constexpr DiagnosticDescriptor empty_statement{"PRISM0002",
                                                          "Empty Statement",
                                                          "Empty statement",
                                                          syntax_category,
                                                          DiagnosticSeverity::warning};

    export constexpr DiagnosticDescriptor unexpected_eof{"PRISM0003",
                                                         "Unexpected EOF",
                                                         "Unexpected end of file",
                                                         syntax_category,
                                                         DiagnosticSeverity::error};

    export constexpr DiagnosticDescriptor unexpected_escape{"PRISM0004",
                                                            "Unexpected Escape",
                                                            "Unexpected escape sequence: '{0}'",
                                                            syntax_category,
                                                            DiagnosticSeverity::error};

    export constexpr DiagnosticDescriptor unterminated_string_literal{"PRISM0005",
                                                                      "Unterminated String Literal",
                                                                      "Unterminated string literal",
                                                                      syntax_category,
                                                                      DiagnosticSeverity::error};

    export constexpr DiagnosticDescriptor unterminated_character_literal{"PRISM0005",
                                                                         "Unterminated Character Literal",
                                                                         "Unterminated character literal",
                                                                         syntax_category,
                                                                         DiagnosticSeverity::error};

    export constexpr DiagnosticDescriptor unterminated_block_comment{"PRISM0007",
                                                                     "Unterminated Block Comment",
                                                                     "Unterminated string block comment",
                                                                     syntax_category,
                                                                     DiagnosticSeverity::error};
} // namespace prism
