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
import :collections.immutable_ordered_set;
import :declarations.merged_namespace_declaration;
import :declarations.single_root_namespace_declaration;
import :util.lazy_value;

namespace prism
{
    using LazyRootNamespace = LazyValue<RefCountPtr<const SingleRootNamespaceDeclaration>>;

    /*
     * Struct to serve as a drop-in replacement for std::hash.
     *
     * @remarks This is because MSVC has an annoying bug that makes it hard to use partial
     *          specializations of std::hash and std::format directly.
     */
    struct LazyRootNamespaceHash
    {
        constexpr std::size_t operator()(const LazyRootNamespace &lazy_root) const noexcept
        {
            return hash_value(lazy_root);
        }
    };

    class DeclarationTable final : public IntrusiveRefCounted
    {

        struct ConstructTag
        {
        };
        class Cache;

        using OldRootSet = ImmutableOrderedSet<LazyRootNamespace, LazyRootNamespaceHash>;

        static constexpr ConstructTag construct_tag;

      public:
        class Builder
        {
          public:
            explicit Builder(RefCountPtr<const DeclarationTable> table = nullptr);

            void add_root_declaration(LazyRootNamespace root);
            void remove_root_declaration(LazyRootNamespace root);

            RefCountPtr<const DeclarationTable> build() &&;

          private:
            void realize_adds();
            void realize_removes();

            RefCountPtr<const DeclarationTable> table_{};
            std::vector<LazyRootNamespace> added_roots_{};
            std::vector<LazyRootNamespace> removed_roots_{};
        };

        DeclarationTable(ConstructTag,
                         OldRootSet old_roots,
                         Optional<LazyRootNamespace> latest_lazy_root,
                         std::shared_ptr<Cache> cache);

        static const RefCountPtr<const DeclarationTable> &empty();

        [[nodiscard]] Builder to_builder() const;

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

        OldRootSet old_roots_;
        Optional<LazyRootNamespace> latest_lazy_root_declaration_;
        std::shared_ptr<Cache> cache_;

        mutable Lazy<RefCountPtr<const MergedNamespaceDeclaration>> merged_root_;
        mutable Lazy<ImmutableHashSet<Name>> type_names_;
        mutable Lazy<ImmutableHashSet<Name>> namespace_names_;
    };
} // namespace prism
