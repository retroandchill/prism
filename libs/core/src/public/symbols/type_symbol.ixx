/**
 * @file type_symbol.ixx
 * @author Francesco Corso
 * @date 7/30/2026
 * @brief
 */
export module prism.core:symbols.type_symbol;

import :symbols.symbol;

namespace prism
{

    export class TypeSymbol : public Symbol
    {
      protected:
        TypeSymbol(const SymbolKind kind, const Name name, const Symbol *containing) : Symbol{kind, name, containing}
        {
        }

        ~TypeSymbol() = default;

      public:
        static constexpr bool instance_of(const Symbol &symbol) noexcept
        {
            return symbol.kind() == SymbolKind::named_type;
        }
    };
} // namespace prism
