/**
 * @file syntax_utils.cpp
 * @author Francesco Corso
 * @date 8/27/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:syntax.syntax_utils.impl;

import :syntax.syntax_utils;
import :syntax.node;
import :syntax.reference;
import :syntax.top_level;
import :syntax.declarations;
import :syntax.names;

namespace prism
{

    const SyntaxNode &get_namespace_declaration_syntax(const SyntaxReference &reference)
    {
        auto *node = &reference.syntax();

        while (node != nullptr && node->is<NameSyntax>())
        {
            node = node->parent().value_ptr();
        }

        DEBUG_ASSERT(node != nullptr && (node->is<CompilationUnitSyntax>() || node->is<NamespaceDeclarationSyntax>()));
        return *node;
    }
} // namespace prism
