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
import :util.lazy;

namespace prism
{
    class BoundStatement;
    class Binder;
    class ExpressionSyntax;
    class BoundExpression;
    class SemanticModelState;
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
    class LookupContext;

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

        [[nodiscard]] Optional<const Symbol &> get_declared_symbol(const SyntaxNode &node) const;

        [[nodiscard]] Optional<const VariableSymbol &> get_declared_symbol(const VariableDeclarationSyntax &node) const;

        [[nodiscard]] Optional<const FunctionSymbol &> get_declared_symbol(const FunctionDeclarationSyntax &node) const;

        [[nodiscard]] Optional<const NamespaceSymbol &> get_declared_symbol(
            const NamespaceDeclarationSyntax &node) const;

        [[nodiscard]] std::shared_ptr<SemanticModel> shared_from_this() noexcept;

        [[nodiscard]] std::shared_ptr<const SemanticModel> shared_from_this() const noexcept;

      private:
        friend struct SemanticModelInternal;

        [[nodiscard]] SemanticModelState &state() const;

        [[nodiscard]] const Binder &get_binder(const SyntaxNode &node) const;

        [[nodiscard]] const BoundExpression &get_bound_initializer(const VariableDeclarationSyntax &declaration,
                                                                   const LookupContext &context) const;

        [[nodiscard]] const BoundExpression &get_bound_initializer(const VariableDeclarationSyntax &declaration,
                                                                   const Binder &binder,
                                                                   const LookupContext &context) const;

        [[nodiscard]] const BoundStatement &get_bound_body(const FunctionDeclarationSyntax &declaration,
                                                           const LookupContext &context) const;

        void validate_is_part_of_compilation(const SyntaxNode &node) const;

        const Compilation *compilation_;
        const SyntaxTree *tree_ = nullptr;
        mutable Lazy<SemanticModelState &> state_{};
    };

    struct SemanticModelInternal
    {
        [[nodiscard]] static SemanticModel &create(const Compilation &compilation, const SyntaxTree &tree);

        [[nodiscard]] static inline SemanticModelState &get_state(const SemanticModel &model)
        {
            return model.state();
        }

        [[nodiscard]] static inline const Binder &get_binder(const SemanticModel &model, const SyntaxNode &node)
        {
            return model.get_binder(node);
        }

        [[nodiscard]] static inline const BoundExpression &get_bound_initializer(
            const SemanticModel &model,
            const VariableDeclarationSyntax &declaration,
            const LookupContext &context)
        {
            return model.get_bound_initializer(declaration, context);
        }

        [[nodiscard]] static inline const BoundExpression &get_bound_initializer(
            const SemanticModel &model,
            const VariableDeclarationSyntax &declaration,
            const Binder &binder,
            const LookupContext &context)
        {
            return model.get_bound_initializer(declaration, binder, context);
        }

        [[nodiscard]] static inline const BoundStatement &get_bound_body(const SemanticModel &model,
                                                                         const FunctionDeclarationSyntax &declaration,
                                                                         const LookupContext &context)
        {
            return model.get_bound_body(declaration, context);
        }
    };
} // namespace prism
