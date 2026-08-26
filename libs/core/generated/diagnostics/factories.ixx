export module prism.core:diagnostics.factories;

import :diagnostics.registry;
import :syntax.kind;
import :text.name;
import :diagnostics.diagnostic;

namespace prism::diagnostics
{
    namespace info
    {
        constexpr RefCountPtr<DiagnosticInfo> make_unexpected_token(std::string token)
        {
            return make_ref_counted<DiagnosticInfo>(unexpected_token, std::move(token));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_unexpected_token(const DiagnosticSeverity severity,
                                                                    std::string token)
        {
            return make_ref_counted<DiagnosticInfo>(unexpected_token, severity, std::move(token));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_unexpected_eof()
        {
            return make_ref_counted<DiagnosticInfo>(unexpected_eof);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_unexpected_eof(const DiagnosticSeverity severity)
        {
            return make_ref_counted<DiagnosticInfo>(unexpected_eof, severity);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_unexpected_escape(std::string sequence)
        {
            return make_ref_counted<DiagnosticInfo>(unexpected_escape, std::move(sequence));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_unexpected_escape(const DiagnosticSeverity severity,
                                                                     std::string sequence)
        {
            return make_ref_counted<DiagnosticInfo>(unexpected_escape, severity, std::move(sequence));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_unterminated_string_literal()
        {
            return make_ref_counted<DiagnosticInfo>(unterminated_string_literal);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_unterminated_string_literal(const DiagnosticSeverity severity)
        {
            return make_ref_counted<DiagnosticInfo>(unterminated_string_literal, severity);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_unterminated_character_literal()
        {
            return make_ref_counted<DiagnosticInfo>(unterminated_character_literal);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_unterminated_character_literal(const DiagnosticSeverity severity)
        {
            return make_ref_counted<DiagnosticInfo>(unterminated_character_literal, severity);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_unterminated_block_comment()
        {
            return make_ref_counted<DiagnosticInfo>(unterminated_block_comment);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_unterminated_block_comment(const DiagnosticSeverity severity)
        {
            return make_ref_counted<DiagnosticInfo>(unterminated_block_comment, severity);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_unresolved_symbol(Name symbol)
        {
            return make_ref_counted<DiagnosticInfo>(unresolved_symbol, symbol);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_unresolved_symbol(const DiagnosticSeverity severity, Name symbol)
        {
            return make_ref_counted<DiagnosticInfo>(unresolved_symbol, severity, symbol);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_invalid_symbol(Name symbol, std::string expected)
        {
            return make_ref_counted<DiagnosticInfo>(invalid_symbol, symbol, std::move(expected));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_invalid_symbol(const DiagnosticSeverity severity,
                                                                  Name symbol,
                                                                  std::string expected)
        {
            return make_ref_counted<DiagnosticInfo>(invalid_symbol, severity, symbol, std::move(expected));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_expected_type_specifier()
        {
            return make_ref_counted<DiagnosticInfo>(expected_type_specifier);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_expected_type_specifier(const DiagnosticSeverity severity)
        {
            return make_ref_counted<DiagnosticInfo>(expected_type_specifier, severity);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_ambiguous_symbol(Name symbol)
        {
            return make_ref_counted<DiagnosticInfo>(ambiguous_symbol, symbol);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_ambiguous_symbol(const DiagnosticSeverity severity, Name symbol)
        {
            return make_ref_counted<DiagnosticInfo>(ambiguous_symbol, severity, symbol);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_symbol_inaccessible(Name symbol)
        {
            return make_ref_counted<DiagnosticInfo>(symbol_inaccessible, symbol);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_symbol_inaccessible(const DiagnosticSeverity severity, Name symbol)
        {
            return make_ref_counted<DiagnosticInfo>(symbol_inaccessible, severity, symbol);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_no_conversion(std::string from_type, std::string to_type)
        {
            return make_ref_counted<DiagnosticInfo>(no_conversion, std::move(from_type), std::move(to_type));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_no_conversion(const DiagnosticSeverity severity,
                                                                 std::string from_type,
                                                                 std::string to_type)
        {
            return make_ref_counted<DiagnosticInfo>(no_conversion, severity, std::move(from_type), std::move(to_type));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_conversion_is_explicit(std::string from_type, std::string to_type)
        {
            return make_ref_counted<DiagnosticInfo>(conversion_is_explicit, std::move(from_type), std::move(to_type));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_conversion_is_explicit(const DiagnosticSeverity severity,
                                                                          std::string from_type,
                                                                          std::string to_type)
        {
            return make_ref_counted<DiagnosticInfo>(conversion_is_explicit,
                                                    severity,
                                                    std::move(from_type),
                                                    std::move(to_type));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_multiple_file_scoped_namespaces()
        {
            return make_ref_counted<DiagnosticInfo>(multiple_file_scoped_namespaces);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_multiple_file_scoped_namespaces(const DiagnosticSeverity severity)
        {
            return make_ref_counted<DiagnosticInfo>(multiple_file_scoped_namespaces, severity);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_file_scoped_and_normal_namespace()
        {
            return make_ref_counted<DiagnosticInfo>(file_scoped_and_normal_namespace);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_file_scoped_and_normal_namespace(const DiagnosticSeverity severity)
        {
            return make_ref_counted<DiagnosticInfo>(file_scoped_and_normal_namespace, severity);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_file_scoped_namespace_not_before_all_members()
        {
            return make_ref_counted<DiagnosticInfo>(file_scoped_namespace_not_before_all_members);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_file_scoped_namespace_not_before_all_members(
            const DiagnosticSeverity severity)
        {
            return make_ref_counted<DiagnosticInfo>(file_scoped_namespace_not_before_all_members, severity);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_bad_modifiers_on_namespace()
        {
            return make_ref_counted<DiagnosticInfo>(bad_modifiers_on_namespace);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_bad_modifiers_on_namespace(const DiagnosticSeverity severity)
        {
            return make_ref_counted<DiagnosticInfo>(bad_modifiers_on_namespace, severity);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_literal_value_too_big()
        {
            return make_ref_counted<DiagnosticInfo>(literal_value_too_big);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_literal_value_too_big(const DiagnosticSeverity severity)
        {
            return make_ref_counted<DiagnosticInfo>(literal_value_too_big, severity);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_unary_operator_undefined(std::string type)
        {
            return make_ref_counted<DiagnosticInfo>(unary_operator_undefined, std::move(type));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_unary_operator_undefined(const DiagnosticSeverity severity,
                                                                            std::string type)
        {
            return make_ref_counted<DiagnosticInfo>(unary_operator_undefined, severity, std::move(type));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_cannot_assign_expression()
        {
            return make_ref_counted<DiagnosticInfo>(cannot_assign_expression);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_cannot_assign_expression(const DiagnosticSeverity severity)
        {
            return make_ref_counted<DiagnosticInfo>(cannot_assign_expression, severity);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_no_compound_assignment_operator(std::string type)
        {
            return make_ref_counted<DiagnosticInfo>(no_compound_assignment_operator, std::move(type));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_no_compound_assignment_operator(const DiagnosticSeverity severity,
                                                                                   std::string type)
        {
            return make_ref_counted<DiagnosticInfo>(no_compound_assignment_operator, severity, std::move(type));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_binary_operator_undefined(std::string type1, std::string type2)
        {
            return make_ref_counted<DiagnosticInfo>(binary_operator_undefined, std::move(type1), std::move(type2));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_binary_operator_undefined(const DiagnosticSeverity severity,
                                                                             std::string type1,
                                                                             std::string type2)
        {
            return make_ref_counted<DiagnosticInfo>(binary_operator_undefined,
                                                    severity,
                                                    std::move(type1),
                                                    std::move(type2));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_no_overload_matching_arg_count(std::size_t arg_count)
        {
            return make_ref_counted<DiagnosticInfo>(no_overload_matching_arg_count, arg_count);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_no_overload_matching_arg_count(const DiagnosticSeverity severity,
                                                                                  std::size_t arg_count)
        {
            return make_ref_counted<DiagnosticInfo>(no_overload_matching_arg_count, severity, arg_count);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_no_overload_for_arg_types(std::string arg_types)
        {
            return make_ref_counted<DiagnosticInfo>(no_overload_for_arg_types, std::move(arg_types));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_no_overload_for_arg_types(const DiagnosticSeverity severity,
                                                                             std::string arg_types)
        {
            return make_ref_counted<DiagnosticInfo>(no_overload_for_arg_types, severity, std::move(arg_types));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_no_call_operator_defined(std::string type)
        {
            return make_ref_counted<DiagnosticInfo>(no_call_operator_defined, std::move(type));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_no_call_operator_defined(const DiagnosticSeverity severity,
                                                                            std::string type)
        {
            return make_ref_counted<DiagnosticInfo>(no_call_operator_defined, severity, std::move(type));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_no_entry_point_defined()
        {
            return make_ref_counted<DiagnosticInfo>(no_entry_point_defined);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_no_entry_point_defined(const DiagnosticSeverity severity)
        {
            return make_ref_counted<DiagnosticInfo>(no_entry_point_defined, severity);
        }

        constexpr RefCountPtr<DiagnosticInfo> make_ambiguous_entry_point(std::string names)
        {
            return make_ref_counted<DiagnosticInfo>(ambiguous_entry_point, std::move(names));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_ambiguous_entry_point(const DiagnosticSeverity severity,
                                                                         std::string names)
        {
            return make_ref_counted<DiagnosticInfo>(ambiguous_entry_point, severity, std::move(names));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_invalid_entry_point_return_type(std::string type, std::string name)
        {
            return make_ref_counted<DiagnosticInfo>(invalid_entry_point_return_type, std::move(type), std::move(name));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_invalid_entry_point_return_type(const DiagnosticSeverity severity,
                                                                                   std::string type,
                                                                                   std::string name)
        {
            return make_ref_counted<DiagnosticInfo>(invalid_entry_point_return_type,
                                                    severity,
                                                    std::move(type),
                                                    std::move(name));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_invalid_entry_point_parameters(std::string types, std::string name)
        {
            return make_ref_counted<DiagnosticInfo>(invalid_entry_point_parameters, std::move(types), std::move(name));
        }

        constexpr RefCountPtr<DiagnosticInfo> make_invalid_entry_point_parameters(const DiagnosticSeverity severity,
                                                                                  std::string types,
                                                                                  std::string name)
        {
            return make_ref_counted<DiagnosticInfo>(invalid_entry_point_parameters,
                                                    severity,
                                                    std::move(types),
                                                    std::move(name));
        }
    } // namespace info

    export constexpr Diagnostic make_unexpected_token(Location location, std::string token)
    {
        return Diagnostic{unexpected_token, std::move(location), std::move(token)};
    }

    export constexpr Diagnostic make_unexpected_token(const DiagnosticSeverity severity,
                                                      Location location,
                                                      std::string token)
    {
        return Diagnostic{unexpected_token, severity, std::move(location), std::move(token)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unexpected_token(Location location, Range &&additional_locations, std::string token)
    {
        return Diagnostic{unexpected_token,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(token)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unexpected_token(const DiagnosticSeverity severity,
                                               Location location,
                                               Range &&additional_locations,
                                               std::string token)
    {
        return Diagnostic{unexpected_token,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(token)};
    }

    export constexpr Diagnostic make_unexpected_eof(Location location)
    {
        return Diagnostic{unexpected_eof, std::move(location)};
    }

    export constexpr Diagnostic make_unexpected_eof(const DiagnosticSeverity severity, Location location)
    {
        return Diagnostic{unexpected_eof, severity, std::move(location)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unexpected_eof(Location location, Range &&additional_locations)
    {
        return Diagnostic{unexpected_eof, std::move(location), std::forward<Range>(additional_locations)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unexpected_eof(const DiagnosticSeverity severity,
                                             Location location,
                                             Range &&additional_locations)
    {
        return Diagnostic{unexpected_eof, severity, std::move(location), std::forward<Range>(additional_locations)};
    }

    export constexpr Diagnostic make_unexpected_escape(Location location, std::string sequence)
    {
        return Diagnostic{unexpected_escape, std::move(location), std::move(sequence)};
    }

    export constexpr Diagnostic make_unexpected_escape(const DiagnosticSeverity severity,
                                                       Location location,
                                                       std::string sequence)
    {
        return Diagnostic{unexpected_escape, severity, std::move(location), std::move(sequence)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unexpected_escape(Location location, Range &&additional_locations, std::string sequence)
    {
        return Diagnostic{unexpected_escape,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(sequence)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unexpected_escape(const DiagnosticSeverity severity,
                                                Location location,
                                                Range &&additional_locations,
                                                std::string sequence)
    {
        return Diagnostic{unexpected_escape,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(sequence)};
    }

    export constexpr Diagnostic make_unterminated_string_literal(Location location)
    {
        return Diagnostic{unterminated_string_literal, std::move(location)};
    }

    export constexpr Diagnostic make_unterminated_string_literal(const DiagnosticSeverity severity, Location location)
    {
        return Diagnostic{unterminated_string_literal, severity, std::move(location)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unterminated_string_literal(Location location, Range &&additional_locations)
    {
        return Diagnostic{unterminated_string_literal, std::move(location), std::forward<Range>(additional_locations)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unterminated_string_literal(const DiagnosticSeverity severity,
                                                          Location location,
                                                          Range &&additional_locations)
    {
        return Diagnostic{unterminated_string_literal,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations)};
    }

    export constexpr Diagnostic make_unterminated_character_literal(Location location)
    {
        return Diagnostic{unterminated_character_literal, std::move(location)};
    }

    export constexpr Diagnostic make_unterminated_character_literal(const DiagnosticSeverity severity,
                                                                    Location location)
    {
        return Diagnostic{unterminated_character_literal, severity, std::move(location)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unterminated_character_literal(Location location, Range &&additional_locations)
    {
        return Diagnostic{unterminated_character_literal,
                          std::move(location),
                          std::forward<Range>(additional_locations)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unterminated_character_literal(const DiagnosticSeverity severity,
                                                             Location location,
                                                             Range &&additional_locations)
    {
        return Diagnostic{unterminated_character_literal,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations)};
    }

    export constexpr Diagnostic make_unterminated_block_comment(Location location)
    {
        return Diagnostic{unterminated_block_comment, std::move(location)};
    }

    export constexpr Diagnostic make_unterminated_block_comment(const DiagnosticSeverity severity, Location location)
    {
        return Diagnostic{unterminated_block_comment, severity, std::move(location)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unterminated_block_comment(Location location, Range &&additional_locations)
    {
        return Diagnostic{unterminated_block_comment, std::move(location), std::forward<Range>(additional_locations)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unterminated_block_comment(const DiagnosticSeverity severity,
                                                         Location location,
                                                         Range &&additional_locations)
    {
        return Diagnostic{unterminated_block_comment,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations)};
    }

    export constexpr Diagnostic make_unresolved_symbol(Location location, Name symbol)
    {
        return Diagnostic{unresolved_symbol, std::move(location), symbol};
    }

    export constexpr Diagnostic make_unresolved_symbol(const DiagnosticSeverity severity,
                                                       Location location,
                                                       Name symbol)
    {
        return Diagnostic{unresolved_symbol, severity, std::move(location), symbol};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unresolved_symbol(Location location, Range &&additional_locations, Name symbol)
    {
        return Diagnostic{unresolved_symbol, std::move(location), std::forward<Range>(additional_locations), symbol};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unresolved_symbol(const DiagnosticSeverity severity,
                                                Location location,
                                                Range &&additional_locations,
                                                Name symbol)
    {
        return Diagnostic{unresolved_symbol,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          symbol};
    }

    export constexpr Diagnostic make_invalid_symbol(Location location, Name symbol, std::string expected)
    {
        return Diagnostic{invalid_symbol, std::move(location), symbol, std::move(expected)};
    }

    export constexpr Diagnostic make_invalid_symbol(const DiagnosticSeverity severity,
                                                    Location location,
                                                    Name symbol,
                                                    std::string expected)
    {
        return Diagnostic{invalid_symbol, severity, std::move(location), symbol, std::move(expected)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_invalid_symbol(Location location,
                                             Range &&additional_locations,
                                             Name symbol,
                                             std::string expected)
    {
        return Diagnostic{invalid_symbol,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          symbol,
                          std::move(expected)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_invalid_symbol(const DiagnosticSeverity severity,
                                             Location location,
                                             Range &&additional_locations,
                                             Name symbol,
                                             std::string expected)
    {
        return Diagnostic{invalid_symbol,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          symbol,
                          std::move(expected)};
    }

    export constexpr Diagnostic make_expected_type_specifier(Location location)
    {
        return Diagnostic{expected_type_specifier, std::move(location)};
    }

    export constexpr Diagnostic make_expected_type_specifier(const DiagnosticSeverity severity, Location location)
    {
        return Diagnostic{expected_type_specifier, severity, std::move(location)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_expected_type_specifier(Location location, Range &&additional_locations)
    {
        return Diagnostic{expected_type_specifier, std::move(location), std::forward<Range>(additional_locations)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_expected_type_specifier(const DiagnosticSeverity severity,
                                                      Location location,
                                                      Range &&additional_locations)
    {
        return Diagnostic{expected_type_specifier,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations)};
    }

    export constexpr Diagnostic make_ambiguous_symbol(Location location, Name symbol)
    {
        return Diagnostic{ambiguous_symbol, std::move(location), symbol};
    }

    export constexpr Diagnostic make_ambiguous_symbol(const DiagnosticSeverity severity, Location location, Name symbol)
    {
        return Diagnostic{ambiguous_symbol, severity, std::move(location), symbol};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_ambiguous_symbol(Location location, Range &&additional_locations, Name symbol)
    {
        return Diagnostic{ambiguous_symbol, std::move(location), std::forward<Range>(additional_locations), symbol};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_ambiguous_symbol(const DiagnosticSeverity severity,
                                               Location location,
                                               Range &&additional_locations,
                                               Name symbol)
    {
        return Diagnostic{ambiguous_symbol,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          symbol};
    }

    export constexpr Diagnostic make_symbol_inaccessible(Location location, Name symbol)
    {
        return Diagnostic{symbol_inaccessible, std::move(location), symbol};
    }

    export constexpr Diagnostic make_symbol_inaccessible(const DiagnosticSeverity severity,
                                                         Location location,
                                                         Name symbol)
    {
        return Diagnostic{symbol_inaccessible, severity, std::move(location), symbol};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_symbol_inaccessible(Location location, Range &&additional_locations, Name symbol)
    {
        return Diagnostic{symbol_inaccessible, std::move(location), std::forward<Range>(additional_locations), symbol};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_symbol_inaccessible(const DiagnosticSeverity severity,
                                                  Location location,
                                                  Range &&additional_locations,
                                                  Name symbol)
    {
        return Diagnostic{symbol_inaccessible,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          symbol};
    }

    export constexpr Diagnostic make_no_conversion(Location location, std::string from_type, std::string to_type)
    {
        return Diagnostic{no_conversion, std::move(location), std::move(from_type), std::move(to_type)};
    }

    export constexpr Diagnostic make_no_conversion(const DiagnosticSeverity severity,
                                                   Location location,
                                                   std::string from_type,
                                                   std::string to_type)
    {
        return Diagnostic{no_conversion, severity, std::move(location), std::move(from_type), std::move(to_type)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_no_conversion(Location location,
                                            Range &&additional_locations,
                                            std::string from_type,
                                            std::string to_type)
    {
        return Diagnostic{no_conversion,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(from_type),
                          std::move(to_type)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_no_conversion(const DiagnosticSeverity severity,
                                            Location location,
                                            Range &&additional_locations,
                                            std::string from_type,
                                            std::string to_type)
    {
        return Diagnostic{no_conversion,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(from_type),
                          std::move(to_type)};
    }

    export constexpr Diagnostic make_conversion_is_explicit(Location location,
                                                            std::string from_type,
                                                            std::string to_type)
    {
        return Diagnostic{conversion_is_explicit, std::move(location), std::move(from_type), std::move(to_type)};
    }

    export constexpr Diagnostic make_conversion_is_explicit(const DiagnosticSeverity severity,
                                                            Location location,
                                                            std::string from_type,
                                                            std::string to_type)
    {
        return Diagnostic{conversion_is_explicit,
                          severity,
                          std::move(location),
                          std::move(from_type),
                          std::move(to_type)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_conversion_is_explicit(Location location,
                                                     Range &&additional_locations,
                                                     std::string from_type,
                                                     std::string to_type)
    {
        return Diagnostic{conversion_is_explicit,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(from_type),
                          std::move(to_type)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_conversion_is_explicit(const DiagnosticSeverity severity,
                                                     Location location,
                                                     Range &&additional_locations,
                                                     std::string from_type,
                                                     std::string to_type)
    {
        return Diagnostic{conversion_is_explicit,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(from_type),
                          std::move(to_type)};
    }

    export constexpr Diagnostic make_multiple_file_scoped_namespaces(Location location)
    {
        return Diagnostic{multiple_file_scoped_namespaces, std::move(location)};
    }

    export constexpr Diagnostic make_multiple_file_scoped_namespaces(const DiagnosticSeverity severity,
                                                                     Location location)
    {
        return Diagnostic{multiple_file_scoped_namespaces, severity, std::move(location)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_multiple_file_scoped_namespaces(Location location, Range &&additional_locations)
    {
        return Diagnostic{multiple_file_scoped_namespaces,
                          std::move(location),
                          std::forward<Range>(additional_locations)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_multiple_file_scoped_namespaces(const DiagnosticSeverity severity,
                                                              Location location,
                                                              Range &&additional_locations)
    {
        return Diagnostic{multiple_file_scoped_namespaces,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations)};
    }

    export constexpr Diagnostic make_file_scoped_and_normal_namespace(Location location)
    {
        return Diagnostic{file_scoped_and_normal_namespace, std::move(location)};
    }

    export constexpr Diagnostic make_file_scoped_and_normal_namespace(const DiagnosticSeverity severity,
                                                                      Location location)
    {
        return Diagnostic{file_scoped_and_normal_namespace, severity, std::move(location)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_file_scoped_and_normal_namespace(Location location, Range &&additional_locations)
    {
        return Diagnostic{file_scoped_and_normal_namespace,
                          std::move(location),
                          std::forward<Range>(additional_locations)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_file_scoped_and_normal_namespace(const DiagnosticSeverity severity,
                                                               Location location,
                                                               Range &&additional_locations)
    {
        return Diagnostic{file_scoped_and_normal_namespace,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations)};
    }

    export constexpr Diagnostic make_file_scoped_namespace_not_before_all_members(Location location)
    {
        return Diagnostic{file_scoped_namespace_not_before_all_members, std::move(location)};
    }

    export constexpr Diagnostic make_file_scoped_namespace_not_before_all_members(const DiagnosticSeverity severity,
                                                                                  Location location)
    {
        return Diagnostic{file_scoped_namespace_not_before_all_members, severity, std::move(location)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_file_scoped_namespace_not_before_all_members(Location location,
                                                                           Range &&additional_locations)
    {
        return Diagnostic{file_scoped_namespace_not_before_all_members,
                          std::move(location),
                          std::forward<Range>(additional_locations)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_file_scoped_namespace_not_before_all_members(const DiagnosticSeverity severity,
                                                                           Location location,
                                                                           Range &&additional_locations)
    {
        return Diagnostic{file_scoped_namespace_not_before_all_members,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations)};
    }

    export constexpr Diagnostic make_bad_modifiers_on_namespace(Location location)
    {
        return Diagnostic{bad_modifiers_on_namespace, std::move(location)};
    }

    export constexpr Diagnostic make_bad_modifiers_on_namespace(const DiagnosticSeverity severity, Location location)
    {
        return Diagnostic{bad_modifiers_on_namespace, severity, std::move(location)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_bad_modifiers_on_namespace(Location location, Range &&additional_locations)
    {
        return Diagnostic{bad_modifiers_on_namespace, std::move(location), std::forward<Range>(additional_locations)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_bad_modifiers_on_namespace(const DiagnosticSeverity severity,
                                                         Location location,
                                                         Range &&additional_locations)
    {
        return Diagnostic{bad_modifiers_on_namespace,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations)};
    }

    export constexpr Diagnostic make_literal_value_too_big(Location location)
    {
        return Diagnostic{literal_value_too_big, std::move(location)};
    }

    export constexpr Diagnostic make_literal_value_too_big(const DiagnosticSeverity severity, Location location)
    {
        return Diagnostic{literal_value_too_big, severity, std::move(location)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_literal_value_too_big(Location location, Range &&additional_locations)
    {
        return Diagnostic{literal_value_too_big, std::move(location), std::forward<Range>(additional_locations)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_literal_value_too_big(const DiagnosticSeverity severity,
                                                    Location location,
                                                    Range &&additional_locations)
    {
        return Diagnostic{literal_value_too_big,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations)};
    }

    export constexpr Diagnostic make_unary_operator_undefined(Location location, std::string type)
    {
        return Diagnostic{unary_operator_undefined, std::move(location), std::move(type)};
    }

    export constexpr Diagnostic make_unary_operator_undefined(const DiagnosticSeverity severity,
                                                              Location location,
                                                              std::string type)
    {
        return Diagnostic{unary_operator_undefined, severity, std::move(location), std::move(type)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unary_operator_undefined(Location location,
                                                       Range &&additional_locations,
                                                       std::string type)
    {
        return Diagnostic{unary_operator_undefined,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(type)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_unary_operator_undefined(const DiagnosticSeverity severity,
                                                       Location location,
                                                       Range &&additional_locations,
                                                       std::string type)
    {
        return Diagnostic{unary_operator_undefined,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(type)};
    }

    export constexpr Diagnostic make_cannot_assign_expression(Location location)
    {
        return Diagnostic{cannot_assign_expression, std::move(location)};
    }

    export constexpr Diagnostic make_cannot_assign_expression(const DiagnosticSeverity severity, Location location)
    {
        return Diagnostic{cannot_assign_expression, severity, std::move(location)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_cannot_assign_expression(Location location, Range &&additional_locations)
    {
        return Diagnostic{cannot_assign_expression, std::move(location), std::forward<Range>(additional_locations)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_cannot_assign_expression(const DiagnosticSeverity severity,
                                                       Location location,
                                                       Range &&additional_locations)
    {
        return Diagnostic{cannot_assign_expression,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations)};
    }

    export constexpr Diagnostic make_no_compound_assignment_operator(Location location, std::string type)
    {
        return Diagnostic{no_compound_assignment_operator, std::move(location), std::move(type)};
    }

    export constexpr Diagnostic make_no_compound_assignment_operator(const DiagnosticSeverity severity,
                                                                     Location location,
                                                                     std::string type)
    {
        return Diagnostic{no_compound_assignment_operator, severity, std::move(location), std::move(type)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_no_compound_assignment_operator(Location location,
                                                              Range &&additional_locations,
                                                              std::string type)
    {
        return Diagnostic{no_compound_assignment_operator,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(type)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_no_compound_assignment_operator(const DiagnosticSeverity severity,
                                                              Location location,
                                                              Range &&additional_locations,
                                                              std::string type)
    {
        return Diagnostic{no_compound_assignment_operator,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(type)};
    }

    export constexpr Diagnostic make_binary_operator_undefined(Location location, std::string type1, std::string type2)
    {
        return Diagnostic{binary_operator_undefined, std::move(location), std::move(type1), std::move(type2)};
    }

    export constexpr Diagnostic make_binary_operator_undefined(const DiagnosticSeverity severity,
                                                               Location location,
                                                               std::string type1,
                                                               std::string type2)
    {
        return Diagnostic{binary_operator_undefined, severity, std::move(location), std::move(type1), std::move(type2)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_binary_operator_undefined(Location location,
                                                        Range &&additional_locations,
                                                        std::string type1,
                                                        std::string type2)
    {
        return Diagnostic{binary_operator_undefined,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(type1),
                          std::move(type2)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_binary_operator_undefined(const DiagnosticSeverity severity,
                                                        Location location,
                                                        Range &&additional_locations,
                                                        std::string type1,
                                                        std::string type2)
    {
        return Diagnostic{binary_operator_undefined,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(type1),
                          std::move(type2)};
    }

    export constexpr Diagnostic make_no_overload_matching_arg_count(Location location, std::size_t arg_count)
    {
        return Diagnostic{no_overload_matching_arg_count, std::move(location), arg_count};
    }

    export constexpr Diagnostic make_no_overload_matching_arg_count(const DiagnosticSeverity severity,
                                                                    Location location,
                                                                    std::size_t arg_count)
    {
        return Diagnostic{no_overload_matching_arg_count, severity, std::move(location), arg_count};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_no_overload_matching_arg_count(Location location,
                                                             Range &&additional_locations,
                                                             std::size_t arg_count)
    {
        return Diagnostic{no_overload_matching_arg_count,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          arg_count};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_no_overload_matching_arg_count(const DiagnosticSeverity severity,
                                                             Location location,
                                                             Range &&additional_locations,
                                                             std::size_t arg_count)
    {
        return Diagnostic{no_overload_matching_arg_count,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          arg_count};
    }

    export constexpr Diagnostic make_no_overload_for_arg_types(Location location, std::string arg_types)
    {
        return Diagnostic{no_overload_for_arg_types, std::move(location), std::move(arg_types)};
    }

    export constexpr Diagnostic make_no_overload_for_arg_types(const DiagnosticSeverity severity,
                                                               Location location,
                                                               std::string arg_types)
    {
        return Diagnostic{no_overload_for_arg_types, severity, std::move(location), std::move(arg_types)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_no_overload_for_arg_types(Location location,
                                                        Range &&additional_locations,
                                                        std::string arg_types)
    {
        return Diagnostic{no_overload_for_arg_types,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(arg_types)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_no_overload_for_arg_types(const DiagnosticSeverity severity,
                                                        Location location,
                                                        Range &&additional_locations,
                                                        std::string arg_types)
    {
        return Diagnostic{no_overload_for_arg_types,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(arg_types)};
    }

    export constexpr Diagnostic make_no_call_operator_defined(Location location, std::string type)
    {
        return Diagnostic{no_call_operator_defined, std::move(location), std::move(type)};
    }

    export constexpr Diagnostic make_no_call_operator_defined(const DiagnosticSeverity severity,
                                                              Location location,
                                                              std::string type)
    {
        return Diagnostic{no_call_operator_defined, severity, std::move(location), std::move(type)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_no_call_operator_defined(Location location,
                                                       Range &&additional_locations,
                                                       std::string type)
    {
        return Diagnostic{no_call_operator_defined,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(type)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_no_call_operator_defined(const DiagnosticSeverity severity,
                                                       Location location,
                                                       Range &&additional_locations,
                                                       std::string type)
    {
        return Diagnostic{no_call_operator_defined,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(type)};
    }

    export constexpr Diagnostic make_no_entry_point_defined(Location location)
    {
        return Diagnostic{no_entry_point_defined, std::move(location)};
    }

    export constexpr Diagnostic make_no_entry_point_defined(const DiagnosticSeverity severity, Location location)
    {
        return Diagnostic{no_entry_point_defined, severity, std::move(location)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_no_entry_point_defined(Location location, Range &&additional_locations)
    {
        return Diagnostic{no_entry_point_defined, std::move(location), std::forward<Range>(additional_locations)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_no_entry_point_defined(const DiagnosticSeverity severity,
                                                     Location location,
                                                     Range &&additional_locations)
    {
        return Diagnostic{no_entry_point_defined,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations)};
    }

    export constexpr Diagnostic make_ambiguous_entry_point(Location location, std::string names)
    {
        return Diagnostic{ambiguous_entry_point, std::move(location), std::move(names)};
    }

    export constexpr Diagnostic make_ambiguous_entry_point(const DiagnosticSeverity severity,
                                                           Location location,
                                                           std::string names)
    {
        return Diagnostic{ambiguous_entry_point, severity, std::move(location), std::move(names)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_ambiguous_entry_point(Location location, Range &&additional_locations, std::string names)
    {
        return Diagnostic{ambiguous_entry_point,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(names)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_ambiguous_entry_point(const DiagnosticSeverity severity,
                                                    Location location,
                                                    Range &&additional_locations,
                                                    std::string names)
    {
        return Diagnostic{ambiguous_entry_point,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(names)};
    }

    export constexpr Diagnostic make_invalid_entry_point_return_type(Location location,
                                                                     std::string type,
                                                                     std::string name)
    {
        return Diagnostic{invalid_entry_point_return_type, std::move(location), std::move(type), std::move(name)};
    }

    export constexpr Diagnostic make_invalid_entry_point_return_type(const DiagnosticSeverity severity,
                                                                     Location location,
                                                                     std::string type,
                                                                     std::string name)
    {
        return Diagnostic{invalid_entry_point_return_type,
                          severity,
                          std::move(location),
                          std::move(type),
                          std::move(name)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_invalid_entry_point_return_type(Location location,
                                                              Range &&additional_locations,
                                                              std::string type,
                                                              std::string name)
    {
        return Diagnostic{invalid_entry_point_return_type,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(type),
                          std::move(name)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_invalid_entry_point_return_type(const DiagnosticSeverity severity,
                                                              Location location,
                                                              Range &&additional_locations,
                                                              std::string type,
                                                              std::string name)
    {
        return Diagnostic{invalid_entry_point_return_type,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(type),
                          std::move(name)};
    }

    export constexpr Diagnostic make_invalid_entry_point_parameters(Location location,
                                                                    std::string types,
                                                                    std::string name)
    {
        return Diagnostic{invalid_entry_point_parameters, std::move(location), std::move(types), std::move(name)};
    }

    export constexpr Diagnostic make_invalid_entry_point_parameters(const DiagnosticSeverity severity,
                                                                    Location location,
                                                                    std::string types,
                                                                    std::string name)
    {
        return Diagnostic{invalid_entry_point_parameters,
                          severity,
                          std::move(location),
                          std::move(types),
                          std::move(name)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_invalid_entry_point_parameters(Location location,
                                                             Range &&additional_locations,
                                                             std::string types,
                                                             std::string name)
    {
        return Diagnostic{invalid_entry_point_parameters,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(types),
                          std::move(name)};
    }

    export template <std::ranges::input_range Range>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, Location>
    constexpr Diagnostic make_invalid_entry_point_parameters(const DiagnosticSeverity severity,
                                                             Location location,
                                                             Range &&additional_locations,
                                                             std::string types,
                                                             std::string name)
    {
        return Diagnostic{invalid_entry_point_parameters,
                          severity,
                          std::move(location),
                          std::forward<Range>(additional_locations),
                          std::move(types),
                          std::move(name)};
    }
} // namespace prism::diagnostics
