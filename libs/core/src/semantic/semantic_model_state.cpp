/**
 * @file semantic_model_state.cpp
 * @author Francesco Corso
 * @date 8/18/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:semantic.semantic_model_state.impl;

import libassert;
import :semantic.semantic_model_state;
import :semantic.compilation;
import :syntax.node;
import :binder.binder_factory;
import :syntax.expressions;

namespace prism
{

    SemanticModelState::SemanticModelState(const Compilation &compilation, const SyntaxTree &syntax_tree)
        : compilation_{compilation}, tree_{syntax_tree}
    {
    }

    const Binder &SemanticModelState::get_binder(const SyntaxNode &node) const
    {
        DEBUG_ASSERT(&node.tree() == &tree_);
        auto &factory = CompilationInternal::get_binder_factory(compilation_, tree_);
        return factory.get_binder(node);
    }

    const BoundExpression &SemanticModelState::get_bound_expression(const ExpressionSyntax &expression)
    {
        auto &binder = get_binder(expression);
        return get_bound_expression(expression, binder);
    }

    const BoundExpression &SemanticModelState::get_bound_expression(const ExpressionSyntax &expression,
                                                                    const Binder &binder)
    {
        Lazy<const BoundExpression &> *bound_expression;
        {
            std::scoped_lock lock{bound_expression_mutex_};
            bound_expression = &bound_expressions_[&expression];
        }

        return bound_expression->get_or_compute([&binder, &expression] -> auto &
                                                { return binder.bind_expression(expression); });
    }

    Optional<const Symbol &> SemanticModelState::get_declared_symbol(const SyntaxNode &node)
    {
        std::shared_lock lock{symbol_mutex_};
        const auto it = symbols_.find(&node);
        if (it == symbols_.end())
            return std::nullopt;

        return *it->second;
    }

    void SemanticModelState::cache_symbol(const SyntaxNode &node, const Symbol &symbol)
    {
        std::unique_lock lock{symbol_mutex_};
        symbols_[&node] = &symbol;
    }
} // namespace prism
