/**
 * @file declaration_table.cpp
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
module prism.core:declarations.declaration_table.impl;

import :declarations.declaration_table;
import :declarations.single_root_namespace_declaration;
import :declarations.visit;

namespace prism
{
    class DeclarationTable::Cache final
    {
      public:
        explicit Cache(const DeclarationTable &table) : old_roots_{table.old_roots_}
        {
        }

        const MergedDeclaration &merged_root() const
        {
            return *merged_root_.get_or_compute(
                [this]
                {
                    return MergedNamespaceDeclaration::create(
                        old_roots_ |
                        std::views::transform(
                            [](const LazyRootNamespace &lazy_root)
                            { return static_pointer_cast<const SingleNamespaceDeclaration>(lazy_root.get()); }) |
                        std::ranges::to<ImmutableArray>());
                });
        }

        const ImmutableHashSet<Name> &type_names() const
        {
            return type_names_.get_or_compute([this] { return get_type_names(merged_root()); });
        }

        const ImmutableHashSet<Name> &namespace_names() const
        {
            return namespace_names_.get_or_compute([this] { return get_namespace_names(merged_root()); });
        }

      private:
        ImmutableOrderedSet<LazyRootNamespace> old_roots_;
        mutable Lazy<RefCountPtr<const MergedNamespaceDeclaration>> merged_root_;
        mutable Lazy<ImmutableHashSet<Name>> type_names_;
        mutable Lazy<ImmutableHashSet<Name>> namespace_names_;
    };

    DeclarationTable::DeclarationTable(ConstructTag,
                                       ImmutableOrderedSet<LazyRootNamespace> old_roots,
                                       Optional<LazyRootNamespace> latest_lazy_root,
                                       std::shared_ptr<Cache> cache)
        : old_roots_{std::move(old_roots)}, latest_lazy_root_declaration_{std::move(latest_lazy_root)},
          cache_{cache != nullptr ? std::move(cache) : std::make_shared<Cache>(*this)}
    {
    }

    const MergedNamespaceDeclaration &DeclarationTable::get_merged_root(const Compilation &compilation) const
    {
        return *merged_root_.get_or_compute([&] { return calculate_merged_root(compilation); });
    }

    const ImmutableHashSet<Name> &DeclarationTable::type_names() const
    {
        return type_names_.get_or_compute([&] { return get_merged_type_names(); });
    }

    const ImmutableHashSet<Name> &DeclarationTable::namespace_names() const
    {
        return namespace_names_.get_or_compute([&] { return get_merged_namespace_names(); });
    }

    RefCountPtr<const MergedNamespaceDeclaration> DeclarationTable::calculate_merged_root(
        const Compilation &compilation) const
    {
        throw NotImplementedException{};
    }

    ImmutableHashSet<Name> DeclarationTable::get_merged_type_names() const
    {
        auto &cached_names = cache_->type_names();

        if (!latest_lazy_root_declaration_.has_value())
            return cached_names;

        return cached_names.add_range(get_type_names(*latest_lazy_root_declaration_->get()));
    }

    ImmutableHashSet<Name> DeclarationTable::get_merged_namespace_names() const
    {
        auto &cached_names = cache_->namespace_names();

        if (!latest_lazy_root_declaration_.has_value())
            return cached_names;

        return cached_names.add_range(get_namespace_names(*latest_lazy_root_declaration_->get()));
    }

    ImmutableHashSet<Name> DeclarationTable::get_type_names(const Declaration &declaration)
    {
        return get_names(declaration, [](const Declaration &d) { return d.kind() != DeclarationKind::namespace_; });
    }

    ImmutableHashSet<Name> DeclarationTable::get_namespace_names(const Declaration &declaration)
    {
        return get_names(declaration, [](const Declaration &d) { return d.kind() == DeclarationKind::namespace_; });
    }

    template <std::predicate<const Declaration &> Predicate>
    ImmutableHashSet<Name> DeclarationTable::get_names(const Declaration &declaration, Predicate &&predicate)
    {
        PooledSet<Name> set;
        PooledVector<Ref<const Declaration>> stack;
        stack.emplace_back(declaration);

        while (!stack.empty())
        {
            auto current = stack.back();
            stack.pop_back();

            if (std::invoke(predicate, current))
            {
                set.insert(current->name());
            }

            prism::visit(declaration,
                         [&](const auto &child)
                         {
                             for (const auto &member : child.members())
                             {
                                 stack.emplace_back(*member);
                             }
                         });
        }

        return ImmutableHashSet{std::from_range, set};
    }
} // namespace prism
