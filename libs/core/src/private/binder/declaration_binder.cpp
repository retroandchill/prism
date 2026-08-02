/**
 * @file declaration_binder.cpp
 * @author Francesco Corso
 * @date 8/1/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:binder.declaration_binder.impl;

import :binder.declaration_binder;
import :symbols.symbol_lifetime;
import :symbols.source;
import :syntax.tree;
import :syntax.visit;
import :util.overload;
import :memory.buffer_pool;

namespace prism
{
    DeclarationBinder::DeclarationBinder(Name assembly_name, const SyntaxTree &tree, SymbolLifetime &lifetime) noexcept
        : tree_{tree}, lifetime_{lifetime}, assembly_symbol_{lifetime.create<SourceAssemblySymbol>(assembly_name)},
          global_namespace_{lifetime.create<SourceNamespaceSymbol>(KnownName::global, &assembly_symbol_)}
    {
    }

    const AssemblySymbol &DeclarationBinder::bind() const
    {
        visit(tree_.root(),
              Overload{[this](const DeclarationSyntax &syntax)
                       {
                           if (const auto symbol = bind_declaration(syntax, global_namespace_); symbol.has_value())
                               global_namespace_.add_member(*symbol);
                       },
                       [this](const CompilationUnitSyntax &syntax)
                       {
                           global_namespace_.add_members(
                               syntax.members() |
                               std::views::transform([this](const DeclarationSyntax &member)
                                                     { return bind_declaration(member, global_namespace_); }) |
                               std::views::join);
                       },
                       [this](const SyntaxNode &syntax)
                       {
                           throw InvalidStateException{
                               std::format("Unexpected syntax node type: {}", get_name(syntax.kind()))};
                       }});

        return assembly_symbol_;
    }

    Optional<const Symbol &> DeclarationBinder::bind_declaration(const DeclarationSyntax &syntax,
                                                                 const Symbol &containing_symbol) const
    {
        return visit<Optional<const Symbol &>>(
            syntax,
            Overload{
                [](const IncompleteDeclarationSyntax &) { return std::nullopt; },
                [&](const NamespaceDeclarationSyntax &ns) -> auto &
                {
                    const NameSyntax *name = &ns.name();
                    PooledVector<const SimpleNameSyntax *> stack;
                    do
                    {
                        visit(*name,
                              Overload{[&](const QualifiedNameSyntax &q) { stack.push_back(&q.right()); },
                                       [&](const SimpleNameSyntax &s)
                                       {
                                           stack.push_back(&s);
                                           name = nullptr;
                                       }});
                    } while (name != nullptr);

                    SourceNamespaceSymbol *symbol = nullptr;

                    ASSUME(!stack.empty());
                    while (!stack.empty())
                    {
                        auto *current = stack.back();
                        ASSUME(current != nullptr);

                        auto name_value = current->identifier().get_value<IdentifierData>().name;
                        symbol =
                            &lifetime_.create<SourceNamespaceSymbol>(name_value,
                                                                     symbol != nullptr ? symbol : &containing_symbol);
                        stack.pop_back();
                    }

                    ASSUME(symbol != nullptr);
                    symbol->add_members(ns.members() |
                                        std::views::transform([&](const DeclarationSyntax &member)
                                                              { return bind_declaration(member, *symbol); }) |
                                        std::views::join);

                    return *symbol;
                },
                [&](const VariableDeclarationSyntax &variable) -> auto &
                {
                    auto name = variable.identifier().get_value<IdentifierData>().name;
                    return lifetime_.create<SourceVariableSymbol>(name, &containing_symbol, variable);
                },
                [&](const FunctionDeclarationSyntax &function) -> auto &
                {
                    auto name = function.identifier().get_value<IdentifierData>().name;
                    auto &symbol = lifetime_.create<SourceFunctionSymbol>(name, &containing_symbol, function);
                    symbol.add_parameters(
                        function.parameters().parameters() |
                        std::views::transform(
                            [&](const ParameterSyntax &parameter) -> auto &
                            {
                                auto param_name = parameter.name().get_value<IdentifierData>().name;
                                return lifetime_.create<SourceParameterSymbol>(param_name, &symbol, parameter);
                            }));
                    return symbol;
                },
            });
    }
} // namespace prism
