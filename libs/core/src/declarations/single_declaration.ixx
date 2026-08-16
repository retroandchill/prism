/**
 * @file single_declaration.ixx
 * @author Francesco Corso
 * @date 8/13/2026
 * @brief
 */
export module prism.core:declarations.single_declaration;

import :declarations.declaration;
import :diagnostics.diagnostic;
import :collections.immutable_hash_set;

namespace prism
{
    class SemanticLifetime;

    class SingleDeclaration : public Declaration
    {
      protected:
        constexpr SingleDeclaration(const Name name,
                                    const DeclarationKind kind,
                                    SyntaxReference syntax_reference,
                                    SourceLocation name_location,
                                    ImmutableHashSet<Name> member_names,
                                    ImmutableArray<Diagnostic> diagnostics)
            : Declaration{name, kind, false}, syntax_reference_{std::move(syntax_reference)},
              name_location_{std::move(name_location)}, diagnostics_{std::move(diagnostics)},
              member_names_{std::move(member_names)}
        {
        }

      public:
        [[nodiscard]] inline SourceLocation location() const noexcept
        {
            return SourceLocation{syntax_reference_};
        }

        [[nodiscard]] constexpr const SyntaxReference &syntax_reference() const noexcept
        {
            return syntax_reference_;
        }

        [[nodiscard]] constexpr const SourceLocation &name_location() const noexcept
        {
            return name_location_;
        }

        [[nodiscard]] constexpr const ImmutableArray<Diagnostic> &diagnostics() const noexcept
        {
            return diagnostics_;
        }

        [[nodiscard]] constexpr const ImmutableHashSet<Name> &member_names() const
        {
            return member_names_;
        }

      private:
        SyntaxReference syntax_reference_;
        SourceLocation name_location_;
        ImmutableArray<Diagnostic> diagnostics_;
        ImmutableHashSet<Name> member_names_;
    };
} // namespace prism
