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
    std::generator<Diagnostic> SemanticModel::get_diagnostics() const
    {
        co_yield std::ranges::elements_of(tree_->get_diagnostics());

        for (auto &diagnostic : compilation_->diagnostics())
        {
            co_yield diagnostic;
        }
    }

    std::generator<Diagnostic> SemanticModel::get_diagnostics(TextSpan span) const
    {
        for (auto diagnostic : get_diagnostics())
        {
            if (auto *source_location = std::get_if<SourceLocation>(&diagnostic.location());
                source_location == nullptr || &source_location->tree() != tree_ ||
                source_location->source_span().overlaps_with(span))
                continue;

            co_yield std::move(diagnostic);
        }
    }

    Optional<const Symbol &> SemanticModel::get_declared_symbol(const SyntaxNode &node) const
    {
        validate_is_part_of_compilation(node);
        return compilation_->semantic_mappings_.get_symbol(node);
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

    void SemanticModel::validate_is_part_of_compilation(const SyntaxNode &node) const
    {
        if (&node.tree() != tree_)
            throw std::invalid_argument{"node is not part of this compilation"};
    }

    SourceVariableSymbol &SemanticModel::get_local_variable(const VariableDeclarationStatementSyntax &syntax) const
    {
        auto &scope = compilation_->get_declaration_scope(syntax);
        const auto name = syntax.declaration().identifier().get_value<IdentifierData>().name;
        return scope.get_local_variable(name);
    }
} // namespace prism
