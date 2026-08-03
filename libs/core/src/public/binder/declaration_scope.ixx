/**
 * @file declaration_scope.ixx
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
export module prism.core:binder.declaration_scope;

import :syntax.node;
import :util.ref;
import :symbols.symbol;
import :memory.buffer_pool;

namespace prism
{
    using SymbolList = PooledVector<Ref<const Symbol>>;

    class DeclarationLookupResult final
    {
      public:
        constexpr DeclarationLookupResult() = default;

        constexpr explicit DeclarationLookupResult(SymbolList symbols) : symbols_{std::move(symbols)}
        {
        }

        [[nodiscard]] constexpr bool found() const noexcept
        {
            return !symbols_.empty();
        }

        [[nodiscard]] constexpr bool ambiguous() const noexcept
        {
            return symbols_.size() > 1;
        }

        [[nodiscard]] constexpr SymbolSpan<Symbol> symbols() const noexcept
        {
            return symbols_;
        }

      private:
        SymbolList symbols_;
    };

    class DeclarationScope final
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

        [[nodiscard]] DeclarationLookupResult lookup_nearest(Name name) const;

        [[nodiscard]] DeclarationLookupResult lookup_all_visible(Name name) const;

      private:
        void append_namespace_members(Name name, SymbolList &symbols) const;

        const SyntaxNode &owner_;
        DeclarationScope *parent_ = nullptr;

        std::unordered_map<Name, std::vector<Ref<const Symbol>>> symbols_;
        std::vector<Ref<const NamespaceSymbol>> using_namespaces_;
    };
} // namespace prism
