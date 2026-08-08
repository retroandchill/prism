/**
 * @file namespace_symbol.cpp
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
module prism.core:symbols.namespace_symbol.impl;

import :symbols.namespace_symbol;

namespace prism
{
    LookupResult NamespaceSymbol::members(Name name) const
    {
        return LookupResult{members() | std::views::filter([name](const Symbol &s) { return s.name() == name; }) |
                            std::ranges::to<SymbolList>()};
    }
} // namespace prism
