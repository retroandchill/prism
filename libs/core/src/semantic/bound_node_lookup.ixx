/**
 * @file bound_node_lookup.ixx
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
export module prism.core:semantic.bound_node_lookup;

import std;

namespace prism
{
    class BoundExpression;
    class BoundStatement;
    class FunctionSymbol;
    class VariableSymbol;

    class BoundNodeLookup final
    {
      public:
        void add_variable_initializer(const VariableSymbol &variable, const BoundExpression &initializer);
        void add_function_body(const FunctionSymbol &function, const BoundStatement &body);

      private:
        std::mutex variable_initializer_mutex_{};
        std::unordered_map<const VariableSymbol *, const BoundExpression *> variable_initializers_{};

        std::mutex function_body_mutex_{};
        std::unordered_map<const FunctionSymbol *, const BoundStatement *> function_bodies_{};
    };
} // namespace prism
