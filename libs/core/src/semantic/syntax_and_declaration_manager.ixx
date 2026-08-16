/**
 * @file syntax_and_declaration_manager.ixx
 * @author Francesco Corso
 * @date 8/16/2026
 * @brief
 */
export module prism.core:semantic.syntax_and_declaration_manager;

import std;
import :collections.immutable_array;
import :collections.immutable_hash_map;
import :util.lazy;
import :declarations.declaration_table;
import :memory.buffer_pool;

namespace prism
{
    class DeclarationTable;
    class SingleRootNamespaceDeclaration;
    class SyntaxTree;

    class SyntaxAndDeclarationManager final : public IntrusiveRefCounted
    {
      public:
        struct State final
        {
            ImmutableArray<std::shared_ptr<const SyntaxTree>> syntax_trees{};
            ImmutableHashMap<const SyntaxTree *, std::uint32_t> ordinal_map{};
            ImmutableHashMap<const SyntaxTree *, LazyRootNamespace> root_namespaces{};
            RefCountPtr<const DeclarationTable> declaration_table{};
        };

        explicit SyntaxAndDeclarationManager(ImmutableArray<std::shared_ptr<const SyntaxTree>> external_syntax_trees);

        explicit SyntaxAndDeclarationManager(ImmutableArray<std::shared_ptr<const SyntaxTree>> external_syntax_trees,
                                             State state);

        [[nodiscard]] const State &state() const noexcept;

      private:
        static State create_state(std::span<const std::shared_ptr<const SyntaxTree>> external_syntax_trees);
        static void append_all_syntax_trees(PooledVector<std::shared_ptr<const SyntaxTree>> &trees_builder,
                                            std::shared_ptr<const SyntaxTree> syntax_tree,
                                            PooledMap<const SyntaxTree *, std::uint32_t> &ordinal_map_builder,
                                            PooledMap<const SyntaxTree *, LazyRootNamespace> &decl_map_builder,
                                            DeclarationTable::Builder &decl_table_builder);
        static void add_syntax_tree_to_declaration_map_and_table(
            const SyntaxTree &syntax_tree,
            PooledMap<const SyntaxTree *, LazyRootNamespace> &decl_map_builder,
            DeclarationTable::Builder &decl_table_builder);

        ImmutableArray<std::shared_ptr<const SyntaxTree>> external_syntax_trees_;
        mutable Lazy<State> state_{};
    };
} // namespace prism
