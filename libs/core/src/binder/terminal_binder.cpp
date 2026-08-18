/**
 * @file terminal_binder.cpp
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */
module prism.core:binder.terminal_binder.impl;

import :binder.terminal_binder;

namespace prism
{

    TerminalBinder::TerminalBinder(const Compilation &compilation) : Binder(compilation)
    {
    }

    Optional<const Binder &> TerminalBinder::get_binder(const SyntaxNode &node) const
    {
        return std::nullopt;
    }

    Optional<const Symbol &> TerminalBinder::containing_symbol() const
    {
        return std::nullopt;
    }

    Optional<const SyntaxNode &> TerminalBinder::scope_designator() const
    {
        return std::nullopt;
    }

    VariablesSpan TerminalBinder::get_declared_local_variables_for_scope(const SyntaxNode &) const
    {
        return {};
    }

    LookupResult TerminalBinder::lookup_local(Name) const
    {
        return LookupResult{};
    }
} // namespace prism
