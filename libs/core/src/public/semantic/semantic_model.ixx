/**
 * @file semantic_model.ixx
 * @author Francesco Corso
 * @date 8/4/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core:semantic.semantic_model;

import :util.optional;
import :diagnostics.diagnostic;

namespace prism
{
    class Symbol;
    class FunctionSymbol;
    class ParameterSymbol;
    class VariableSymbol;
    class TypeSymbol;
    class Compilation;
    class SyntaxTree;

    export class PRISM_CORE_API SemanticModel final
    {
      public:
        explicit constexpr SemanticModel(const Compilation &compilation) noexcept : compilation_{&compilation}
        {
        }

        SemanticModel(const Compilation &compilation, const SyntaxTree &tree) noexcept
            : compilation_{&compilation}, tree_{&tree}
        {
        }

        [[nodiscard]] constexpr const Compilation &compilation() const noexcept
        {
            return *compilation_;
        }

        [[nodiscard]] constexpr Optional<const SyntaxTree &> tree() const noexcept
        {
            return tree_;
        }

        [[nodiscard]] std::generator<Diagnostic> get_diagnostics() const;

        [[nodiscard]] std::generator<Diagnostic> get_diagnostics(TextSpan span) const;

      private:
        const Compilation *compilation_;
        const SyntaxTree *tree_ = nullptr;
    };
} // namespace prism
