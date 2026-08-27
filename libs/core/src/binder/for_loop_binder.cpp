/**
 * @file for_loop_binder.cpp
 * @author Francesco Corso
 * @date 8/27/2026
 * @brief
 */
module prism.core:binder.for_loop_binder.impl;

import :binder.for_loop_binder;

namespace prism
{

    std::span<Ref<const VariableSymbol>> ForLoopBinder::build_local_variables_impl() const
    {
        if (!syntax_.declaration().has_value())
            return {};

        std::array<Ref<const StatementSyntax>, 1> variables = {*syntax_.declaration()};
        return build_local_variables(variables, *this);
    }
} // namespace prism
