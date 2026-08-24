/**
 * @file semantic_model_state.ixx
 * @author Francesco Corso
 * @date 8/18/2026
 * @brief
 */
export module prism.core:semantic.semantic_model_state;

import :util.noncopyable;
import std;
import :util.lazy;

namespace prism
{
    class BoundStatement;
    export class FunctionDeclarationSyntax;
    export class Symbol;
    export class ExpressionSyntax;
    class BoundExpression;
    export class SyntaxNode;
    class Binder;
    export class SyntaxTree;
    export class Compilation;
    class LookupContext;
    export class VariableDeclarationSyntax;

    class SemanticModelState final : NonCopyable
    {
      public:
        SemanticModelState(const Compilation &compilation, const SyntaxTree &syntax_tree);

        [[nodiscard]] const Binder &get_binder(const SyntaxNode &node) const;
        const BoundExpression &get_bound_variable_initializer(const VariableDeclarationSyntax &declaration,
                                                              const LookupContext &context);
        const BoundExpression &get_bound_variable_initializer(const VariableDeclarationSyntax &declaration,
                                                              const Binder &binder,
                                                              const LookupContext &context);

        const BoundStatement &get_bound_body(const FunctionDeclarationSyntax &declaration,
                                             const LookupContext &context);

        [[nodiscard]] Optional<const Symbol &> get_declared_symbol(const SyntaxNode &node);
        void cache_symbol(const SyntaxNode &node, const Symbol &symbol);

      private:
        const Compilation &compilation_;
        const SyntaxTree &tree_;

        std::mutex bound_initializers_mutex_;
        std::unordered_map<const VariableDeclarationSyntax *, Lazy<const BoundExpression &>> bound_initializers_;

        std::mutex bound_bodies_mutex_;
        std::unordered_map<const FunctionDeclarationSyntax *, Lazy<const BoundStatement &>> bound_bodies_;

        std::shared_mutex symbol_mutex_;
        std::unordered_map<const SyntaxNode *, const Symbol *> symbols_;
    };
} // namespace prism
