/**
 * @file symbol.cpp
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module prism.core:symbols.symbol.impl;

import :symbols.symbol;
import :symbols.assembly_symbol;
import :symbols.namespace_symbol;
import :symbols.type_symbol;

namespace prism
{

    Optional<const AssemblySymbol &> Symbol::containing_assembly() const noexcept
    {
        return containing_symbol().and_then([](const Symbol &symbol) { return symbol.as<AssemblySymbol>(); });
    }

    Optional<const NamespaceSymbol &> Symbol::containing_namespace() const noexcept
    {
        return containing_symbol().and_then([](const Symbol &symbol) { return symbol.as<NamespaceSymbol>(); });
    }

    Optional<const TypeSymbol &> Symbol::containing_type() const noexcept
    {
        return containing_symbol().and_then([](const Symbol &symbol) { return symbol.as<TypeSymbol>(); });
    }
} // namespace prism
