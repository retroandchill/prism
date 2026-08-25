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
        symbol_inaccessible = 2004,
        no_conversion = 2005,
        conversion_is_explicit = 2006,
        multiple_file_scoped_namespaces = 2007,
        file_scoped_and_normal_namespace = 2008,
        file_scoped_namespace_not_before_all_members = 2009,
        bad_modifiers_on_namespace = 2010,
        literal_value_too_big = 2011,
        unary_operator_undefined = 2012,
        cannot_assign_expression = 2013,
        no_compound_assignment_operator = 2014,
        binary_operator_undefined = 2015,
        no_overload_matching_arg_count = 2016,
        no_overload_for_arg_types = 2017,
        no_call_operator_defined = 2018,
        no_entry_point_defined = 2019,
        ambiguous_entry_point = 2020,
        invalid_entry_point_return_type = 2021,
        invalid_entry_point_parameters = 2022,
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
            case DiagnosticCode::symbol_inaccessible:
                return "E2004";
            case DiagnosticCode::no_conversion:
                return "E2005";
            case DiagnosticCode::conversion_is_explicit:
                return "E2006";
            case DiagnosticCode::multiple_file_scoped_namespaces:
                return "E2007";
            case DiagnosticCode::file_scoped_and_normal_namespace:
                return "E2008";
            case DiagnosticCode::file_scoped_namespace_not_before_all_members:
                return "E2009";
            case DiagnosticCode::bad_modifiers_on_namespace:
                return "E2010";
            case DiagnosticCode::literal_value_too_big:
                return "E2011";
            case DiagnosticCode::unary_operator_undefined:
                return "E2012";
            case DiagnosticCode::cannot_assign_expression:
                return "E2013";
            case DiagnosticCode::no_compound_assignment_operator:
                return "E2014";
            case DiagnosticCode::binary_operator_undefined:
                return "E2015";
            case DiagnosticCode::no_overload_matching_arg_count:
                return "E2016";
            case DiagnosticCode::no_overload_for_arg_types:
                return "E2017";
            case DiagnosticCode::no_call_operator_defined:
                return "E2018";
            case DiagnosticCode::no_entry_point_defined:
                return "E2019";
            case DiagnosticCode::ambiguous_entry_point:
                return "E2020";
            case DiagnosticCode::invalid_entry_point_return_type:
                return "E2021";
            case DiagnosticCode::invalid_entry_point_parameters:
                return "E2022";
            default:
                throw std::invalid_argument{"Invalid diagnostic code"};
        }
    }
} // namespace prism
