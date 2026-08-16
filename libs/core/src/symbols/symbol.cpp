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

    Optional<Location> Symbol::try_get_first_location() const
    {
        const auto locations = this->locations();
        if (locations.empty())
            return std::nullopt;

        return locations.front();
    }

    Location Symbol::first_location() const
    {
        auto location = try_get_first_location();
        if (!location.has_value())
            throw InvalidStateException{"Symbol has no location"};

        return *std::move(location);
    }

    Location Symbol::first_location_or_none() const
    {
        return try_get_first_location().value_or(no_location);
    }

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

    Optional<const Compilation &> Symbol::declaring_compilation() const
    {
        return containing_assembly().and_then([](const AssemblySymbol &assembly)
                                              { return assembly.declaring_compilation(); });
    }
} // namespace prism
