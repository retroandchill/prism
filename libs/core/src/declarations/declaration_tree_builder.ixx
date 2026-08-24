/**
 * @file declaration_tree_builder.ixx
 * @author Francesco Corso
 * @date 8/16/2026
 * @brief
 */
export module prism.core:declarations.declaration_tree_builder;

import std;
import :memory.ref_counted_ptr;
import :collections.immutable_array;
import :syntax.list;
import :syntax.tree;
import :collections.immutable_hash_set;
import :declarations.single_root_namespace_declaration;

namespace prism
{
    export class NamespaceDeclarationSyntax;
    export class CompilationUnitSyntax;
    export class DeclarationSyntax;

    class DeclarationTreeBuilder final
    {
        explicit DeclarationTreeBuilder(const SyntaxTree &syntax_tree);

      public:
        static RefCountPtr<const SingleRootNamespaceDeclaration> for_tree(const SyntaxTree &syntax_tree);

      private:
        RefCountPtr<const SingleRootNamespaceDeclaration> visit_tree();

        ImmutableArray<RefCountPtr<const SingleDeclaration>> visit_namespace_children(
            const SyntaxNode &node,
            SyntaxList<DeclarationSyntax> members);

        RefCountPtr<const SingleRootNamespaceDeclaration> visit_compilation_unit(
            const CompilationUnitSyntax &compilation_unit);
        RefCountPtr<const SingleDeclaration> visit_namespace_declaration(const NamespaceDeclarationSyntax &node);

        RefCountPtr<const SingleRootNamespaceDeclaration> create_single_root_namespace_declaration(
            const CompilationUnitSyntax &compilation_unit,
            ImmutableArray<RefCountPtr<const SingleDeclaration>> children);

        static ImmutableHashSet<Name> get_non_type_member_names(SyntaxList<DeclarationSyntax> members);

        const SyntaxTree &syntax_tree_;
        std::uint32_t current_type_index_ = 0;
    };
} // namespace prism
