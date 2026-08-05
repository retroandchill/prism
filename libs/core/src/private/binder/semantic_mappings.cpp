/**
 * @file semantic_mappings.cpp
 * @author Francesco Corso
 * @date 8/4/2026
 * @brief
 */
module prism.core:binder.semantic_mappings.impl;

import :binder.semantic_mappings;

namespace prism
{
    void SemanticMappings::add_symbol_mapping(const SyntaxNode &node, const Symbol &symbol)
    {
        symbol_mappings_.emplace(&node, &symbol);
    }

    void SemanticMappings::add_scope_mapping(const SyntaxNode &node, const DeclarationScope &scope)
    {
        scope_mappings_.emplace(&node, &scope);
    }

    Optional<const Symbol &> SemanticMappings::get_symbol(const SyntaxNode &node) const
    {
        const auto it = symbol_mappings_.find(&node);
        if (it == symbol_mappings_.end())
            return std::nullopt;

        return it->second;
    }

    Optional<const DeclarationScope &> SemanticMappings::get_scope(const SyntaxNode &node) const
    {
        const auto it = scope_mappings_.find(&node);
        if (it == scope_mappings_.end())
            return std::nullopt;

        return it->second;
    }
} // namespace prism
