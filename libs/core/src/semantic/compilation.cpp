/**
 * @file compilation.cpp
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:semantic.compilation.impl;

import :semantic.compilation;
import :diagnostics.diagnostic_bag;
import :binder.declaration_scope;
import :binder.declaration_scope_builder;
import :symbols.assembly_symbol;
import :symbols.merged_namespace_symbol;
import :symbols.intrinsic;
import :binder.signature_binder;
import :symbols.error;
import :semantic.conversion_classifier;
import :symbols.source;
import :binder.terminal_binder;

namespace prism
{
    Compilation::Compilation(CreateTag,
                             SemanticLifetime &lifetime,
                             const Name assembly_name,
                             const TargetSettings target_settings,
                             RefCountPtr<SyntaxAndDeclarationManager> syntax_and_declarations) noexcept
        : lifetime_{lifetime}, assembly_name_{assembly_name}, target_settings_{target_settings},
          syntax_and_declaration_manager_{std::move(syntax_and_declarations)}
    {
    }

    std::shared_ptr<Compilation> Compilation::create(const Name assembly_name,
                                                     ImmutableArray<std::shared_ptr<const SyntaxTree>> trees,
                                                     const TargetSettings target_settings)
    {
        if (trees.empty())
            throw std::invalid_argument{"Cannot create a compilation with 0 syntax trees"};

        auto lifetime = std::make_shared<SemanticLifetime>();
        auto &compilation =
            lifetime->create<Compilation>(CreateTag{},
                                          *lifetime,
                                          assembly_name,
                                          target_settings,
                                          make_ref_counted<SyntaxAndDeclarationManager>(std::move(trees)));
        return std::shared_ptr<Compilation>{std::move(lifetime), &compilation};
    }

    const AssemblySymbol &Compilation::assembly() const
    {
        return assembly_.get_or_compute([this] -> auto & { return lifetime_.create<SourceAssemblySymbol>(*this); });
    }

    const NamespaceSymbol &Compilation::common_global_namespace() const
    {
        return global_namespace_.get_or_compute(
            [this] -> auto &
            {
                return MergedNamespaceSymbol::create(*this,
                                                     nullptr,
                                                     std::vector{Ref{assembly().global_namespace()},
                                                                 Ref{IntrinsicSymbols::instance().global_namespace()}});
            });
    }

    const ImmutableArray<std::shared_ptr<const SyntaxTree>> &Compilation::trees() const noexcept
    {
        return syntax_and_declaration_manager_->state().syntax_trees;
    }

    bool Compilation::contains_syntax_tree(const SyntaxTree &tree) const noexcept
    {
        return syntax_and_declaration_manager_->state().root_namespaces.contains(&tree);
    }

    const SemanticModel &Compilation::get_semantic_model(const SyntaxTree &tree) const
    {
        std::scoped_lock lock{semantic_models_mutex_};
        if (const auto it = semantic_models_.find(&tree); it != semantic_models_.end())
            return *it->second;

        auto &model = lifetime_.create<SemanticModel>(SemanticModel::create_tag, *this, tree);
        semantic_models_.emplace(&tree, &model);
        return model;
    }

    const DeclarationScope &Compilation::get_declaration_scope(const SyntaxNode &node) const
    {
        const auto scope = semantic_mappings_.get_scope(node);
        if (!scope.has_value())
            throw std::invalid_argument{"No declaration scope found for node"};

        return *scope;
    }

    // ReSharper disable once CppMemberFunctionMayBeStatic
    const NamedTypeSymbol &Compilation::get_special_type(const SpecialType type) const
    {
        return IntrinsicSymbols::instance().get_type(type);
    }

    const NamedTypeSymbol &Compilation::create_error_type_symbol(const Optional<const Symbol &> container,
                                                                 Name name) const
    {
        SymbolLookupKey lookup_key{.symbol = container.value_ptr(), .name = name};
        std::scoped_lock lock{error_type_mutex_};
        if (const auto it = error_types_.find(lookup_key); it != error_types_.end())
            return *it->second;

        auto &error_type = lifetime_.create<ErrorTypeSymbol>(name, container.value_ptr());
        error_types_.emplace(lookup_key, &error_type);
        return error_type;
    }

    const NamespaceSymbol &Compilation::create_error_namespace_symbol(const Optional<const NamespaceSymbol &> container,
                                                                      Name name) const
    {
        SymbolLookupKey lookup_key{.symbol = container.value_ptr(), .name = name};
        std::scoped_lock lock{error_namespace_mutex_};
        if (const auto it = error_namespaces_.find(lookup_key); it != error_namespaces_.end())
            return *it->second;

        auto &error_namespace = lifetime_.create<ErrorNamespaceSymbol>(name, container.value_ptr());
        error_namespaces_.emplace(lookup_key, &error_namespace);
        return error_namespace;
    }

    Conversion Compilation::classify_conversion(const TypeSymbol &source, const TypeSymbol &destination) const
    {
        return no_conversion;
        /*
        const ConversionClassifier classifier{target_settings_};
        return classifier.classify_conversion(source, destination);
        */
    }

    std::shared_ptr<Compilation> Compilation::shared_from_this() noexcept
    {
        return {lifetime_.shared_from_this(), this};
    }

    std::shared_ptr<const Compilation> Compilation::shared_from_this() const noexcept
    {
        return {lifetime_.shared_from_this(), this};
    }

    const DeclarationTable &Compilation::declarations() const
    {
        return *syntax_and_declaration_manager_->state().declaration_table;
    }

    const MergedNamespaceDeclaration &Compilation::merged_root_declaration() const
    {
        return declarations().get_merged_root(*this);
    }

    std::strong_ordering Compilation::compare_source_locations(const Location &lhs, const Location &rhs) const
    {
        return compare_source_locations(std::get<SourceLocation>(lhs), std::get<SourceLocation>(rhs));
    }

    std::strong_ordering Compilation::compare_source_locations(const SourceLocation &lhs,
                                                               const SourceLocation &rhs) const
    {
        if (const auto comparison = compare_syntax_tree_ordering(lhs.tree(), rhs.tree());
            comparison != std::strong_ordering::equal)
            return comparison;

        return lhs.source_span().start <=> rhs.source_span().start;
    }

    std::strong_ordering Compilation::compare_source_locations(const SyntaxReference &lhs,
                                                               const SyntaxReference &rhs) const
    {
        if (const auto comparison = compare_syntax_tree_ordering(lhs.tree(), rhs.tree());
            comparison != std::strong_ordering::equal)
            return comparison;

        return lhs.span().start <=> rhs.span().start;
    }

    std::strong_ordering Compilation::compare_source_locations(const SyntaxNode &lhs, const SyntaxNode &rhs) const
    {
        if (const auto comparison = compare_syntax_tree_ordering(lhs.tree(), rhs.tree());
            comparison != std::strong_ordering::equal)
            return comparison;

        return lhs.span().start <=> rhs.span().start;
    }

    std::strong_ordering Compilation::compare_syntax_tree_ordering(const SyntaxTree &lhs, const SyntaxTree &rhs) const
    {
        if (&lhs == &rhs)
            return std::strong_ordering::equal;

        DEBUG_ASSERT(contains_syntax_tree(lhs));
        DEBUG_ASSERT(contains_syntax_tree(rhs));
        return get_syntax_tree_ordinal(lhs) <=> get_syntax_tree_ordinal(rhs);
    }

    std::uint32_t Compilation::get_syntax_tree_ordinal(const SyntaxTree &tree) const
    {
        DEBUG_ASSERT(contains_syntax_tree(tree));
        return syntax_and_declaration_manager_->state().ordinal_map.get(&tree);
    }

    const BinderFactory &Compilation::get_binder_factory(const SyntaxTree &tree) const
    {
        std::scoped_lock lock{binder_factory_mutex_};
        if (const auto it = binder_factories_.find(&tree); it != binder_factories_.end())
            return *it->second;
    }

    const Binder &Compilation::root_binder() const
    {
        return root_binder_.get_or_compute([this] -> auto & { return lifetime_.create<TerminalBinder>(*this); });
    }
} // namespace prism
