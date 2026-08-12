/**
 * @file bound_node_lookup.cpp
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
module prism.core:semantic.bound_node_lookup.impl;

import :semantic.bound_node_lookup;

namespace prism
{
    void BoundNodeLookup::add_variable_initializer(const VariableSymbol &variable, const BoundExpression &initializer)
    {
        std::scoped_lock lock{variable_initializer_mutex_};
        variable_initializers_.emplace(&variable, &initializer);
    }

    void BoundNodeLookup::add_function_body(const FunctionSymbol &function, const BoundStatement &body)
    {
        std::scoped_lock lock{function_body_mutex_};
        function_bodies_.emplace(&function, &body);
    }
} // namespace prism
