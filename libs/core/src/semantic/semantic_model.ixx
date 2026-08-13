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
import :semantic.conversion;

namespace prism
{
    class VariableDeclarationStatementSyntax;
    class SourceVariableSymbol;
    class NamespaceDeclarationSyntax;
    class NamespaceSymbol;
    class VariableDeclarationSyntax;
    class FunctionDeclarationSyntax;
    class Symbol;
    class FunctionSymbol;
    class ParameterSymbol;
    class VariableSymbol;
    class TypeSymbol;
    class Compilation;
    class SyntaxTree;

    export class PRISM_CORE_API SemanticModel final : NonCopyable
    {
        struct CreateTag
        {
        };

        static constexpr CreateTag create_tag;

      public:
        SemanticModel(CreateTag, const Compilation &compilation, const SyntaxTree &tree) noexcept
            : compilation_{&compilation}, tree_{&tree}
        {
        }

        [[nodiscard]] constexpr const Compilation &compilation() const noexcept
        {
            return *compilation_;
        }

        [[nodiscard]] constexpr const SyntaxTree &tree() const noexcept
        {
            return *tree_;
        }

        [[nodiscard]] std::generator<Diagnostic> get_diagnostics() const;

        [[nodiscard]] std::generator<Diagnostic> get_diagnostics(TextSpan span) const;

        [[nodiscard]] Optional<const Symbol &> get_declared_symbol(const SyntaxNode &node) const;

        [[nodiscard]] Optional<const VariableSymbol &> get_declared_symbol(const VariableDeclarationSyntax &node) const;

        [[nodiscard]] Optional<const FunctionSymbol &> get_declared_symbol(const FunctionDeclarationSyntax &node) const;

        [[nodiscard]] Optional<const NamespaceSymbol &> get_declared_symbol(
            const NamespaceDeclarationSyntax &node) const;

        [[nodiscard]] std::shared_ptr<SemanticModel> shared_from_this() noexcept;

        [[nodiscard]] std::shared_ptr<const SemanticModel> shared_from_this() const noexcept;

      private:
        friend class Compilation;
        friend class ExpressionBinder;

        void validate_is_part_of_compilation(const SyntaxNode &node) const;
        [[nodiscard]] SourceVariableSymbol &get_local_variable(const VariableDeclarationStatementSyntax &syntax) const;

        const Compilation *compilation_;
        const SyntaxTree *tree_ = nullptr;
    };
} // namespace prism
