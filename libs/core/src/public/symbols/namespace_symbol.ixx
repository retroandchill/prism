/**
 * @file namespace_symbol.ixx
 * @author Francesco Corso
 * @date 7/31/2026
 * @brief
 */
export module prism.core:symbols.namespace_symbol;

import :symbols.symbol;
import :util.ref;

namespace prism
{
    export class NamespaceSymbol final : public Symbol
    {
      public:
        constexpr NamespaceSymbol(const Name name, const Symbol *containing)
            : Symbol{SymbolKind::namespace_, name, containing}
        {
        }

        [[nodiscard]] constexpr bool is_global() const noexcept
        {
            return name() == KnownName::global;
        }

        [[nodiscard]] constexpr SymbolSpan<Symbol> members() const noexcept
        {
            return members_;
        }

        [[nodiscard]] static constexpr bool instance_of(const Symbol &symbol) noexcept
        {
            return symbol.kind() == SymbolKind::namespace_;
        }

      private:
        void add_member(const Symbol &member);

        friend class DeclarationBinder;
        friend class SymbolMerger;

        std::vector<Ref<const Symbol>> members_;
    };
} // namespace prism
