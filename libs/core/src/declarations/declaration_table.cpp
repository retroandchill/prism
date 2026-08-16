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
import :semantic.compilation;

namespace prism
{
    class DeclarationTable::Cache final
    {
      public:
        explicit Cache(const DeclarationTable &table) : old_roots_{table.old_roots_}
        {
        }

        const MergedNamespaceDeclaration &merged_root() const
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

    DeclarationTable::Builder::Builder(RefCountPtr<const DeclarationTable> table)
        : table_{table != nullptr ? std::move(table) : empty()}
    {
    }

    void DeclarationTable::Builder::add_root_declaration(LazyRootNamespace root)
    {
        realize_removes();
        added_roots_.push_back(std::move(root));
    }

    void DeclarationTable::Builder::remove_root_declaration(LazyRootNamespace root)
    {
        realize_adds();
        removed_roots_.push_back(std::move(root));
    }

    RefCountPtr<const DeclarationTable> DeclarationTable::Builder::build() &&
    {
        realize_adds();
        realize_removes();

        return std::move(table_);
    }

    void DeclarationTable::Builder::realize_adds()
    {
        if (added_roots_.empty())
            return;

        auto last_declaration = std::move(added_roots_.back());
        if (added_roots_.size() == 1)
        {
            if (!table_->latest_lazy_root_declaration_.has_value())
            {
                table_ = make_ref_counted<const DeclarationTable>(construct_tag,
                                                                  table_->old_roots_,
                                                                  std::move(last_declaration),
                                                                  table_->cache_);
            }
            else
            {
                table_ = make_ref_counted<const DeclarationTable>(
                    construct_tag,
                    table_->old_roots_.add(*table_->latest_lazy_root_declaration_),
                    std::move(last_declaration),
                    table_->cache_);
            }
        }
        else
        {
            added_roots_.pop_back();

            if (table_->latest_lazy_root_declaration_.has_value())
            {
                added_roots_.insert(added_roots_.begin(), *table_->latest_lazy_root_declaration_);
            }

            auto new_old_roots = table_->old_roots_.add_range(added_roots_ | std::views::as_rvalue);
        }

        added_roots_.clear();
    }

    void DeclarationTable::Builder::realize_removes()
    {
        if (removed_roots_.empty())
            return;

        if (removed_roots_.size() == 1)
        {
            auto first_declaration = std::move(removed_roots_.front());
            if (table_->latest_lazy_root_declaration_ == first_declaration)
            {
                table_ =
                    make_ref_counted<DeclarationTable>(construct_tag, table_->old_roots_, std::nullopt, table_->cache_);
            }
            else
            {
                table_ = make_ref_counted<DeclarationTable>(construct_tag,
                                                            table_->old_roots_.remove(first_declaration),
                                                            std::move(first_declaration),
                                                            table_->cache_);
            }
        }
        else
        {
            auto is_latest_removed = table_->latest_lazy_root_declaration_.has_value() &&
                                     std::ranges::contains(removed_roots_, *table_->latest_lazy_root_declaration_);

            auto new_old_roots = table_->old_roots_.remove_range(removed_roots_);
            auto new_latest = !is_latest_removed ? table_->latest_lazy_root_declaration_ : std::nullopt;

            table_ = make_ref_counted<DeclarationTable>(construct_tag,
                                                        std::move(new_old_roots),
                                                        std::move(new_latest),
                                                        table_->cache_);
        }

        removed_roots_.clear();
    }

    DeclarationTable::DeclarationTable(ConstructTag,
                                       ImmutableOrderedSet<LazyRootNamespace> old_roots,
                                       Optional<LazyRootNamespace> latest_lazy_root,
                                       std::shared_ptr<Cache> cache)
        : old_roots_{std::move(old_roots)}, latest_lazy_root_declaration_{std::move(latest_lazy_root)},
          cache_{cache != nullptr ? std::move(cache) : std::make_shared<Cache>(*this)}
    {
    }

    const RefCountPtr<const DeclarationTable> &DeclarationTable::empty()
    {
        static auto empty_table = make_ref_counted<const DeclarationTable>(construct_tag,
                                                                           ImmutableOrderedSet<LazyRootNamespace>{},
                                                                           std::nullopt,
                                                                           nullptr);
        return empty_table;
    }

    DeclarationTable::Builder DeclarationTable::to_builder() const
    {
        return Builder{shared_from_this()};
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
        auto &old_root = cache_->merged_root();
        if (!latest_lazy_root_declaration_.has_value())
        {
            return old_root.shared_from_this();
        }

        auto &old_declarations = old_root.declarations();
        PooledVector<RefCountPtr<const SingleNamespaceDeclaration>> new_declarations;
        new_declarations.reserve(old_declarations.size() + 1);
        new_declarations.append_range(old_declarations);
        new_declarations.emplace_back(latest_lazy_root_declaration_->get());
        std::ranges::sort(new_declarations,
                          [&compilation](const RefCountPtr<const SingleNamespaceDeclaration> &lhs,
                                         const RefCountPtr<const SingleNamespaceDeclaration> &rhs)
                          {
                              return compilation.compare_source_locations(lhs->syntax_reference(),
                                                                          rhs->syntax_reference()) ==
                                     std::strong_ordering::less;
                          });
        return MergedNamespaceDeclaration::create(
            ImmutableArray{std::from_range, new_declarations | std::views::as_rvalue});
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
