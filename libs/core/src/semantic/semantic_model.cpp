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

namespace prism
{
    Optional<const Symbol &> SemanticModel::get_declared_symbol(const SyntaxNode &node) const
    {
        validate_is_part_of_compilation(node);
        return CompilationInternal::get_semantic_mappings(*compilation_).get_symbol(node);
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

    void SemanticModel::validate_is_part_of_compilation(const SyntaxNode &node) const
    {
        if (&node.tree() != tree_)
            throw std::invalid_argument{"node is not part of this compilation"};
    }
} // namespace prism
