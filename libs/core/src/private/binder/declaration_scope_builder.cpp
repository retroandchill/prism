/**
 * @file declaration_scope_builder.cpp
 * @author Francesco Corso
 * @date 8/4/2026
 * @brief
 */
module prism.core:binder.declaration_scope_builder.impl;

import :binder.declaration_scope_builder;
import :semantic.semantic_lifetime;
import :syntax.tree;
import :syntax.visit;
import :memory.buffer_pool;
import :symbols.namespace_symbol;

namespace prism
{

    void DeclarationScopeBuilder::add(const SyntaxTree &tree)
    {
        const auto root = tree.root().as<CompilationUnitSyntax>();
        if (!root.has_value())
            throw std::invalid_argument{"Syntax tree root must be a compilation unit"};

        bind_compilation_unit(*root);
    }

    DeclarationScope &DeclarationScopeBuilder::create_scope(const SyntaxNode &owner, DeclarationScope *parent)
    {
        auto &scope = lifetime_.create<DeclarationScope>(owner, parent);
        scopes_.emplace(&owner, &scope);
        return scope;
    }

    void DeclarationScopeBuilder::bind_compilation_unit(const CompilationUnitSyntax &syntax)
    {
        auto &scope = create_scope(syntax, nullptr);

        for (auto &directive : syntax.usings())
        {
        }
    }

    void DeclarationScopeBuilder::bind_declaration(const DeclarationSyntax &syntax, DeclarationScope &current)
    {
    }

    void DeclarationScopeBuilder::bind_statement(const StatementSyntax &syntax, DeclarationScope &current)
    {
    }

    Optional<const Symbol &> DeclarationScopeBuilder::resolve_symbol(const NameSyntax &syntax)
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
    }

    Optional<const Symbol &> DeclarationScopeBuilder::resolve_symbol(Name syntax, const Symbol &ns)
    {
    }
} // namespace prism
