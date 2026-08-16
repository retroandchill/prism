/**
 * @file declaration_table.ixx
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
export module prism.core:declarations.declaration_table;

import :collections.immutable_array;
import :util.lazy;
import :util.ref;
import :memory.ref_counted_ptr;
import :declarations.merged_namespace_declaration;
import :util.lazy_value;
import :collections.immutable_ordered_set;

namespace prism
{
    class SingleRootNamespaceDeclaration;
    class Compilation;

    class DeclarationTable final
    {

        using LazyRootNamespace = LazyValue<RefCountPtr<const SingleRootNamespaceDeclaration>>;
        struct ConstructTag
        {
        };
        class Cache;

        static constexpr ConstructTag construct_tag;

      public:
        DeclarationTable(ConstructTag,
                         ImmutableOrderedSet<LazyRootNamespace> old_roots,
                         Optional<LazyRootNamespace> latest_lazy_root,
                         std::shared_ptr<Cache> cache);

        [[nodiscard]] const MergedNamespaceDeclaration &get_merged_root(const Compilation &compilation) const;
        [[nodiscard]] const ImmutableHashSet<Name> &type_names() const;
        [[nodiscard]] const ImmutableHashSet<Name> &namespace_names() const;

      private:
        [[nodiscard]] RefCountPtr<const MergedNamespaceDeclaration> calculate_merged_root(
            const Compilation &compilation) const;

        [[nodiscard]] ImmutableHashSet<Name> get_merged_type_names() const;
        [[nodiscard]] ImmutableHashSet<Name> get_merged_namespace_names() const;

        static ImmutableHashSet<Name> get_type_names(const Declaration &declaration);
        static ImmutableHashSet<Name> get_namespace_names(const Declaration &declaration);

        template <std::predicate<const Declaration &> Predicate>
        static ImmutableHashSet<Name> get_names(const Declaration &declaration, Predicate &&predicate);

        ImmutableOrderedSet<LazyRootNamespace> old_roots_;
        Optional<LazyRootNamespace> latest_lazy_root_declaration_;
        std::shared_ptr<Cache> cache_;

        mutable Lazy<RefCountPtr<const MergedNamespaceDeclaration>> merged_root_;
        mutable Lazy<ImmutableHashSet<Name>> type_names_;
        mutable Lazy<ImmutableHashSet<Name>> namespace_names_;
    };
} // namespace prism
