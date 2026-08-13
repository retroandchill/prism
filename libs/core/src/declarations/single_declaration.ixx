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
      public:
        using ChildType = SingleDeclaration;

      protected:
        SingleDeclaration(SemanticLifetime &lifetime,
                          Name name,
                          SyntaxReference syntax_reference,
                          SourceLocation name_location,
                          std::span<const Diagnostic> diagnostics,
                          std::span<const Ref<const Declaration>> children);

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

      protected:
        [[nodiscard]] std::span<const Ref<const Declaration>> get_declaration_children() const final;

      private:
#ifndef NDEBUG
        void validate_children() const;
#endif

        SyntaxReference syntax_reference_;
        SourceLocation name_location_;
        std::span<const Diagnostic> diagnostics_;
        std::span<const Ref<const Declaration>> children_;
    };
} // namespace prism
