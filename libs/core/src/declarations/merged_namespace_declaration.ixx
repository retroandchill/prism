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
        MergedNamespaceDeclaration(ConstructTag,
                                   SemanticLifetime &lifetime,
                                   std::span<const Ref<const SingleNamespaceDeclaration>> declarations);

        static MergedNamespaceDeclaration &create(SemanticLifetime &lifetime,
                                                  std::span<const Ref<const SingleNamespaceDeclaration>> declarations);

        static std::shared_ptr<MergedNamespaceDeclaration> create(const SingleNamespaceDeclaration &declaration);

        [[nodiscard]] constexpr DeclarationKind kind() const noexcept override
        {
            return DeclarationKind::namespace_;
        }

        [[nodiscard]] LexicalSortKey get_lexical_sort_key(const Compilation &compilation) const;

        [[nodiscard]] std::vector<Location> name_locations() const;

        [[nodiscard]] std::span<const Ref<const SingleNamespaceDeclaration>> declarations() const noexcept
        {
            return declarations_;
        }

        [[nodiscard]] std::span<const Ref<const MergedDeclaration>> members() const;

      private:
        [[nodiscard]] std::span<const Ref<const MergedDeclaration>> make_children() const;

        std::span<const Ref<const SingleNamespaceDeclaration>> declarations_;
        mutable Lazy<std::span<const Ref<const MergedDeclaration>>> members_;
    };
} // namespace prism
