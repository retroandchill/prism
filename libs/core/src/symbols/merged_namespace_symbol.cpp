/**
 * @file merged_namespace_symbol.cpp
 * @author Francesco Corso
 * @date 8/5/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:symbols.merged_namespace_symbol.impl;

import :symbols.merged_namespace_symbol;
import :compilation;
import :symbols.visit;

namespace prism
{

    const NamespaceSymbol &MergedNamespaceSymbol::create(const Compilation &compilation,
                                                         const NamespaceSymbol *containing_namespace,
                                                         std::vector<Ref<const NamespaceSymbol>> namespaces)
    {
        DEBUG_ASSERT(!namespaces.empty(), "Cannot create merged namespace symbol with no namespaces");

        if (namespaces.size() == 1)
            return namespaces.front();

        auto &lifetime = CompilationInternal::get_lifetime(compilation);
        auto name = namespaces.front()->name();
        return lifetime.create<MergedNamespaceSymbol>(name, containing_namespace, compilation, std::move(namespaces));
    }

    const ImmutableArray<Location> &MergedNamespaceSymbol::locations() const
    {
        return locations_.get_or_compute(
            [this]
            {
                return namespaces_ |
                       std::views::transform([](const NamespaceSymbol &n) -> auto & { return n.locations(); }) |
                       std::views::join | std::ranges::to<ImmutableArray>();
            });
    }

    std::span<const SyntaxReference> MergedNamespaceSymbol::declaring_syntax_references() const
    {
        return syntax_references_.get_or_compute(
            [this]
            {
                return namespaces_ |
                       std::views::transform([](const NamespaceSymbol &n) { return n.declaring_syntax_references(); }) |
                       std::views::join | std::ranges::to<std::vector>();
            });
    }

    Optional<const Compilation &> MergedNamespaceSymbol::containing_compilation() const noexcept
    {
        return compilation_;
    }

    SymbolSpan<Symbol> MergedNamespaceSymbol::members() const
    {
        return members_.get_or_compute(
            [this]
            {
                std::unordered_map<Name, std::vector<Ref<const NamespaceSymbol>>> namespace_groups;

                for (const auto sym : namespaces_ |
                                          std::views::transform([](const NamespaceSymbol &n) { return n.members(); }) |
                                          std::views::join)
                {
                    if (auto ns = sym->as<NamespaceSymbol>(); ns.has_value())
                    {
                        namespace_groups[sym->name()].emplace_back(*ns);
                    }
                }

                std::unordered_set<Name> emitted_namespaces;
                std::vector<Ref<const Symbol>> merged_members;
                for (const auto member :
                     namespaces_ | std::views::transform([](const NamespaceSymbol &n) { return n.members(); }) |
                         std::views::join)
                {
                    if (auto ns = member->as<NamespaceSymbol>(); ns.has_value())
                    {
                        if (emitted_namespaces.insert(member->name()).second)
                        {
                            merged_members.emplace_back(
                                create(compilation_, this, std::move(namespace_groups[member->name()])));
                        }

                        continue;
                    }

                    merged_members.emplace_back(member);
                }

                return merged_members;
            });
    }

    SymbolSpan<Symbol> MergedNamespaceSymbol::members(Name name) const
    {
        Lazy<ImmutableArray<Ref<const Symbol>>> *lazy_array;
        {
            std::scoped_lock lock{name_to_members_mutex_};
            lazy_array = &name_to_members_[name];
        }

        return lazy_array->get_or_compute([this, name] { return compute_members(name); });
    }

    void MergedNamespaceSymbol::force_complete(const Optional<SourceLocation> &location,
                                               const Optional<SymbolPredicate> &filter) const
    {
        for (auto part : namespaces_)
        {
            SymbolInternal::force_complete(part, location, filter);
        }
    }

    ImmutableArray<Ref<const Symbol>> MergedNamespaceSymbol::compute_members(const Name name) const
    {
        return namespaces_ | std::views::transform([&](const NamespaceSymbol &n) { return n.members(name); }) |
               std::views::join | std::ranges::to<ImmutableArray>();
    }
} // namespace prism
