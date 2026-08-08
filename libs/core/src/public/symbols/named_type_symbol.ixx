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
        constexpr NamedTypeSymbol(const Name name,
                                  const Symbol *containing,
                                  const SpecialType special_type = SpecialType::none)
            : TypeSymbol{SymbolKind::named_type, name, containing, special_type}
        {
        }

        ~NamedTypeSymbol() noexcept = default;

      public:
        static constexpr bool instance_of(const Symbol &symbol) noexcept
        {
            return symbol.kind() == SymbolKind::named_type;
        }

        void write_display_string(TextWriter &writer) const final;
    };
} // namespace prism
