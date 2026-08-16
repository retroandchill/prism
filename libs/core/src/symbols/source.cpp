/**
 * @file source.cpp
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:symbols.source.impl;

import :symbols.source;
import :syntax.declarations;
import :syntax.clauses;
import :semantic.compilation;

namespace prism
{
    SourceAssemblySymbol::SourceAssemblySymbol(const Compilation &compilation)
        : AssemblySymbol{compilation.assembly_name()}, declaring_compilation_{compilation}
    {
    }

    const ImmutableArray<Location> &SourceAssemblySymbol::locations() const
    {
        return locations_.get_or_compute(
            [this]
            {
                return declaring_compilation_.merged_root_declaration().declarations() |
                       std::views::transform([](const RefCountPtr<const SingleNamespaceDeclaration> &d)
                                             { return d->location(); }) |
                       std::ranges::to<ImmutableArray<Location>>();
            });
    }

    const NamespaceSymbol &SourceAssemblySymbol::global_namespace() const
    {
        return global_namespace_.get_or_compute(
            [this] -> auto &
            {
                return declaring_compilation_.lifetime_.create<SourceNamespaceSymbol>(
                    declaring_compilation_.merged_root_declaration().shared_from_this(),
                    this);
            });
    }

    std::span<const SyntaxReference> SourceAssemblySymbol::declaring_syntax_references() const
    {
        return {};
    }

    Optional<const Compilation &> SourceAssemblySymbol::declaring_compilation() const
    {
        return declaring_compilation_;
    }

    SourceNamespaceSymbol::SourceNamespaceSymbol(RefCountPtr<const MergedNamespaceDeclaration> declaration,
                                                 const Symbol *containing)
        : NamespaceSymbol{declaration->name(), containing}, merged_declaration_{std::move(declaration)}
    {
    }

    const ImmutableArray<Location> &SourceNamespaceSymbol::locations() const
    {
        return locations_.get_or_compute([this] { return merged_declaration_->name_locations(); });
    }

    Optional<const Compilation &> SourceNamespaceSymbol::containing_compilation() const noexcept
    {
        return std::nullopt;
    }

    SourceVariableSymbol::SourceVariableSymbol(const Name &name,
                                               const Symbol *containing,
                                               const VariableDeclarationSyntax &syntax)
        : VariableSymbol(name, containing), syntax_{syntax}, syntax_reference_{syntax}
    {
    }

    const ImmutableArray<Location> &SourceVariableSymbol::locations() const
    {
        return locations_.get_or_compute([this] { return ImmutableArray{syntax_.identifier().location()}; });
    }

    const TypeSymbol &SourceVariableSymbol::type() const
    {
        ASSUME(type_ == nullptr);
        return *type_;
    }

    bool SourceVariableSymbol::is_mutable() const noexcept
    {
        return syntax_.mut_keyword().has_value();
    }

    std::span<const SyntaxReference> SourceVariableSymbol::declaring_syntax_references() const
    {
        return {&syntax_reference_, 1};
    }

    SourceFunctionSymbol::SourceFunctionSymbol(const Name &name,
                                               const Symbol *containing,
                                               const FunctionDeclarationSyntax &syntax)
        : FunctionSymbol(name, containing), syntax_{syntax}, syntax_reference_{syntax}
    {
    }

    const ImmutableArray<Location> &SourceFunctionSymbol::locations() const
    {
        return locations_.get_or_compute([this] { return ImmutableArray{syntax_.identifier().location()}; });
    }

    const TypeSymbol &SourceFunctionSymbol::return_type() const
    {
        ASSUME(return_type_ == nullptr);
        return *return_type_;
    }

    std::span<const SyntaxReference> SourceFunctionSymbol::declaring_syntax_references() const
    {
        return {&syntax_reference_, 1};
    }

    SourceParameterSymbol::SourceParameterSymbol(const Name &name,
                                                 const Symbol *containing,
                                                 const ParameterSyntax &syntax)
        : ParameterSymbol(name, containing), syntax_{syntax}, syntax_reference_{syntax}
    {
    }

    const ImmutableArray<Location> &SourceParameterSymbol::locations() const
    {
        return locations_.get_or_compute([this] { return ImmutableArray{syntax_.name().location()}; });
    }

    const TypeSymbol &SourceParameterSymbol::type() const
    {
        ASSUME(type_ == nullptr);
        return *type_;
    }

    bool SourceParameterSymbol::is_mutable() const noexcept
    {
        return syntax_.mut_keyword().has_value();
    }

    std::span<const SyntaxReference> SourceParameterSymbol::declaring_syntax_references() const
    {
        return {&syntax_reference_, 1};
    }
} // namespace prism
