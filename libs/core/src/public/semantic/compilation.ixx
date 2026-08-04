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
import :symbols.symbol_lifetime;
import :semantic.semantic_model;

namespace prism
{
    class AssemblySymbol;

    export class PRISM_CORE_API Compilation : NonCopyable
    {
        Compilation(std::unique_ptr<SymbolLifetime> lifetime,
                    const AssemblySymbol &assembly,
                    std::vector<std::unique_ptr<SyntaxTree>> trees,
                    std::vector<Diagnostic> diagnostics) noexcept;

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

      private:
        std::unique_ptr<SymbolLifetime> lifetime_;
        const AssemblySymbol &assembly_;
        std::vector<std::unique_ptr<SyntaxTree>> trees_;
        std::vector<Diagnostic> diagnostics_;
    };
} // namespace prism
