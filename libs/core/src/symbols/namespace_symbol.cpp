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
    Optional<const NamespaceSymbol &> NamespaceSymbol::get_nested_namespace(const Name name) const
    {
        for (const auto symbol : members(name))
        {
            if (auto ns = symbol->as<NamespaceSymbol>(); ns.has_value())
                return ns;
        }

        return std::nullopt;
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
