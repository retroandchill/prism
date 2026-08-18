/**
 * @file block_binder.cpp
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:binder.block_binder.impl;

import :binder.block_binder;
import :syntax.statements;

namespace prism
{

    BlockBinder::BlockBinder(const Binder &enclosing, const BlockSyntax &block_syntax)
        : LocalScopeBinder{enclosing}, block_syntax_{block_syntax}
    {
    }

    Optional<const SyntaxNode &> BlockBinder::scope_designator() const
    {
        return block_syntax_;
    }

    VariablesSpan BlockBinder::get_declared_local_variables_for_scope(const SyntaxNode &designator) const
    {
        DEBUG_ASSERT(&designator == &block_syntax_);
        return local_variables();
    }

    std::span<Ref<const VariableSymbol>> BlockBinder::build_local_variables_impl() const
    {
        return build_local_variables(block_syntax_.statements(), *this);
    }
} // namespace prism
