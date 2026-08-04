/**
 * @file compilation.ixx
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core:semantic.compilation;

import :util.noncopyable;
import :syntax.tree;
import :semantic.semantic_lifetime;
import :semantic.semantic_model;
import :binder.declaration_scope;

namespace prism
{
    class AssemblySymbol;

    export class PRISM_CORE_API Compilation : NonCopyable
    {
        Compilation(std::unique_ptr<SemanticLifetime> lifetime,
                    const AssemblySymbol &assembly,
                    std::vector<std::unique_ptr<SyntaxTree>> trees,
                    std::vector<Diagnostic> diagnostics,
                    DeclarationScopeMap declaration_scopes) noexcept;

      public:
        static std::unique_ptr<Compilation> create(Name assembly_name, std::vector<std::unique_ptr<SyntaxTree>> trees);

        [[nodiscard]] constexpr const AssemblySymbol &assembly() const noexcept
        {
            return assembly_;
        }

        [[nodiscard]] constexpr const std::vector<std::unique_ptr<SyntaxTree>> &trees() const noexcept
        {
            return trees_;
        }

        [[nodiscard]] SemanticModel get_semantic_model(const SyntaxTree &tree) const noexcept
        {
            return SemanticModel{*this, tree};
        }

        [[nodiscard]] constexpr const std::vector<Diagnostic> &diagnostics() const noexcept
        {
            return diagnostics_;
        }

        [[nodiscard]] const DeclarationScope &get_declaration_scope(const SyntaxNode &node) const;

      private:
        std::unique_ptr<SemanticLifetime> lifetime_;
        const AssemblySymbol &assembly_;
        std::vector<std::unique_ptr<SyntaxTree>> trees_;
        std::vector<Diagnostic> diagnostics_;
        DeclarationScopeMap declaration_scopes_;
    };
} // namespace prism
