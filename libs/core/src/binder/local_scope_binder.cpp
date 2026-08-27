/**
 * @file local_scope_binder.cpp
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:binder.local_scope_binder.impl;

import :binder.local_scope_binder;
import :syntax.visit;
import :symbols.source;
import :semantic.semantic_lifetime;
import :binder.binding_helpers;

namespace prism
{

    LocalScopeBinder::LocalScopeBinder(const Binder &next) : Binder{next}
    {
    }

    std::span<Ref<const VariableSymbol>> LocalScopeBinder::local_variables() const
    {
        return local_variables_.get_or_compute([this] { return build_local_variables_impl(); });
    }

    void LocalScopeBinder::ensure_locals() const
    {
        std::ignore = local_variables();
        Binder::ensure_locals();
    }

    std::span<Ref<const VariableSymbol>> LocalScopeBinder::build_local_variables(SyntaxList<StatementSyntax> statements,
                                                                                 const Binder &enclosing_binder) const
    {
        PooledVector<Ref<const VariableSymbol>> local_variables;
        for (auto &statement : statements)
        {
            build_local_variables(enclosing_binder, statement, local_variables);
        }
        return lifetime().copy_refs(local_variables);
    }

    std::span<Ref<const VariableSymbol>> LocalScopeBinder::build_local_variables(
        const std::span<Ref<const StatementSyntax>> statements,
        const Binder &enclosing_binder) const
    {
        PooledVector<Ref<const VariableSymbol>> local_variables;
        for (const auto statement : statements)
        {
            build_local_variables(enclosing_binder, statement, local_variables);
        }
        return lifetime().copy_refs(local_variables);
    }

    void LocalScopeBinder::build_local_variables(const Binder &enclosing_binder,
                                                 const StatementSyntax &statement,
                                                 PooledVector<Ref<const VariableSymbol>> &variables) const
    {
        const auto local = statement.as<VariableDeclarationStatementSyntax>();
        if (!local.has_value())
            return;

        auto &declaration_binder =
            *enclosing_binder.get_binder(*local).or_else([&] -> auto & { return enclosing_binder; });
        variables.emplace_back(make_local(local->declaration(), &declaration_binder));
    }

    const SourceVariableSymbol &LocalScopeBinder::make_local(const VariableDeclarationSyntax &declaration,
                                                             const Binder *initializer_binder) const
    {
        auto name = get_identifier_name(declaration.identifier());
        const auto containing = containing_symbol();
        DEBUG_ASSERT(containing.has_value());
        return lifetime().create<SourceLocalVariableSymbol>(name,
                                                            containing.value_ptr(),
                                                            declaration,
                                                            *this,
                                                            initializer_binder);
    }

    LookupResult LocalScopeBinder::lookup_local(Name name,
                                                const LookupOptions options,
                                                [[maybe_unused]] const LookupContext &context) const
    {
        if (has_any_flags(options, LookupOptions::value))
        {
            return make_lookup_result(
                local_variables() | std::views::filter([name](const Symbol &symbol) { return symbol.name() == name; }) |
                    std::ranges::to<SymbolList>(),
                LookupOptions::value);
        }

        return LookupResult::not_found();
    }
} // namespace prism
