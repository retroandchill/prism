/**
 * @file single_declaration.cpp
 * @author Francesco Corso
 * @date 8/13/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:declarations.single_declaration.impl;

import libassert;
import :declarations.single_declaration;

namespace prism
{
    SingleDeclaration::SingleDeclaration(SemanticLifetime &lifetime,
                                         const Name name,
                                         SyntaxReference syntax_reference,
                                         SourceLocation name_location,
                                         const std::span<const Diagnostic> diagnostics,
                                         const std::span<const Ref<const Declaration>> children)
        : Declaration{lifetime, name}, syntax_reference_{std::move(syntax_reference)},
          name_location_{std::move(name_location)}, diagnostics_{diagnostics}, children_{children}
    {
#ifndef NDEBUG
        validate_children();
#endif
    }

    std::span<const Ref<const Declaration>> SingleDeclaration::get_declaration_children() const
    {
        return children_;
    }

#ifndef NDEBUG
    void SingleDeclaration::validate_children() const
    {
        for (auto child : children_)
        {
            DEBUG_ASSERT(child.is<ChildType>());
        }
    }
#endif
} // namespace prism
