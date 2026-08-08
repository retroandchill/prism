/**
 * @file declaration_scope.ixx
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core:binder.declaration_scope;

import :syntax.node;
import :util.ref;
import :symbols.symbol;
import :memory.buffer_pool;
import :semantic.lookup_result;

namespace prism
{
    export class PRISM_CORE_API DeclarationScope final
    {
      public:
        constexpr explicit DeclarationScope(const SyntaxNode &owner, DeclarationScope *parent = nullptr)
            : owner_{owner}, parent_{parent}
        {
        }

        [[nodiscard]] constexpr const SyntaxNode &owner() const noexcept
        {
            return owner_;
        }

        [[nodiscard]] constexpr Optional<const DeclarationScope &> parent() const noexcept
        {
            return parent_;
        }

        void add_symbol(const Symbol &symbol);

        void add_using_declaration(const NamespaceSymbol &symbol);

        [[nodiscard]] LookupResult lookup_nearest(Name name) const;

        [[nodiscard]] LookupResult lookup_all_visible(Name name) const;

      private:
        void append_namespace_members(Name name, SymbolList &symbols) const;

        const SyntaxNode &owner_;
        DeclarationScope *parent_ = nullptr;

        std::unordered_map<Name, std::vector<Ref<const Symbol>>> symbols_{};
        std::vector<Ref<const NamespaceSymbol>> using_namespaces_{};
    };

    using DeclarationScopeMap = std::unordered_map<const SyntaxNode *, const DeclarationScope *>;
} // namespace prism
