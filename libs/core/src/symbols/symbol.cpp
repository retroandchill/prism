/**
 * @file symbol.cpp
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:symbols.symbol.impl;

import :symbols.symbol;
import :symbols.assembly_symbol;
import :symbols.namespace_symbol;
import :symbols.named_type_symbol;
import :compilation;
import :diagnostics.diagnostic_bag;

namespace prism
{

    DeclaredVisibility Symbol::declared_visibility() const
    {
        // TODO: For now symbol visibility does not apply but eventually it will
        return DeclaredVisibility::not_applicable;
    }

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

    Optional<const AssemblySymbol &> Symbol::containing_assembly() const
    {
        return containing_symbol().and_then([](const Symbol &symbol) { return symbol.containing_assembly(); });
    }

    Optional<const NamespaceSymbol &> Symbol::containing_namespace() const
    {
        for (auto container = containing_symbol(); container.has_value(); container = container->containing_symbol())
        {
            auto ns = container->as<NamespaceSymbol>();
            if (ns.has_value())
                return ns;
        }

        return std::nullopt;
    }

    Optional<const TypeSymbol &> Symbol::containing_type() const
    {
        auto container = containing_symbol();
        auto containing_type =
            containing_symbol().and_then([](const Symbol &symbol) { return symbol.as<NamedTypeSymbol>(); });
        if (container.value_ptr() == containing_type.value_ptr())
        {
            return containing_type;
        }

        return container->containing_type();
    }

    Optional<const Compilation &> Symbol::declaring_compilation() const
    {
        return containing_assembly().and_then([](const AssemblySymbol &assembly)
                                              { return assembly.declaring_compilation(); });
    }

    void Symbol::add_declaration_diagnostics(const DiagnosticBag &diagnostics) const
    {
        const auto compilation = declaring_compilation();
        DEBUG_ASSERT(compilation.has_value());
        CompilationInternal::get_declaration_diagnostics(*compilation).add_range(diagnostics);
    }
} // namespace prism
