/**
 * @file common_syntax.ixx
 * @author Francesco Corso
 * @date 6/26/2026
 * @brief
 */
export module prism.core.ast.common_syntax;

import prism.core.lexer.token;
import prism.core.util;
import prism.core.source.source_file;
#include <variant>

namespace prism
{
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
