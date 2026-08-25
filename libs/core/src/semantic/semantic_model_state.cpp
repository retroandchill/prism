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
import :compilation;
import :syntax.node;
import :binder.binder_factory;
import :syntax.expressions;
import :syntax.statements;
import :syntax.declarations;
import :syntax.clauses;
import :symbols.function_symbol;

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

    const BoundExpression &SemanticModelState::get_bound_variable_initializer(
        const VariableDeclarationSyntax &declaration,
        const LookupContext &context)
    {
        auto &binder = get_binder(declaration);
        return get_bound_variable_initializer(declaration, binder, context);
    }

    const BoundExpression &SemanticModelState::get_bound_variable_initializer(
        const VariableDeclarationSyntax &declaration,
        const Binder &binder,
        const LookupContext &context)
    {
        DEBUG_ASSERT(declaration.initializer().has_value());
        Lazy<const BoundExpression &> *bound_expression;
        {
            std::scoped_lock lock{bound_initializers_mutex_};
            bound_expression = &bound_initializers_[&declaration];
        }

        return bound_expression->get_or_compute(
            [&] -> auto & { return binder.bind_expression(declaration.initializer()->value(), context); });
    }

    const BoundStatement &SemanticModelState::get_bound_body(const FunctionDeclarationSyntax &declaration,
                                                             const LookupContext &context)
    {
        auto &binder = get_binder(declaration);
        auto &symbol = get_declared_symbol(declaration).value().as_checked<FunctionSymbol>();
        Lazy<const BoundStatement &> *bound_body;
        {
            std::scoped_lock lock{bound_bodies_mutex_};
            bound_body = &bound_bodies_[&declaration];
        }

        return bound_body->get_or_compute(
            [&] -> const auto &
            {
                if (declaration.body().has_value())
                {
                    DEBUG_ASSERT(!declaration.expression_body().has_value());
                    return binder.bind_statement(*declaration.body(), symbol.return_type(), context);
                }

                DEBUG_ASSERT(declaration.expression_body().has_value());
                return binder.bind_expression_body(*declaration.expression_body(), symbol.return_type(), context);
            });
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
