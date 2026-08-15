/**
 * @file single_declaration.ixx
 * @author Francesco Corso
 * @date 8/13/2026
 * @brief
 */
export module prism.core:declarations.single_declaration;

import :declarations.declaration;
import :diagnostics.diagnostic;

namespace prism
{
    class SemanticLifetime;

    class SingleDeclaration : public Declaration
    {
      protected:
        constexpr SingleDeclaration(SemanticLifetime &lifetime,
                                    Name name,
                                    SyntaxReference syntax_reference,
                                    SourceLocation name_location,
                                    std::span<const Diagnostic> diagnostics)
            : Declaration{lifetime, name}, syntax_reference_{std::move(syntax_reference)},
              name_location_{std::move(name_location)}, diagnostics_{diagnostics}
        {
        }

        ~SingleDeclaration() noexcept = default;

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

        [[nodiscard]] constexpr std::span<const Diagnostic> diagnostics() const noexcept
        {
            return diagnostics_;
        }

      private:
        SyntaxReference syntax_reference_;
        SourceLocation name_location_;
        std::span<const Diagnostic> diagnostics_;
    };
} // namespace prism
