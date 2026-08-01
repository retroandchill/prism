/**
 * @file named_type_symbol.ixx
 * @author Francesco Corso
 * @date 7/31/2026
 * @brief
 */
export module prism.core:symbols.named_type_symbol;

import :symbols.type_symbol;

namespace prism
{
    export class NamedTypeSymbol : public TypeSymbol
    {
      protected:
        NamedTypeSymbol(const Name name, const Symbol *containing)
            : TypeSymbol{SymbolKind::named_type, name, containing}
        {
        }

        ~NamedTypeSymbol() = default;

      public:
        static constexpr bool instance_of(const Symbol &symbol) noexcept
        {
            return symbol.kind() == SymbolKind::named_type;
        }
    };
} // namespace prism
