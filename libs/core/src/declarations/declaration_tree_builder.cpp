/**
 * @file declaration_tree_builder.cpp
 * @author Francesco Corso
 * @date 8/16/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:declarations.declaration_tree_builder.impl;

import :declarations.declaration_tree_builder;
import :syntax.visit;
import :syntax.tree;
import :syntax.green.visit;
import :declarations.single_root_namespace_declaration;
import :diagnostics.diagnostic_bag;
import :memory.buffer_pool;
import :binder.binding_helpers;

namespace prism
{

    DeclarationTreeBuilder::DeclarationTreeBuilder(const SyntaxTree &syntax_tree) : syntax_tree_{syntax_tree}
    {
    }

    RefCountPtr<const SingleRootNamespaceDeclaration> DeclarationTreeBuilder::for_tree(const SyntaxTree &syntax_tree)
    {
        DeclarationTreeBuilder builder{std::move(syntax_tree)};
        return builder.visit_tree();
    }

    RefCountPtr<const SingleRootNamespaceDeclaration> DeclarationTreeBuilder::visit_tree()
    {
        return visit(syntax_tree_.root(),
                     Overload{[this](const CompilationUnitSyntax &syntax) { return visit_compilation_unit(syntax); },
                              [](const SyntaxNode &syntax) -> RefCountPtr<const SingleRootNamespaceDeclaration>
                              {
                                  throw InvalidStateException{
                                      std::format("Unexpected syntax node type: {}", get_name(syntax.kind()))};
                              }});
    }

    ImmutableArray<RefCountPtr<const SingleDeclaration>> DeclarationTreeBuilder::visit_namespace_children(
        const SyntaxNode &node,
        SyntaxList<DeclarationSyntax> members)
    {
        DEBUG_ASSERT(node.is<NamespaceDeclarationSyntax>() || node.is<CompilationUnitSyntax>());

        if (members.empty())
            return {};

        PooledVector<RefCountPtr<const SingleDeclaration>> builder;
        for (auto &member : members)
        {
            auto namespace_or_type =
                visit(member,
                      Overload{[this](const NamespaceDeclarationSyntax &syntax)
                               { return visit_namespace_declaration(syntax); },
                               [](const DeclarationSyntax &) -> RefCountPtr<const SingleDeclaration>
                               {
                                   return nullptr;
                               }});
            if (namespace_or_type != nullptr)
                builder.push_back(std::move(namespace_or_type));
        }

        return ImmutableArray{std::from_range, builder | std::views::as_rvalue};
    }

    RefCountPtr<const SingleRootNamespaceDeclaration> DeclarationTreeBuilder::visit_compilation_unit(
        const CompilationUnitSyntax &compilation_unit)
    {
        auto children = visit_namespace_children(compilation_unit, compilation_unit.members());
        return create_single_root_namespace_declaration(compilation_unit, std::move(children));
    }

    RefCountPtr<const SingleDeclaration> DeclarationTreeBuilder::visit_namespace_declaration(
        const NamespaceDeclarationSyntax &node)
    {
        auto children = visit_namespace_children(node, node.members());

        auto has_usings = !node.usings().empty();
        auto *name = &node.name();
        const SyntaxNode *current_node = &node;
        auto member_names = get_non_type_member_names(node.members());
        while (name->is<QualifiedNameSyntax>())
        {
            auto qualified = static_cast<const QualifiedNameSyntax *>(name);
            auto right_name = get_unqualified_name(qualified->right());
            auto ns = make_ref_counted<const SingleNamespaceDeclaration>(
                right_name,
                has_usings,
                SyntaxReference{*current_node},
                SourceLocation{qualified->right()},
                std::move(children),
                std::exchange(member_names, ImmutableHashSet<Name>{}),
                ImmutableArray<Diagnostic>{});

            children = ImmutableArray<RefCountPtr<const SingleDeclaration>>{std::move(ns)};
            current_node = &qualified->left();
            has_usings = false;
        }

        DiagnosticBag diagnostics{};

        if (node.is<FileScopedNamespaceDeclarationSyntax>())
        {
            if (is_instance<FileScopedNamespaceDeclarationSyntax>(node.parent()))
            {
                diagnostics.add(Diagnostic{DiagnosticInfo::create<DiagnosticCode::multiple_file_scoped_namespaces>(),
                                           node.name().location()});
            }
            else if (is_instance<NamespaceDeclarationSyntax>(node.parent()))
            {
                diagnostics.add(Diagnostic{DiagnosticInfo::create<DiagnosticCode::file_scoped_and_normal_namespace>(),
                                           node.name().location()});
            }
            else
            {
                DEBUG_ASSERT(is_instance<CompilationUnitSyntax>(node.parent()));
                auto &compilation_unit = static_cast<const CompilationUnitSyntax &>(*node.parent());
                if (&node != &compilation_unit.members()[0])
                {
                    diagnostics.add(Diagnostic{
                        DiagnosticInfo::create<DiagnosticCode::file_scoped_namespace_not_before_all_members>(),
                        node.name().location()});
                }
            }
        }
        else
        {
            DEBUG_ASSERT(node.is<BlockNamespaceDeclarationSyntax>());

            if (is_instance<FileScopedNamespaceDeclarationSyntax>(node.parent()))
            {
                diagnostics.add(Diagnostic{DiagnosticInfo::create<DiagnosticCode::file_scoped_and_normal_namespace>(),
                                           node.name().location()});
            }
        }

        if (!node.modifiers().empty())
        {
            diagnostics.add(Diagnostic{DiagnosticInfo::create<DiagnosticCode::bad_modifiers_on_namespace>(),
                                       node.modifiers()[0].location()});
        }

        DEBUG_ASSERT(name->is<SimpleNameSyntax>());
        auto unqualified_name = get_unqualified_name(static_cast<const SimpleNameSyntax &>(*name));
        return make_ref_counted<SingleNamespaceDeclaration>(unqualified_name,
                                                            has_usings,
                                                            SyntaxReference{*current_node},
                                                            SourceLocation{*name},
                                                            std::move(children),
                                                            std::move(member_names),
                                                            std::move(diagnostics).move_to<ImmutableArray>());
    }

    RefCountPtr<const SingleRootNamespaceDeclaration> DeclarationTreeBuilder::create_single_root_namespace_declaration(
        const CompilationUnitSyntax &compilation_unit,
        ImmutableArray<RefCountPtr<const SingleDeclaration>> children)
    {
        bool has_usings = !compilation_unit.usings().empty();

        return make_ref_counted<const SingleRootNamespaceDeclaration>(
            has_usings,
            SyntaxReference{compilation_unit},
            std::move(children),
            get_non_type_member_names(compilation_unit.members()),
            ImmutableArray<Diagnostic>{});
    }

    ImmutableHashSet<Name> DeclarationTreeBuilder::get_non_type_member_names(SyntaxList<DeclarationSyntax> members)
    {
        std::unordered_set<Name> member_names;
        for (auto &member : members)
        {
            visit(member,
                  Overload{[&](const VariableDeclarationSyntax &variable)
                           { member_names.insert(get_identifier_name(variable.identifier())); },
                           [&](const FunctionDeclarationSyntax &function)
                           { member_names.insert(get_identifier_name(function.identifier())); },
                           [&](const DeclarationSyntax &)
                           {
                               // Other declarations aren't part of this
                           }});
        }

        return ImmutableHashSet{std::from_range, member_names};
    }
} // namespace prism
