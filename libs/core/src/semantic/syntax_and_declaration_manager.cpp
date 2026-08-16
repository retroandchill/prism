/**
 * @file syntax_and_declaration_manager.cpp
 * @author Francesco Corso
 * @date 8/16/2026
 * @brief
 */
module prism.core:semantic.syntax_and_declaration_manager.impl;

import :semantic.syntax_and_declaration_manager;
import :syntax.tree;
import :declarations.declaration_tree_builder;

namespace prism
{

    SyntaxAndDeclarationManager::SyntaxAndDeclarationManager(
        ImmutableArray<std::shared_ptr<const SyntaxTree>> external_syntax_trees)
        : external_syntax_trees_{std::move(external_syntax_trees)}
    {
    }

    SyntaxAndDeclarationManager::SyntaxAndDeclarationManager(
        ImmutableArray<std::shared_ptr<const SyntaxTree>> external_syntax_trees,
        State state)
        : external_syntax_trees_{std::move(external_syntax_trees)}, state_{std::move(state)}
    {
    }

    const SyntaxAndDeclarationManager::State &SyntaxAndDeclarationManager::state() const noexcept
    {
        return state_.get_or_compute([this] { return create_state(external_syntax_trees_); });
    }

    SyntaxAndDeclarationManager::State SyntaxAndDeclarationManager::create_state(
        const std::span<const std::shared_ptr<const SyntaxTree>> external_syntax_trees)
    {
        PooledVector<std::shared_ptr<const SyntaxTree>> trees_builder;
        PooledMap<const SyntaxTree *, std::uint32_t> ordinal_map_builder;
        PooledMap<const SyntaxTree *, LazyRootNamespace> decl_map_builder;
        auto decl_table_builder = DeclarationTable::empty()->to_builder();

        for (auto &syntax_tree : external_syntax_trees)
        {
            append_all_syntax_trees(trees_builder,
                                    syntax_tree,
                                    ordinal_map_builder,
                                    decl_map_builder,
                                    decl_table_builder);
        }

        return State{.syntax_trees{std::from_range, std::move(trees_builder)},
                     .ordinal_map{std::from_range, std::move(ordinal_map_builder)},
                     .root_namespaces{std::from_range, std::move(decl_map_builder)},
                     .declaration_table{std::move(decl_table_builder).build()}};
    }

    void SyntaxAndDeclarationManager::append_all_syntax_trees(
        PooledVector<std::shared_ptr<const SyntaxTree>> &trees_builder,
        std::shared_ptr<const SyntaxTree> syntax_tree,
        PooledMap<const SyntaxTree *, std::uint32_t> &ordinal_map_builder,
        PooledMap<const SyntaxTree *, LazyRootNamespace> &decl_map_builder,
        DeclarationTable::Builder &decl_table_builder)
    {
        add_syntax_tree_to_declaration_map_and_table(*syntax_tree, decl_map_builder, decl_table_builder);
        ordinal_map_builder.emplace(syntax_tree.get(), trees_builder.size());
        trees_builder.push_back(std::move(syntax_tree));
    }

    void SyntaxAndDeclarationManager::add_syntax_tree_to_declaration_map_and_table(
        const SyntaxTree &syntax_tree,
        PooledMap<const SyntaxTree *, LazyRootNamespace> &decl_map_builder,
        DeclarationTable::Builder &decl_table_builder)
    {
        LazyRootNamespace lazy_root{[&syntax_tree]
                                    {
                                        return DeclarationTreeBuilder::for_tree(syntax_tree);
                                    }};
        decl_map_builder.emplace(&syntax_tree, lazy_root);
        decl_table_builder.add_root_declaration(std::move(lazy_root));
    }
} // namespace prism
