/**
 * @file single_namespace_declaration.ixx
 * @author Francesco Corso
 * @date 8/13/2026
 * @brief
 */
export module prism.core:declarations.single_namespace_declaration;

import :declarations.single_declaration;

namespace prism
{
    class SingleNamespaceDeclaration : public SingleDeclaration
    {
      public:
        using MemberList = ImmutableArray<RefCountPtr<const SingleDeclaration>>;

        constexpr SingleNamespaceDeclaration(const Name name,
                                             const bool has_usings,
                                             SyntaxReference syntax_reference,
                                             SourceLocation name_location,
                                             MemberList members,
                                             ImmutableHashSet<Name> member_names,
                                             ImmutableArray<Diagnostic> diagnostics)
            : SingleDeclaration{name,
                                DeclarationKind::namespace_,
                                std::move(syntax_reference),
                                std::move(name_location),
                                std::move(member_names),
                                std::move(diagnostics)},
              has_usings_{has_usings}, members_{std::move(members)}
        {
        }

        [[nodiscard]] constexpr bool has_usings() const noexcept
        {
            return has_usings_;
        }

        [[nodiscard]] constexpr const MemberList &members() const noexcept
        {
            return members_;
        }

      private:
        bool has_usings_;
        MemberList members_{};
    };
} // namespace prism
