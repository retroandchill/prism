/**
 * @file declaration_scope_builder.cpp
 * @author Francesco Corso
 * @date 8/4/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:binder.declaration_scope_builder.impl;

import :binder.declaration_scope_builder;
import :semantic.semantic_lifetime;
import :syntax.tree;
import :syntax.visit;
import :memory.buffer_pool;
import :symbols.namespace_symbol;
import :diagnostics.diagnostic_bag;
import :binder.semantic_mappings;
import :binder.binding_helpers;
import :symbols.source;
import :symbols.function_symbol;

namespace prism
{

    void DeclarationScopeBuilder::add(const SyntaxTree &tree) const
    {
        const auto root = tree.root().as<CompilationUnitSyntax>();
        if (!root.has_value())
            throw std::invalid_argument{"Syntax tree root must be a compilation unit"};

        bind_compilation_unit(*root);
    }

    DeclarationScope &DeclarationScopeBuilder::create_scope(const SyntaxNode &owner, DeclarationScope *parent) const
    {
        auto &scope = lifetime_.create<DeclarationScope>(owner, parent);
        mappings_.add_scope_mapping(owner, scope);
        return scope;
    }

    void DeclarationScopeBuilder::bind_compilation_unit(const CompilationUnitSyntax &syntax) const
    {
        auto &scope = create_scope(syntax, nullptr);

        for (auto &directive : syntax.usings())
        {
            auto namespace_symbol = resolve_namespace(directive.name());
            if (!namespace_symbol.has_value())
                continue;

            scope.add_using_declaration(*namespace_symbol);
        }

        for (auto &declaration : syntax.members())
        {
            bind_declaration(declaration, scope);
        }
    }

    void DeclarationScopeBuilder::bind_declaration(const DeclarationSyntax &syntax, DeclarationScope &current) const
    {
        const auto symbol = mappings_.get_symbol(syntax);
        DEBUG_ASSERT(symbol.has_value(), "No symbol found for declaration");
        current.add_symbol(*symbol);

        visit(syntax,
              Overload{[&](const NamespaceDeclarationSyntax &ns) { bind_namespace_declaration(ns, current); },
                       [&](const FunctionDeclarationSyntax &fn) { bind_function_declaration(fn, current); },
                       [](auto &&) {

                       }});
    }

    void DeclarationScopeBuilder::bind_namespace_declaration(const NamespaceDeclarationSyntax &syntax,
                                                             DeclarationScope &current) const
    {
        auto &scope = create_scope(syntax, &current);

        for (auto &directive : syntax.usings())
        {
            auto namespace_symbol = resolve_namespace(directive.name());
            if (!namespace_symbol.has_value())
                continue;

            scope.add_using_declaration(*namespace_symbol);
        }

        for (auto &declaration : syntax.members())
        {
            bind_declaration(declaration, scope);
        }
    }

    void DeclarationScopeBuilder::bind_function_declaration(const FunctionDeclarationSyntax &syntax,
                                                            DeclarationScope &current) const
    {
        auto &scope = create_scope(syntax, &current);

        for (auto &parameter : syntax.parameters().parameters())
        {
            auto symbol = mappings_.get_symbol(parameter);
            DEBUG_ASSERT(symbol.has_value(), "No symbol found for function parameter");
            scope.add_symbol(*symbol);
        }

        auto body = syntax.body();
        if (body.has_value())
        {
            auto &symbol = mappings_.get_symbol(syntax).value().as_checked<FunctionSymbol>();
            bind_block(*body, scope, symbol);
        }
    }

    void DeclarationScopeBuilder::bind_block(const BlockSyntax &syntax,
                                             DeclarationScope &current,
                                             const FunctionSymbol &function) const
    {
        auto &scope = create_scope(syntax, &current);

        for (auto &statement : syntax.statements())
        {
            visit(statement,
                  Overload{[&](const VariableDeclarationStatementSyntax &s)
                           {
                               auto name = s.declaration().identifier().get_value<IdentifierData>().name;
                               const auto &variable =
                                   lifetime_.create<SourceVariableSymbol>(name, &function, s.declaration());
                               mappings_.add_symbol_mapping(s.declaration(), variable);
                               scope.add_symbol(variable);
                           },
                           [&](const BlockSyntax &block) { bind_block(block, scope, function); },
                           [](const StatementSyntax &) {

                           }});
        }
    }

    Optional<const NamespaceSymbol &> DeclarationScopeBuilder::resolve_namespace(const NameSyntax &syntax) const
    {
        const auto names = collect_names(syntax);

        const auto *current_namespace = &global_namespace_;
        for (auto name : names | std::views::transform([](const SimpleNameSyntax &s)
                                                       { return s.identifier().get_value<IdentifierData>().name; }))
        {
            auto members = current_namespace->members();
            auto it = std::ranges::find_if(members, [&](const Symbol &member) { return member.name() == name; });
            if (it == members.end())
            {
                const auto location = syntax.location();
                auto info = DiagnosticInfo::create<DiagnosticCode::unresolved_symbol>(name);
                diagnostics_.add(Diagnostic{std::move(info), location});
                return std::nullopt;
            }

            auto as_namespace = (*it)->as<NamespaceSymbol>();
            if (!as_namespace.has_value())
            {
                const auto location = syntax.location();
                auto info = DiagnosticInfo::create<DiagnosticCode::invalid_symbol>(name, "namespace");
                diagnostics_.add(Diagnostic{std::move(info), location});
                return std::nullopt;
            }

            current_namespace = as_namespace.value_ptr();
        }

        DEBUG_ASSERT(current_namespace != nullptr);
        return current_namespace;
    }
} // namespace prism
