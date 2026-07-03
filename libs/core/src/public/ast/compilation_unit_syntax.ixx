/**
 * @file compilation_unit_syntax.ixx
 * @author Francesco Corso
 * @date 7/2/2026
 * @brief
 */
module;

#include "ast/macros.hpp"

export module prism.core.ast:compilation_unit_syntax;

import std;
import :syntax_node;
import :statement_syntax;
import prism.core.source.source_file;
import prism.core.util;
import :declaration_syntax;

namespace prism
{
    export class CompilationUnitSyntax : public SyntaxNode
    {
      public:
        constexpr CompilationUnitSyntax(const std::span<const Ref<const DeclarationSyntax>> declarations,
                                        const SourceRange range,
                                        const SyntaxFlags flags = SyntaxFlags::none)
            : SyntaxNode{SyntaxCategory::none, SyntaxKind::compilation_unit, range, flags}, declarations_{declarations}
        {
        }

        VALUE_PROPERTY(std::span<const Ref<const DeclarationSyntax>>, declarations)
    };
} // namespace prism
