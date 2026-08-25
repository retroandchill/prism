/**
 * @file compilation.cpp
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:compilation.impl;

import :compilation;
import :diagnostics.diagnostic_bag;
import :symbols.assembly_symbol;
import :symbols.merged_namespace_symbol;
import :symbols.intrinsic;
import :symbols.error;
import :semantic.conversion_classifier;
import :symbols.source;
import :binder.terminal_binder;
import :symbols.namespace_symbol;
import :binder.binder_factory;
import :symbols.visit;
import :syntax.visit;
import :binder.lookup_context;
import :semantic.syntax_and_declaration_manager;
import :codegen.llvm_emitter;

namespace prism
{
    class Compilation::Cache final
    {
      public:
        explicit Cache(const Compilation &compilation) : compilation_{compilation}
        {
        }

        [[nodiscard]] Optional<const NamespaceSymbol &> get_compilation_namespace(const NamespaceSymbol &symbol)
        {
            if (symbol.namespace_kind() == NamespaceKind::compilation &&
                symbol.containing_compilation().value_ptr() == &compilation_)
            {
                return symbol;
            }

            {
                std::scoped_lock lock{compilation_namespace_mutex_};
                if (const auto it = compilation_namespaces_.find(&symbol); it != compilation_namespaces_.end())
                {
                    return it->second;
                }
            }

            auto containing_namespace = symbol.containing_namespace();
            if (!containing_namespace.has_value())
            {
                return compilation_.global_namespace();
            }

            auto current = get_compilation_namespace(*containing_namespace);
            if (!current.has_value())
            {
                return std::nullopt;
            }

            const auto found = current->get_nested_namespace(symbol.name());
            DEBUG_ASSERT(found.has_value());
            // It's fine if we overwrite an existing entry, since we're not creating any new symbol objects
            // so there shouldn't be any leaks if two threads write to the same key.
            std::scoped_lock lock{compilation_namespace_mutex_};
            compilation_namespaces_.emplace(&symbol, &*found);
            return found;
        }

        [[nodiscard]] const SemanticModel &get_semantic_model(const SyntaxTree &tree)
        {
            std::scoped_lock lock{semantic_models_mutex_};
            if (const auto it = semantic_models_.find(&tree); it != semantic_models_.end())
                return *it->second;

            auto &model = SemanticModelInternal::create(compilation_, tree);
            semantic_models_.emplace(&tree, &model);
            return model;
        }

        [[nodiscard]] const NamedTypeSymbol &create_error_type_symbol(const Optional<const Symbol &> container,
                                                                      Name name)
        {
            SymbolLookupKey lookup_key{.symbol = container.value_ptr(), .name = name};
            std::scoped_lock lock{error_type_mutex_};
            if (const auto it = error_types_.find(lookup_key); it != error_types_.end())
                return *it->second;

            auto &error_type = compilation_.lifetime_.create<ErrorTypeSymbol>(name, container.value_ptr());
            error_types_.emplace(lookup_key, &error_type);
            return error_type;
        }

        const NamespaceSymbol &create_error_namespace_symbol(const Optional<const NamespaceSymbol &> container,
                                                             Name name)
        {
            SymbolLookupKey lookup_key{.symbol = container.value_ptr(), .name = name};
            std::scoped_lock lock{error_namespace_mutex_};
            if (const auto it = error_namespaces_.find(lookup_key); it != error_namespaces_.end())
                return *it->second;

            auto &error_namespace = compilation_.lifetime_.create<ErrorNamespaceSymbol>(name, container.value_ptr());
            error_namespaces_.emplace(lookup_key, &error_namespace);
            return error_namespace;
        }

        const BinderFactory &get_binder_factory(const SyntaxTree &tree)
        {
            Lazy<const BinderFactory &> *factory;
            {
                std::scoped_lock lock{binder_factory_mutex_};
                factory = &binder_factories_[&tree];
            }

            return factory->get_or_compute(
                [this, &tree] -> auto & { return compilation_.lifetime_.create<BinderFactory>(compilation_, tree); });
        }

        const Binder &root_binder()
        {
            return root_binder_.get_or_compute([this] -> auto &
                                               { return compilation_.lifetime_.create<TerminalBinder>(compilation_); });
        }

        [[nodiscard]] const ImmutableArray<Ref<const VariableSymbol>> &get_global_variables()
        {
            return top_level_variables_.get_or_compute(
                [this]
                {
                    PooledVector<Ref<const VariableSymbol>> variables;
                    collect_global_variables(compilation_.assembly().global_namespace(), variables);
                    return ImmutableArray{std::from_range, variables};
                });
        }

        [[nodiscard]] const ImmutableArray<Ref<const FunctionSymbol>> &get_global_functions()
        {
            return top_level_functions_.get_or_compute(
                [this]
                {
                    PooledVector<Ref<const FunctionSymbol>> variables;
                    collect_global_functions(compilation_.assembly().global_namespace(), variables);
                    return ImmutableArray{std::from_range, variables};
                });
        }

      private:
        static void collect_global_variables(const NamespaceSymbol &ns,
                                             PooledVector<Ref<const VariableSymbol>> &variables)
        {
            for (auto member : ns.members())
            {
                visit(member,
                      Overload{[&](const VariableSymbol &variable) { variables.push_back(variable); },
                               [&](const NamespaceSymbol &namespace_)
                               { collect_global_variables(namespace_, variables); },
                               [](const Symbol &)
                               {
                                   // Ignore other symbols
                               }});
            }
        }

        static void collect_global_functions(const NamespaceSymbol &ns,
                                             PooledVector<Ref<const FunctionSymbol>> &functions)
        {
            for (auto member : ns.members())
            {
                visit(member,
                      Overload{[&](const FunctionSymbol &function) { functions.push_back(function); },
                               [&](const NamespaceSymbol &namespace_)
                               { collect_global_functions(namespace_, functions); },
                               [](const Symbol &)
                               {
                                   // Ignore other symbols
                               }});
            }
        }

        const Compilation &compilation_;
        std::mutex compilation_namespace_mutex_;
        std::unordered_map<const NamespaceSymbol *, const NamespaceSymbol *> compilation_namespaces_;

        std::mutex binder_factory_mutex_;
        std::unordered_map<const SyntaxTree *, Lazy<const BinderFactory &>> binder_factories_;
        Lazy<const Binder &> root_binder_;

        std::mutex semantic_models_mutex_;
        std::unordered_map<const SyntaxTree *, SemanticModel *> semantic_models_;

        std::mutex error_type_mutex_;
        std::unordered_map<SymbolLookupKey, const NamedTypeSymbol *> error_types_;

        std::mutex error_namespace_mutex_;
        std::unordered_map<SymbolLookupKey, const NamespaceSymbol *> error_namespaces_;

        Lazy<ImmutableArray<Ref<const VariableSymbol>>> top_level_variables_;
        Lazy<ImmutableArray<Ref<const FunctionSymbol>>> top_level_functions_;
    };

    Compilation::Compilation(CreateTag,
                             SemanticLifetime &lifetime,
                             const Name assembly_name,
                             const TargetSettings target_settings,
                             RefCountPtr<SyntaxAndDeclarationManager> syntax_and_declarations) noexcept
        : lifetime_{lifetime}, assembly_name_{assembly_name}, target_settings_{target_settings},
          syntax_and_declaration_manager_{std::move(syntax_and_declarations)}, cache_{lifetime_.create<Cache>(*this)}
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

    const NamespaceSymbol &Compilation::global_namespace() const
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

    Optional<const NamespaceSymbol &> Compilation::get_compilation_namespace(const NamespaceSymbol &symbol) const
    {
        return cache_.get_compilation_namespace(symbol);
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
        return cache_.get_semantic_model(tree);
    }

    // ReSharper disable once CppMemberFunctionMayBeStatic
    const NamedTypeSymbol &Compilation::get_special_type(const SpecialType type) const
    {
        return IntrinsicSymbols::instance().get_type(type);
    }

    const NamedTypeSymbol &Compilation::create_error_type_symbol(const Optional<const Symbol &> container,
                                                                 const Name name) const
    {
        return cache_.create_error_type_symbol(container, name);
    }

    const NamespaceSymbol &Compilation::create_error_namespace_symbol(const Optional<const NamespaceSymbol &> container,
                                                                      const Name name) const
    {
        return cache_.create_error_namespace_symbol(container, name);
    }

    EmitResult Compilation::emit(std::filesystem::path output_directory) const
    {
        const LlvmEmitter emitter(*this, {.output_directory = std::move(output_directory)});
        return emitter.emit();
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
        return cache_.get_binder_factory(tree);
    }

    const Binder &Compilation::root_binder() const
    {
        return cache_.root_binder();
    }

    const ImmutableArray<Ref<const VariableSymbol>> &Compilation::get_global_variables() const
    {
        return cache_.get_global_variables();
    }

    const ImmutableArray<Ref<const FunctionSymbol>> &Compilation::get_global_functions() const
    {
        return cache_.get_global_functions();
    }

    Optional<const BoundExpression &> Compilation::get_bound_initializer(const VariableSymbol &symbol) const
    {
        const LookupContext context{declaration_diagnostics_};
        for (auto &ref : symbol.declaring_syntax_references())
        {
            auto &semantic_model = get_semantic_model(ref.tree());
            auto declaration = ref.syntax().as<VariableDeclarationSyntax>();
            if (!declaration.has_value())
                continue;

            return SemanticModelInternal::get_bound_initializer(semantic_model, *declaration, context);
        }

        return std::nullopt;
    }

    Optional<const BoundStatement &> Compilation::get_bound_body(const FunctionSymbol &symbol) const
    {
        const LookupContext context{declaration_diagnostics_};
        for (auto &ref : symbol.declaring_syntax_references())
        {
            auto &semantic_model = get_semantic_model(ref.tree());
            auto declaration = ref.syntax().as<FunctionDeclarationSyntax>();
            if (!declaration.has_value())
                continue;

            return SemanticModelInternal::get_bound_body(semantic_model, *declaration, context);
        }

        return std::nullopt;
    }
} // namespace prism
