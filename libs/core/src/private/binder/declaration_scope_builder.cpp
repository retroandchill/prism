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
    }

    Optional<const NamespaceSymbol &> DeclarationScopeBuilder::resolve_namespace(const NameSyntax &syntax) const
    {
        PooledVector<Name> stack;

        auto *current = &syntax;
        while (current != nullptr)
        {
            visit(syntax,
                  Overload{[&](const SimpleNameSyntax &simple)
                           {
                               stack.push_back(simple.identifier().get_value<IdentifierData>().name);
                               current = nullptr;
                           },
                           [&](const QualifiedNameSyntax &qualified)
                           {
                               stack.push_back(qualified.right().identifier().get_value<IdentifierData>().name);
                               current = &qualified.left();
                           }});
        }

        const auto *current_namespace = &global_namespace_;
        for (auto name : stack | std::views::reverse)
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
