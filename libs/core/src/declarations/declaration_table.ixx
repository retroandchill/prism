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

namespace prism
{
    class Compilation;

    class DeclarationTable final
    {
      public:
        [[nodiscard]] const MergedNamespaceDeclaration &get_merged_root(const Compilation &compilation) const;

      private:
        [[nodiscard]] RefCountPtr<const MergedNamespaceDeclaration> calculate_merged_root(
            const Compilation &compilation) const;

        mutable Lazy<RefCountPtr<const MergedNamespaceDeclaration>> merged_root_;
    };
} // namespace prism
