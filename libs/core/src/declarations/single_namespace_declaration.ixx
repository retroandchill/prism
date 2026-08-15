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
      protected:
        constexpr SingleNamespaceDeclaration(SemanticLifetime &lifetime,
                                             const Name name,
                                             const bool has_usings,
                                             SyntaxReference syntax_reference,
                                             SourceLocation name_location,
                                             const std::span<const Diagnostic> diagnostics,
                                             const std::span<const Ref<const SingleDeclaration>> members)
            : SingleDeclaration{lifetime, name, std::move(syntax_reference), std::move(name_location), diagnostics},
              has_usings_{has_usings}, members_{members}
        {
        }

        ~SingleNamespaceDeclaration() noexcept = default;

      public:
        [[nodiscard]] constexpr DeclarationKind kind() const noexcept final
        {
            return DeclarationKind::namespace_;
        }

        [[nodiscard]] constexpr bool has_usings() const noexcept
        {
            return has_usings_;
        }

        [[nodiscard]] constexpr std::span<const Ref<const SingleDeclaration>> members() const noexcept
        {
            return members_;
        }

      private:
        bool has_usings_;
        std::span<const Ref<const SingleDeclaration>> members_{};
    };
} // namespace prism
