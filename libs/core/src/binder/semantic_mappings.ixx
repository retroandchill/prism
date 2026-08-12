/**
 * @file semantic_mappings.ixx
 * @author Francesco Corso
 * @date 8/4/2026
 * @brief
 */
export module prism.core:binder.semantic_mappings;

import std;
import :util.optional;

namespace prism
{
    class DeclarationScope;
    class Symbol;
    class SyntaxNode;

    class SemanticMappings
    {
      public:
        void add_symbol_mapping(const SyntaxNode &node, const Symbol &symbol);

        void add_scope_mapping(const SyntaxNode &node, const DeclarationScope &scope);

        [[nodiscard]] Optional<const Symbol &> get_symbol(const SyntaxNode &node) const;

        [[nodiscard]] Optional<const DeclarationScope &> get_scope(const SyntaxNode &node) const;

      private:
        std::unordered_map<const SyntaxNode *, const Symbol *> symbol_mappings_;
        std::unordered_map<const SyntaxNode *, const DeclarationScope *> scope_mappings_;
    };
} // namespace prism
