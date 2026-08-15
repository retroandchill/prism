/**
 * @file single_variable_declaration.ixx
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */

export module prism.core:declarations.single_variable_declaration;

import :declarations.single_declaration;

namespace prism
{
    class SingleVariableDeclaration final : public SingleDeclaration
    {
      public:
        SingleVariableDeclaration(SemanticLifetime &lifetime,
                                  const Name name,
                                  SyntaxReference syntax_reference,
                                  SourceLocation name_location,
                                  const std::span<const Diagnostic> diagnostics)
            : SingleDeclaration{lifetime, name, std::move(syntax_reference), std::move(name_location), diagnostics}
        {
        }

        [[nodiscard]] DeclarationKind kind() const noexcept override
        {
            return DeclarationKind::variable;
        }
    };
} // namespace prism
