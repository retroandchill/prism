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

namespace prism
{

    Compilation::Compilation(std::unique_ptr<SymbolLifetime> lifetime,
                             const AssemblySymbol &assembly,
                             std::vector<std::unique_ptr<SyntaxTree>> trees,
                             std::vector<Diagnostic> diagnostics) noexcept
        : lifetime_{std::move(lifetime)}, assembly_{assembly}, trees_{std::move(trees)},
          diagnostics_{std::move(diagnostics)}
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
        auto lifetime = std::make_unique<SymbolLifetime>();
        auto &assembly = DeclarationMerger{assembly_name, *lifetime}.merge(declaration_records);
        return std::unique_ptr<Compilation>{
            new Compilation{std::move(lifetime), assembly, std::move(trees), diagnostics.drain()}};
    }
} // namespace prism
