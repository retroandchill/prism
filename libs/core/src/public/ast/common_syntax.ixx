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
    };

    export constexpr EmptySyntax empty_syntax{};

    export struct ErrorSyntax
    {
        SourceRange range;
    };

    export struct ValidIdentifierSyntax
    {
        SharedString name;
        SourceRange range;
    };

    export using IdentifierSyntax = std::variant<ValidIdentifierSyntax, ErrorSyntax>;
} // namespace prism
