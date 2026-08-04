/**
 * @file declaration_scope.cpp
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module prism.core:binder.declaration_scope.impl;

import :binder.declaration_scope;

import :symbols.namespace_symbol;
import :semantic.

    namespace prism{void DeclarationScope::add_symbol(const Symbol &symbol){auto &list = symbols_[symbol.name()];
list.emplace_back(symbol);
}

void DeclarationScope::add_using_declaration(const NamespaceSymbol &symbol)
{
    using_namespaces_.emplace_back(symbol);
}

DeclarationLookupResult DeclarationScope::lookup_nearest(const Name name) const
{
    SymbolList symbols;

    if (const auto it = symbols_.find(name); it != symbols_.end())
    {
        symbols.append_range(it->second);
    }
    else
    {
        append_namespace_members(name, symbols);
    }

    if (!symbols.empty())
    {
        return DeclarationLookupResult{std::move(symbols)};
    }

    return parent()
        .transform([name](const DeclarationScope &scope) { return scope.lookup_nearest(name); })
        .value_or_default();
}

DeclarationLookupResult DeclarationScope::lookup_all_visible(const Name name) const
{
    SymbolList symbols;
    auto *current = this;

    while (current != nullptr)
    {
        if (const auto it = current->symbols_.find(name); it != current->symbols_.end())
        {
            symbols.append_range(it->second);
        }

        append_namespace_members(name, symbols);

        current = current->parent_;
    }

    return DeclarationLookupResult{std::move(symbols)};
}

void DeclarationScope::append_namespace_members(const Name name, SymbolList &symbols) const
{
    symbols.append_range(
        using_namespaces_ | std::views::transform([](const NamespaceSymbol &ns) { return ns.members(); }) |
        std::views::join | std::views::filter([name](const Symbol &symbol) { return symbol.name() == name; }));
}
} // namespace prism
