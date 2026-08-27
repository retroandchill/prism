/**
 * @file binder_factory.cpp
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:binder.binder_factory.impl;

import :binder.binder_factory;
import :syntax.visit;
import :compilation;
import :binder.block_binder;
import :binder.for_loop_binder;
import :binder.member_container_binder;
import :binder.compilation_unit_binder;
import :symbols.namespace_symbol;
import :binder.function_signature_binder;

namespace prism
{
    BinderFactory::BinderFactory(const Compilation &compilation, const SyntaxTree &syntax_tree)
        : compilation_{compilation}, syntax_tree_{syntax_tree}
    {
        DEBUG_ASSERT(compilation_.contains_syntax_tree(syntax_tree));
    }

    const Binder &BinderFactory::get_binder(const SyntaxNode &node) const
    {
        DEBUG_ASSERT(&syntax_tree_ == &node.tree());
        auto enclosing = find_enclosing_designator(node);
        if (!enclosing.has_value())
            return CompilationInternal::get_root_binder(compilation_);

        return get_slot(*enclosing)
            .get_or_compute([this, enclosing] -> auto & { return create_binder_for_designator(*enclosing); });
    }
    Optional<const SyntaxNode &> BinderFactory::find_enclosing_designator(const SyntaxNode &node)
    {
        for (auto current = Optional<const SyntaxNode &>{node}; current.has_value(); current = current->parent())
        {
            if (introduces_new_scope(*current))
                return current;
        }

        return std::nullopt;
    }

    bool BinderFactory::introduces_new_scope(const SyntaxNode &node)
    {
        return node.is_any_of<FunctionDeclarationSyntax,
                              BlockSyntax,
                              NamespaceDeclarationSyntax,
                              CompilationUnitSyntax,
                              ForStatementSyntax>();
    }

    Lazy<const Binder &> &BinderFactory::get_slot(const SyntaxNode &node) const
    {
        std::scoped_lock lock{mutex_};
        return binder_cache_[&node];
    }

    const Binder &BinderFactory::create_binder_for_designator(const SyntaxNode &node) const
    {
        auto &enclosing = *node.parent()
                               .transform([this](const SyntaxNode &parent) -> auto & { return get_binder(parent); })
                               .or_else([this] -> Optional<const Binder &>
                                        { return CompilationInternal::get_root_binder(compilation_); });
        return visit(
            node,
            Overload{
                [&](const CompilationUnitSyntax &compilation_unit) -> const Binder & {
                    return CompilationInternal::get_lifetime(compilation_)
                        .create<CompilationUnitBinder>(enclosing, compilation_unit);
                },
                [&](const NamespaceDeclarationSyntax &ns) -> const Binder &
                {
                    auto &symbol = compilation_.get_semantic_model(syntax_tree_).get_declared_symbol(ns).value();
                    auto &compilation_symbol = compilation_.get_compilation_namespace(symbol).value();
                    return CompilationInternal::get_lifetime(compilation_)
                        .create<MemberContainerBinder>(enclosing, compilation_symbol, ns);
                },
                [&](const FunctionDeclarationSyntax &function_declaration) -> const Binder &
                {
                    auto &symbol =
                        compilation_.get_semantic_model(syntax_tree_).get_declared_symbol(function_declaration).value();
                    return CompilationInternal::get_lifetime(compilation_)
                        .create<FunctionSignatureBinder>(enclosing, symbol, function_declaration);
                },
                [&](const BlockSyntax &block_syntax) -> const Binder & {
                    return CompilationInternal::get_lifetime(compilation_).create<BlockBinder>(enclosing, block_syntax);
                },
                [&](const ForStatementSyntax &syntax) -> const Binder &
                { return CompilationInternal::get_lifetime(compilation_).create<ForLoopBinder>(enclosing, syntax); },
                [](const SyntaxNode &) -> const Binder &
                {
                    throw std::invalid_argument("Unsupported syntax node type");
                }});
    }
} // namespace prism
