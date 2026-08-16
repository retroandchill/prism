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
import :declarations.visit;
import :util.overload;

namespace prism
{
    namespace
    {
        void add_namespace_to_children(const PooledVector<RefCountPtr<const SingleNamespaceDeclaration>> &namespaces,
                                       const bool all_namespaces_has_same_name,
                                       PooledVector<RefCountPtr<const MergedDeclaration>> &children)
        {
            if (namespaces.empty())
                return;

            if (all_namespaces_has_same_name)
            {

                children.emplace_back(MergedNamespaceDeclaration::create(ImmutableArray{std::from_range, namespaces}));
            }
            else
            {
                PooledMap<Name, PooledVector<RefCountPtr<const SingleNamespaceDeclaration>>> namespaces_by_name;
                for (auto &n : namespaces)
                {
                    namespaces_by_name[n->name()].push_back(n);
                }

                for (const auto &grouped : namespaces_by_name | std::views::values)
                {
                    children.emplace_back(MergedNamespaceDeclaration::create(ImmutableArray{std::from_range, grouped}));
                }
            }
        }
    } // namespace

    MergedNamespaceDeclaration::MergedNamespaceDeclaration(ConstructTag, SingleNamespaceList declarations)
        : MergedDeclaration{!declarations.empty() ? declarations.front()->name() : KnownName::none,
                            DeclarationKind::namespace_},
          declarations_{std::move(declarations)}
    {
    }

    RefCountPtr<const MergedNamespaceDeclaration> MergedNamespaceDeclaration::create(SingleNamespaceList declarations)
    {
        return make_ref_counted<const MergedNamespaceDeclaration>(construct_tag, std::move(declarations));
    }

    RefCountPtr<const MergedNamespaceDeclaration> MergedNamespaceDeclaration::create(
        const SingleNamespaceDeclaration &declaration)
    {
        return create(ImmutableArray{declaration.shared_from_this()});
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
               std::views::transform([](const RefCountPtr<const SingleNamespaceDeclaration> &declaration) -> auto &
                                     { return declaration->name_location(); }) |
               std::ranges::to<ImmutableArray<Location>>();
    }

    const ImmutableArray<RefCountPtr<const MergedDeclaration>> &MergedNamespaceDeclaration::members() const
    {
        return members_.get_or_compute([this] { return make_children(); });
    }

    const ImmutableHashSet<Name> &MergedNamespaceDeclaration::member_names() const
    {
        return member_names_.get_or_compute(
            [this]
            {
                return declarations_ |
                       std::views::transform(
                           [](const RefCountPtr<const SingleNamespaceDeclaration> &declaration) -> auto &
                           { return declaration->member_names(); }) |
                       std::views::join | std::ranges::to<ImmutableHashSet>();
            });
    }

    ImmutableArray<RefCountPtr<const MergedDeclaration>> MergedNamespaceDeclaration::make_children() const
    {
        PooledVector<RefCountPtr<const SingleNamespaceDeclaration>> namespaces;
        bool all_namespaces_has_same_name = true;

        for (const auto &child :
             declarations_ |
                 std::views::transform([](const RefCountPtr<const SingleNamespaceDeclaration> &declaration) -> auto &
                                       { return declaration->members(); }) |
                 std::views::join)
        {
            visit(*child,
                  Overload{[&](const SingleNamespaceDeclaration &as_namespace)
                           {
                               if (!namespaces.empty() && all_namespaces_has_same_name &&
                                   as_namespace.name() != namespaces.front()->name())
                               {
                                   all_namespaces_has_same_name = false;
                               }

                               namespaces.emplace_back(as_namespace.shared_from_this());
                           }});
        }

        PooledVector<RefCountPtr<const MergedDeclaration>> children;

        add_namespace_to_children(namespaces, all_namespaces_has_same_name, children);

        return ImmutableArray{std::from_range, children};
    }
} // namespace prism
