/**
 * @file merged_namespace_declaration.ixx
 * @author Francesco Corso
 * @date 8/13/2026
 * @brief
 */
export module prism.core:declarations.merged_namespace_declaration;

import :declarations.merged_declaration;
import :declarations.single_namespace_declaration;
import :util.lazy;
import :symbols.lexical_sort_key;

namespace prism
{
    class MergedNamespaceDeclaration final : public MergedDeclaration
    {
        struct ConstructTag
        {
        };

        static constexpr ConstructTag construct_tag;

      public:
        using SingleNamespaceList = ImmutableArray<RefCountPtr<const SingleNamespaceDeclaration>>;

        MergedNamespaceDeclaration(ConstructTag, SingleNamespaceList declarations);

        static RefCountPtr<const MergedNamespaceDeclaration> create(SingleNamespaceList declarations);

        static RefCountPtr<const MergedNamespaceDeclaration> create(const SingleNamespaceDeclaration &declaration);

        [[nodiscard]] LexicalSortKey get_lexical_sort_key(const Compilation &compilation) const;

        [[nodiscard]] ImmutableArray<Location> name_locations() const;

        [[nodiscard]] const SingleNamespaceList &declarations() const noexcept
        {
            return declarations_;
        }

        [[nodiscard]] const ImmutableArray<RefCountPtr<const MergedDeclaration>> &members() const;

        [[nodiscard]] const ImmutableHashSet<Name> &member_names() const;

      private:
        [[nodiscard]] ImmutableArray<RefCountPtr<const MergedDeclaration>> make_children() const;

        SingleNamespaceList declarations_;
        mutable Lazy<ImmutableArray<RefCountPtr<const MergedDeclaration>>> members_;
        mutable Lazy<ImmutableHashSet<Name>> member_names_;
    };
} // namespace prism
