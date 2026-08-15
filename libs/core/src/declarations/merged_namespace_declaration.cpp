/**
 * @file merged_namespace_declaration.cpp
 * @author Francesco Corso
 * @date 8/13/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:declarations.merged_namespace_declaration.impl;

import :declarations.merged_namespace_declaration;
import :memory.buffer_pool;

namespace prism
{
    namespace
    {
        void add_namespace_to_children(SemanticLifetime &lifetime,
                                       const PooledVector<Ref<const SingleNamespaceDeclaration>> &namespaces,
                                       const bool all_namespaces_has_same_name,
                                       PooledVector<Ref<const MergedDeclaration>> &children)
        {
            if (namespaces.empty())
                return;

            if (all_namespaces_has_same_name)
            {

                children.emplace_back(MergedNamespaceDeclaration::create(lifetime, namespaces));
            }
            else
            {
                PooledMap<Name, PooledVector<Ref<const SingleNamespaceDeclaration>>> namespaces_by_name;
                for (auto n : namespaces)
                {
                    namespaces_by_name[n->name()].push_back(n);
                }

                for (const auto &grouped : namespaces_by_name | std::views::values)
                {
                    children.emplace_back(MergedNamespaceDeclaration::create(lifetime, grouped));
                }
            }
        }
    } // namespace

    MergedNamespaceDeclaration::MergedNamespaceDeclaration(
        ConstructTag,
        SemanticLifetime &lifetime,
        const std::span<const Ref<const SingleNamespaceDeclaration>> declarations)
        : MergedDeclaration{lifetime, !declarations.empty() ? declarations.front()->name() : KnownName::none},
          declarations_{declarations}
    {
    }

    MergedNamespaceDeclaration &MergedNamespaceDeclaration::create(
        SemanticLifetime &lifetime,
        std::span<const Ref<const SingleNamespaceDeclaration>> declarations)
    {
        auto child_span = lifetime.copy_refs(declarations);
        return lifetime.create<MergedNamespaceDeclaration>(construct_tag, lifetime, child_span);
    }

    std::shared_ptr<MergedNamespaceDeclaration> MergedNamespaceDeclaration::create(
        const SingleNamespaceDeclaration &declaration)
    {
        const auto lifetime = std::make_shared<SemanticLifetime>();
        std::array elements = {Ref{declaration}};
        return create(*lifetime, elements).shared_from_this();
    }

    LexicalSortKey MergedNamespaceDeclaration::get_lexical_sort_key(const Compilation &compilation) const
    {
        LexicalSortKey sort_key{declarations_[0]->name_location(), compilation};
        for (const auto declaration : declarations_ | std::views::drop(1))
        {
            sort_key = LexicalSortKey::first(sort_key, LexicalSortKey{declaration->name_location(), compilation});
        }

        return sort_key;
    }

    ImmutableArray<Location> MergedNamespaceDeclaration::name_locations() const
    {
        return declarations_ |
               std::views::transform([](const SingleDeclaration &declaration) -> auto &
                                     { return declaration.name_location(); }) |
               std::ranges::to<ImmutableArray<Location>>();
    }

    std::span<const Ref<const MergedDeclaration>> MergedNamespaceDeclaration::members() const
    {
        return members_.get_or_compute([this] { return make_children(); });
    }

    const ImmutableHashSet<Name> &MergedNamespaceDeclaration::member_names() const
    {
        return member_names_.get_or_compute(
            [this]
            {
                return declarations_ |
                       std::views::transform([](const SingleNamespaceDeclaration &declaration) -> auto &
                                             { return declaration.member_names(); }) |
                       std::views::join | std::ranges::to<ImmutableHashSet>();
            });
    }

    std::span<const Ref<const MergedDeclaration>> MergedNamespaceDeclaration::make_children() const
    {
        PooledVector<Ref<const SingleNamespaceDeclaration>> namespaces;
        bool all_namespaces_has_same_name = true;

        for (const auto child : declarations_ |
                                    std::views::transform([](const SingleNamespaceDeclaration &declaration)
                                                          { return declaration.members(); }) |
                                    std::views::join)
        {
            if (const auto as_namespace = dynamic_cast<const SingleNamespaceDeclaration *>(&child.get());
                as_namespace != nullptr)
            {
                if (!namespaces.empty() && all_namespaces_has_same_name &&
                    as_namespace->name() != namespaces.front()->name())
                {
                    all_namespaces_has_same_name = false;
                }

                namespaces.emplace_back(*as_namespace);
                continue;
            }

            PANIC("If we got here then some strange type got through that we don't want");
        }

        PooledVector<Ref<const MergedDeclaration>> children;

        add_namespace_to_children(lifetime(), namespaces, all_namespaces_has_same_name, children);

        return lifetime().copy_refs(children);
    }
} // namespace prism
