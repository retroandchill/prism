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

    std::vector<Location> MergedNamespaceDeclaration::name_locations() const
    {
        return declarations_ |
               std::views::transform([](const SingleDeclaration &declaration) -> auto &
                                     { return declaration.name_location(); }) |
               std::ranges::to<std::vector<Location>>();
    }

    std::span<const Ref<const Declaration>> MergedNamespaceDeclaration::get_declaration_children() const
    {
        return children_.get_or_compute([this] { return make_children(); });
    }

    std::span<const Ref<const Declaration>> MergedNamespaceDeclaration::make_children() const
    {
        PooledVector<Ref<const SingleNamespaceDeclaration>> namespaces;
        bool all_namespaces_has_same_name = true;

        for (auto &child :
             declarations_ |
                 std::views::transform([](const SingleDeclaration &declaration) { return declaration.children(); }) |
                 std::views::join)
        {
            if (const auto as_namespace = dynamic_cast<const SingleNamespaceDeclaration *>(&child);
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

        return lifetime().copy_refs<const Declaration>(children);
    }
} // namespace prism
