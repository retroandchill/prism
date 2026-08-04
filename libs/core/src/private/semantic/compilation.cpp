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

namespace prism
{

    Compilation::Compilation(std::unique_ptr<SemanticLifetime> lifetime,
                             const AssemblySymbol &assembly,
                             std::vector<std::unique_ptr<SyntaxTree>> trees,
                             std::vector<Diagnostic> diagnostics,
                             DeclarationScopeMap declaration_scopes) noexcept
        : lifetime_{std::move(lifetime)}, assembly_{assembly}, trees_{std::move(trees)},
          diagnostics_{std::move(diagnostics)}, declaration_scopes_{std::move(declaration_scopes)}
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
        auto lifetime = std::make_unique<SemanticLifetime>();
        auto &assembly = DeclarationMerger{assembly_name, *lifetime}.merge(declaration_records);
        DeclarationScopeBuilder declaration_scopes{*lifetime};
        for (const auto &tree : trees)
        {
            declaration_scopes.add(*tree);
        }

        return std::unique_ptr<Compilation>{new Compilation{std::move(lifetime),
                                                            assembly,
                                                            std::move(trees),
                                                            diagnostics.drain(),
                                                            std::move(declaration_scopes).build()}};
    }

    const DeclarationScope &Compilation::get_declaration_scope(const SyntaxNode &node) const
    {
        if (const auto it = declaration_scopes_.find(&node); it != declaration_scopes_.end())
            return *it->second;

        throw std::invalid_argument{"Declaration scope not found"};
    }
} // namespace prism
