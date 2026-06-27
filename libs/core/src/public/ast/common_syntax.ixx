/**
 * @file common_syntax.ixx
 * @author Francesco Corso
 * @date 6/26/2026
 * @brief
 */
export module prism.core.ast.common_syntax;

import std;
import prism.core.lexer.token;
import prism.core.util;
import prism.core.source.source_file;

namespace prism
{
    export struct Modifiers
    {
        bool is_extern = false;
    };

    export struct EmptySyntax
    {
        SourceRange range;
    };

    export struct ErrorSyntax
    {
        SourceRange range;
    };

    export enum class IdentifierFlags : std::uint8_t
    {
        none = 0,
        missing = 1 << 0,
    };

    template <>
    constexpr bool is_flag_enum<IdentifierFlags> = true;

    export struct IdentifierSyntax
    {
        SharedString name;
        SourceRange range;
        IdentifierFlags flags = IdentifierFlags::none;
    };
} // namespace prism
