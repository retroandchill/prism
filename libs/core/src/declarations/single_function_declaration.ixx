/**
 * @file single_function_declaration.ixx
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */

export module prism.core:declarations.single_function_declaration;

import :declarations.single_declaration;

namespace prism
{
    class SingleFunctionDeclaration final : public SingleDeclaration
    {
      public:
        SingleFunctionDeclaration(SemanticLifetime &lifetime,
                                  const Name name,
                                  SyntaxReference syntax_reference,
                                  SourceLocation name_location,
                                  const std::span<const Diagnostic> diagnostics,
                                  const std::span<const Ref<const Declaration>> children)
            : SingleDeclaration{lifetime,
                                name,
                                std::move(syntax_reference),
                                std::move(name_location),
                                diagnostics,
                                children}
        {
        }

        [[nodiscard]] DeclarationKind kind() const noexcept override
        {
            return DeclarationKind::function;
        }
    };
} // namespace prism
