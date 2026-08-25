/**
 * @file member_container_binder.cpp
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */
module prism.core:binder.member_container_binder.impl;

import :binder.member_container_binder;
import :syntax.visit;
import :binder.binding_helpers;
import :compilation;
import :binder.lookup_context;
import :symbols.namespace_symbol;

namespace prism
{

    MemberContainerBinder::MemberContainerBinder(const Binder &next,
                                                 const MemberContainerSymbol &symbol,
                                                 const DeclarationSyntax &syntax)
        : Binder{next}, symbol_{symbol}, syntax_{syntax}, using_namespaces_{get_using_namespaces()}
    {
    }

    Optional<const Symbol &> MemberContainerBinder::containing_symbol() const
    {
        return symbol_;
    }
    Optional<const SyntaxNode &> MemberContainerBinder::scope_designator() const
    {
        return Binder::scope_designator();
    }

    LookupResult MemberContainerBinder::lookup_local(const Name name,
                                                     const LookupOptions options,
                                                     [[maybe_unused]] const LookupContext &context) const
    {
        if (auto result = lookup_qualified_name(name, symbol_, options); result.viable())
            return result;

        return make_lookup_result(
            get_using_namespaces() |
                std::views::transform([&](const NamespaceSymbol &ns)
                                      { return lookup_qualified_name(name, ns, options).symbols(); }) |
                std::views::join | std::ranges::to<SymbolList>(),
            options);
    }

    const ImmutableArray<Ref<const NamespaceSymbol>> &MemberContainerBinder::get_using_namespaces() const
    {
        return using_namespaces_.get_or_compute([this] { return compute_using_namespaces(); });
    }

    ImmutableArray<Ref<const NamespaceSymbol>> MemberContainerBinder::compute_using_namespaces() const
    {
        const auto ns = syntax_.as<NamespaceDeclarationSyntax>();
        if (!ns.has_value())
            return {};

        return build_using_namespaces(ns->usings());
    }
} // namespace prism
