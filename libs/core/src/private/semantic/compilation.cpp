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

namespace prism
{

    Compilation::Compilation(std::unique_ptr<SemanticLifetime> lifetime,
                             const AssemblySymbol &assembly,
                             std::vector<std::unique_ptr<SyntaxTree>> trees,
                             std::vector<Diagnostic> diagnostics,
                             SemanticMappings semantic_mappings) noexcept
        : lifetime_{std::move(lifetime)}, assembly_{assembly}, trees_{std::move(trees)},
          diagnostics_{std::move(diagnostics)}, semantic_mappings_{std::move(semantic_mappings)}
    {
    }

    std::unique_ptr<Compilation> Compilation::create(const Name assembly_name,
                                                     std::vector<std::unique_ptr<SyntaxTree>> trees)
    {
        if (trees.empty())
            throw std::invalid_argument{"Cannot create a compilation with 0 syntax trees"};

        auto declaration_records =
            trees | std::views::transform([](const auto &tree) { return DeclarationBinder{*tree}.bind(); }) |
            std::views::join | std::ranges::to<std::vector>();

        DiagnosticBag diagnostics{16};
        SemanticMappings mappings;
        auto lifetime = std::make_unique<SemanticLifetime>();
        auto &assembly = DeclarationMerger{assembly_name, *lifetime, mappings}.merge(declaration_records);
        DeclarationScopeBuilder declaration_scopes{*lifetime, assembly.global_namespace(), diagnostics, mappings};
        for (const auto &tree : trees)
        {
            declaration_scopes.add(*tree);
        }

        return std::unique_ptr<Compilation>{
            new Compilation{std::move(lifetime), assembly, std::move(trees), diagnostics.drain(), std::move(mappings)}};
    }

    const DeclarationScope &Compilation::get_declaration_scope(const SyntaxNode &node) const
    {
        const auto scope = semantic_mappings_.get_scope(node);
        if (!scope.has_value())
            throw std::invalid_argument{"No declaration scope found for node"};

        return *scope;
    }
} // namespace prism
