/**
 * @file semantic_model.cpp
 * @author Francesco Corso
 * @date 8/4/2026
 * @brief
 */
module prism.core:semantic.semantic_model.impl;

import :semantic.semantic_model;
import :syntax.tree;
import :semantic.compilation;
import :diagnostics.diagnostic;
import :syntax.declarations;
import :symbols.source;
import :syntax.statements;
import :semantic.semantic_model_state;

namespace prism
{
    Optional<const Symbol &> SemanticModel::get_declared_symbol(const SyntaxNode &node) const
    {
        validate_is_part_of_compilation(node);
        return state().get_declared_symbol(node);
    }

    Optional<const VariableSymbol &> SemanticModel::get_declared_symbol(const VariableDeclarationSyntax &node) const
    {
        return get_declared_symbol(static_cast<const SyntaxNode &>(node))
            .transform([](const Symbol &symbol) -> auto & { return symbol.as_checked<VariableSymbol>(); });
    }

    Optional<const FunctionSymbol &> SemanticModel::get_declared_symbol(const FunctionDeclarationSyntax &node) const
    {
        return get_declared_symbol(static_cast<const SyntaxNode &>(node))
            .transform([](const Symbol &symbol) -> auto & { return symbol.as_checked<FunctionSymbol>(); });
    }

    Optional<const NamespaceSymbol &> SemanticModel::get_declared_symbol(const NamespaceDeclarationSyntax &node) const
    {
        return get_declared_symbol(static_cast<const SyntaxNode &>(node))
            .transform([](const Symbol &symbol) -> auto & { return symbol.as_checked<NamespaceSymbol>(); });
    }

    std::shared_ptr<SemanticModel> SemanticModel::shared_from_this() noexcept
    {
        return {compilation_->shared_from_this(), this};
    }

    std::shared_ptr<const SemanticModel> SemanticModel::shared_from_this() const noexcept
    {
        return {compilation_->shared_from_this(), this};
    }

    SemanticModelState &SemanticModel::state() const
    {
        return state_.get_or_compute(
            [this] -> auto & {
                return CompilationInternal::get_lifetime(*compilation_)
                    .create<SemanticModelState>(*compilation_, *tree_);
            });
    }

    const Binder &SemanticModel::get_binder(const SyntaxNode &node) const
    {
        return state().get_binder(node);
    }

    const BoundExpression &SemanticModel::get_bound_initializer(const VariableDeclarationSyntax &declaration,
                                                                const LookupContext &context) const
    {
        return state().get_bound_variable_initializer(declaration, context);
    }

    const BoundExpression &SemanticModel::get_bound_initializer(const VariableDeclarationSyntax &declaration,
                                                                const Binder &binder,
                                                                const LookupContext &context) const
    {
        return state().get_bound_variable_initializer(declaration, binder, context);
    }

    const BoundStatement &SemanticModel::get_bound_body(const FunctionDeclarationSyntax &declaration,
                                                        const LookupContext &context) const
    {
        return state().get_bound_body(declaration, context);
    }

    void SemanticModel::validate_is_part_of_compilation(const SyntaxNode &node) const
    {
        if (&node.tree() != tree_)
            throw std::invalid_argument{"node is not part of this compilation"};
    }

    SemanticModel &SemanticModelInternal::create(const Compilation &compilation, const SyntaxTree &tree)
    {
        return CompilationInternal::get_lifetime(compilation)
            .create<SemanticModel>(SemanticModel::create_tag, compilation, tree);
    }
} // namespace prism
