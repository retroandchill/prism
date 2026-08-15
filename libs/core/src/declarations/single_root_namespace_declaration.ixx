/**
 * @file single_root_namespace_declaration.ixx
 * @author Francesco Corso
 * @date 8/13/2026
 * @brief
 */
export module prism.core:declarations.single_root_namespace_declaration;

import :declarations.single_namespace_declaration;

namespace prism
{
    class SingleRootNamespaceDeclaration final : public SingleNamespaceDeclaration
    {
      public:
        constexpr SingleRootNamespaceDeclaration(const Name name,
                                                 const bool has_usings,
                                                 SyntaxReference syntax_reference,
                                                 SourceLocation name_location,
                                                 MemberList members,
                                                 ImmutableHashSet<Name> member_names,
                                                 ImmutableArray<Diagnostic> diagnostics)
            : SingleNamespaceDeclaration{name,
                                         has_usings,
                                         std::move(syntax_reference),
                                         std::move(name_location),
                                         std::move(members),
                                         std::move(member_names),
                                         std::move(diagnostics)}
        {
        }
    };
} // namespace prism
