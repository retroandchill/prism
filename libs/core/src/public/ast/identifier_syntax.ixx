/**
 * @file identifier_syntax.ixx
 * @author Francesco Corso
 * @date 6/26/2026
 * @brief
 */
export module prism.core.ast:identifier_syntax;

import std;
import prism.core.lexer.token;
import prism.core.util;
import prism.core.source.source_file;
import prism.core.strings.name;
import :syntax_node;

namespace prism
{
    export struct IdentifierSyntax
    {
        Name name;
        SyntaxFlags flags{};
        SourceRange range{};
    };
} // namespace prism
