export module prism.core:diagnostics.traits;

import :diagnostics.code;
import :syntax.kind;
import :text.name;

namespace prism
{
    template <DiagnosticCode Code>
    struct DiagnosticTraits;

    template <>
    struct DiagnosticTraits<DiagnosticCode::unexpected_token>
    {
        using Args = std::tuple<std::string>;
    };

    template <>
    struct DiagnosticTraits<DiagnosticCode::unexpected_eof>
    {
        using Args = std::tuple<>;
    };

    template <>
    struct DiagnosticTraits<DiagnosticCode::unexpected_escape>
    {
        using Args = std::tuple<std::string>;
    };

    template <>
    struct DiagnosticTraits<DiagnosticCode::unterminated_string_literal>
    {
        using Args = std::tuple<>;
    };

    template <>
    struct DiagnosticTraits<DiagnosticCode::unterminated_character_literal>
    {
        using Args = std::tuple<>;
    };

    template <>
    struct DiagnosticTraits<DiagnosticCode::unterminated_block_comment>
    {
        using Args = std::tuple<>;
    };

    template <>
    struct DiagnosticTraits<DiagnosticCode::unresolved_symbol>
    {
        using Args = std::tuple<Name>;
    };

    template <>
    struct DiagnosticTraits<DiagnosticCode::invalid_symbol>
    {
        using Args = std::tuple<Name, std::string>;
    };

    template <>
    struct DiagnosticTraits<DiagnosticCode::expected_type_specifier>
    {
        using Args = std::tuple<>;
    };

    template <>
    struct DiagnosticTraits<DiagnosticCode::ambiguous_symbol>
    {
        using Args = std::tuple<Name>;
    };
} // namespace prism
