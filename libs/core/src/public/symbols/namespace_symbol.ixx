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
    export class NamespaceSymbol : public Symbol
    {
      protected:
        constexpr NamespaceSymbol(const Name name, const Symbol *containing)
            : Symbol{SymbolKind::namespace_, name, containing}
        {
        }
        ~NamespaceSymbol() = default;

      public:
        [[nodiscard]] constexpr bool is_global() const noexcept
        {
            return name() == KnownName::global;
        }

        [[nodiscard]] virtual SymbolSpan<Symbol> members() const = 0;

        [[nodiscard]] static constexpr bool instance_of(const Symbol &symbol) noexcept
        {
            return symbol.kind() == SymbolKind::namespace_;
        }
    };
} // namespace prism
