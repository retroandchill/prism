/**
 * @file single_root_namespace_declaration.ixx
 * @author Francesco Corso
 * @date 8/13/2026
 * @brief
 */
export module prism.core:declarations.single_non_root_namespace_declaration;

import :declarations.single_namespace_declaration;

namespace prism
{
    class SingleNonRootNamespaceDeclaration final : public SingleNamespaceDeclaration
    {
      public:
        constexpr SingleNonRootNamespaceDeclaration(SemanticLifetime &lifetime,
                                                    const Name name,
                                                    const bool has_usings,
                                                    SyntaxReference syntax_reference,
                                                    SourceLocation name_location,
                                                    const std::span<const Diagnostic> diagnostics,
                                                    const std::span<const Ref<const Declaration>> children)
            : SingleNamespaceDeclaration{lifetime,
                                         name,
                                         has_usings,
                                         std::move(syntax_reference),
                                         std::move(name_location),
                                         diagnostics,
                                         children}
        {
        }
    };
} // namespace prism
