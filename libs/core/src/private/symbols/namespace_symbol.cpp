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

    void NamespaceSymbol::write_display_string(TextWriter &writer) const
    {
        if (const auto owner = containing_namespace(); owner.has_value() && !owner->is_global())
        {
            owner->write_display_string(writer);
            writer.write("::");
        }

        writer.write(name().as_string_view());
    }
} // namespace prism
