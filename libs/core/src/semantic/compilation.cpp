/**
 * @file compilation.cpp
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module prism.core:semantic.compilation.impl;

import :semantic.compilation;
import :binder.declaration_binder;
import :binder.declaration_merger;
import :diagnostics.diagnostic_bag;
import :binder.declaration_scope;
import :binder.declaration_scope_builder;
import :symbols.assembly_symbol;
import :symbols.merged_namespace_symbol;
import :symbols.intrinsic;
import :binder.signature_binder;
import :symbols.error;
import :semantic.conversion_classifier;

namespace prism
{

    Compilation::Compilation(CreateTag,
                             std::vector<std::shared_ptr<SyntaxTree>> trees,
                             const TargetSettings target_settings) noexcept
        : target_settings_{target_settings}, trees_{std::move(trees)}
    {
    }

    std::shared_ptr<Compilation> Compilation::create(const Name assembly_name,
                                                     std::vector<std::shared_ptr<SyntaxTree>> trees,
                                                     const TargetSettings target_settings)
    {
        if (trees.empty())
            throw std::invalid_argument{"Cannot create a compilation with 0 syntax trees"};

        auto compilation = std::make_shared<Compilation>(CreateTag{}, std::move(trees), target_settings);

        auto declaration_records =
            compilation->trees_ |
            std::views::transform([](const auto &tree) { return DeclarationBinder{*tree}.bind(); }) | std::views::join |
            std::ranges::to<std::vector>();

        DiagnosticBag diagnostics{16};
        auto &lifetime = *compilation->lifetime_;
        auto &mappings = compilation->semantic_mappings_;
        std::vector<PartiallyBoundSymbol> partially_bound;
        compilation->assembly_ =
            &DeclarationMerger{assembly_name, lifetime, mappings, partially_bound}.merge(declaration_records);

        std::vector<Ref<const NamespaceSymbol>> global_namespaces;
        global_namespaces.reserve(2);
        global_namespaces.emplace_back(compilation->assembly().global_namespace());
        global_namespaces.emplace_back(IntrinsicSymbols::instance().global_namespace());
        compilation->global_namespace_ =
            &MergedNamespaceSymbol::create(*compilation, nullptr, std::move(global_namespaces));

        const DeclarationScopeBuilder declaration_scopes{lifetime,
                                                         *compilation->global_namespace_,
                                                         diagnostics,
                                                         mappings};
        for (const auto &tree : trees)
        {
            declaration_scopes.add(*tree);
        }

        SignatureBinder{*compilation, diagnostics}.bind(partially_bound);
        diagnostics.move_to(compilation->diagnostics_);

        return compilation;
    }

    const SemanticModel &Compilation::get_semantic_model(const SyntaxTree &tree) const
    {
        std::scoped_lock lock{semantic_models_mutex_};
        if (const auto it = semantic_models_.find(&tree); it != semantic_models_.end())
            return *it->second;

        auto &model = lifetime_->create<SemanticModel>(SemanticModel::create_tag, *this, tree);
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

        auto &error_type = lifetime_->create<ErrorTypeSymbol>(name, container.value_ptr());
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

        auto &error_namespace = lifetime_->create<ErrorNamespaceSymbol>(name, container.value_ptr());
        error_namespaces_.emplace(lookup_key, &error_namespace);
        return error_namespace;
    }

    Conversion Compilation::classify_conversion(const TypeSymbol &source, const TypeSymbol &destination) const
    {
        const ConversionClassifier classifier{target_settings_};
        return classifier.classify_conversion(source, destination);
    }
} // namespace prism
