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

namespace prism
{

    Compilation::Compilation(CreateTag, std::vector<std::unique_ptr<SyntaxTree>> trees) noexcept
        : trees_{std::move(trees)}
    {
    }

    std::unique_ptr<Compilation> Compilation::create(const Name assembly_name,
                                                     std::vector<std::unique_ptr<SyntaxTree>> trees)
    {
        if (trees.empty())
            throw std::invalid_argument{"Cannot create a compilation with 0 syntax trees"};

        auto compilation = std::make_unique<Compilation>(CreateTag{}, std::move(trees));

        auto declaration_records =
            compilation->trees_ |
            std::views::transform([](const auto &tree) { return DeclarationBinder{*tree}.bind(); }) | std::views::join |
            std::ranges::to<std::vector>();

        DiagnosticBag diagnostics{16};
        auto &lifetime = *compilation->lifetime_;
        auto &mappings = compilation->semantic_mappings_;
        compilation->assembly_ = &DeclarationMerger{assembly_name, lifetime, mappings}.merge(declaration_records);

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

        diagnostics.move_to(compilation->diagnostics_);

        return compilation;
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
} // namespace prism
