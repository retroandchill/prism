/**
 * @file syntax_utils.ixx
 * @author Francesco Corso
 * @date 8/27/2026
 * @brief
 */

export module prism.core:syntax.syntax_utils;

namespace prism
{
    export class SyntaxReference;
    export class SyntaxNode;

    const SyntaxNode &get_namespace_declaration_syntax(const SyntaxReference &reference);
} // namespace prism
