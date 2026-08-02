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
    std::vector<DeclarationRecord> DeclarationBinder::bind() const
    {
        std::vector<DeclarationRecord> records;

        visit(tree_.root(),
              Overload{[&](const DeclarationSyntax &syntax)
                       {
                           if (auto record = bind_declaration(syntax); record.has_value())
                               records.push_back(*std::move(record));
                       },
                       [&](const CompilationUnitSyntax &syntax) { records = bind_declarations(syntax.members()); },
                       [](const SyntaxNode &syntax)
                       {
                           throw InvalidStateException{
                               std::format("Unexpected syntax node type: {}", get_name(syntax.kind()))};
                       }});

        return records;
    }

    std::vector<DeclarationRecord> DeclarationBinder::bind_declarations(
        SyntaxList<DeclarationSyntax> declarations) const
    {
        std::vector<DeclarationRecord> records;
        records.reserve(declarations.size());
        records.append_range(declarations |
                             std::views::transform([this](const DeclarationSyntax &declaration)
                                                   { return bind_declaration(declaration); }) |
                             std::views::join);
        return records;
    }

    Optional<DeclarationRecord> DeclarationBinder::bind_declaration(const DeclarationSyntax &syntax) const
    {
        return visit<Optional<DeclarationRecord>>(
            syntax,
            Overload{
                [](const IncompleteDeclarationSyntax &) { return std::nullopt; },
                [&](const NamespaceDeclarationSyntax &ns)
                {
                    const NameSyntax *name = &ns.name();
                    PooledVector<const SimpleNameSyntax *> stack;
                    do
                    {
                        visit(*name,
                              Overload{[&](const QualifiedNameSyntax &q)
                                       {
                                           stack.push_back(&q.right());
                                           name = &q.left();
                                       },
                                       [&](const SimpleNameSyntax &s)
                                       {
                                           stack.push_back(&s);
                                           name = nullptr;
                                       }});
                    } while (name != nullptr);

                    return NamespaceRecord{
                        .names = stack | std::views::reverse |
                                 std::views::transform([](const SimpleNameSyntax *s)
                                                       { return s->identifier().get_value<IdentifierData>().name; }) |
                                 std::ranges::to<std::vector>(),
                        .syntax = &ns,
                        .declarations = bind_declarations(ns.members()),
                    };
                },
                [&](const VariableDeclarationSyntax &variable)
                {
                    return VariableRecord{
                        .name = variable.identifier().get_value<IdentifierData>().name,
                        .syntax = &variable,
                    };
                },
                [&](const FunctionDeclarationSyntax &function)
                {
                    return FunctionRecord{
                        .name = function.identifier().get_value<IdentifierData>().name,
                        .syntax = &function,
                        .parameters = function.parameters().parameters() |
                                      std::views::transform(
                                          [&](const ParameterSyntax &parameter)
                                          {
                                              return ParameterRecord{
                                                  .name = parameter.name().get_value<IdentifierData>().name,
                                                  .syntax = &parameter,
                                              };
                                          }) |
                                      std::ranges::to<std::vector>(),
                    };
                },
            });
    }
} // namespace prism
