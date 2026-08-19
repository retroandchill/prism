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
    class Symbol;
    class ExpressionSyntax;
    class BoundExpression;
    class SyntaxNode;
    class Binder;
    class SyntaxTree;
    class Compilation;

    class SemanticModelState final : NonCopyable
    {
      public:
        SemanticModelState(const Compilation &compilation, const SyntaxTree &syntax_tree);

        [[nodiscard]] const Binder &get_binder(const SyntaxNode &node) const;
        const BoundExpression &get_bound_expression(const ExpressionSyntax &expression);
        const BoundExpression &get_bound_expression(const ExpressionSyntax &expression, const Binder &binder);

        [[nodiscard]] Optional<const Symbol &> get_declared_symbol(const SyntaxNode &node);
        void cache_symbol(const SyntaxNode &node, const Symbol &symbol);

      private:
        const Compilation &compilation_;
        const SyntaxTree &tree_;

        std::mutex bound_expression_mutex_;
        std::unordered_map<const ExpressionSyntax *, Lazy<const BoundExpression &>> bound_expressions_;

        std::shared_mutex symbol_mutex_;
        std::unordered_map<const SyntaxNode *, const Symbol *> symbols_;
    };
} // namespace prism
