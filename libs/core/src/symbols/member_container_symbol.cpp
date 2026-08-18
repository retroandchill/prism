/**
 * @file member_container_symbol.cpp
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */
module prism.core:symbols.member_container_symbol.impl;

import :symbols.member_container_symbol;

namespace prism
{

    LookupResult MemberContainerSymbol::members(Name name) const
    {
        return LookupResult{members() | std::views::filter([name](const Symbol &s) { return s.name() == name; }) |
                            std::ranges::to<SymbolList>()};
    }
} // namespace prism
