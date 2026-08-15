/**
 * @file merged_declaration.ixx
 * @author Francesco Corso
 * @date 8/13/2026
 * @brief
 */
export module prism.core:declarations.merged_declaration;

import :declarations.declaration;

namespace prism
{
    class MergedDeclaration : public Declaration
    {
      protected:
        constexpr MergedDeclaration(SemanticLifetime &lifetime, const Name name) : Declaration{lifetime, name}
        {
        }

        ~MergedDeclaration() = default;
    };
} // namespace prism
