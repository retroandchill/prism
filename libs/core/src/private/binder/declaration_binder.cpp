/**
 * @file declaration_binder.cpp
 * @author Francesco Corso
 * @date 8/1/2026
 * @brief
 */
module prism.core:binder.declaration_binder.impl;

import :binder.declaration_binder;
import :symbols.symbol_lifetime;
import :symbols.source;
import :syntax.tree;
import :syntax.visit;
import :util.overload;

namespace prism
{
    DeclarationBinder::DeclarationBinder(Name assembly_name, const SyntaxTree &tree, SymbolLifetime &lifetime) noexcept
        : tree_{tree}, lifetime_{lifetime}, assembly_symbol_{lifetime.create<SourceAssemblySymbol>(assembly_name)},
          global_namespace_{lifetime.create<SourceNamespaceSymbol>(KnownName::global, &assembly_symbol_)}
    {
    }

    const AssemblySymbol &DeclarationBinder::bind()
    {
        visit(tree_.root(),
              Overload{[this](const DeclarationSyntax &syntax)
                       {
                           auto symbol = bind_declaration(syntax);
                           if (symbol.has_value())
                               global_namespace_.add_member(*symbol);
                       },
                       [this](const CompilationUnitSyntax &syntax)
                       {
                           global_namespace_.add_members(syntax.members() |
                                                         std::views::transform([this](const DeclarationSyntax &member)
                                                                               { return bind_declaration(member); }) |
                                                         std::views::join);
                       },
                       [this](const SyntaxNode &syntax)
                       {
                           throw InvalidStateException{
                               std::format("Unexpected syntax node type: {}", get_name(syntax.kind()))};
                       }});

        return assembly_symbol_;
    }

    Optional<const Symbol &> DeclarationBinder::bind_declaration(const DeclarationSyntax &syntax)
    {
        // TODO: Actually bind the members
        return std::nullopt;
    }
} // namespace prism
