/**
 * @file parameter_symbol.ixx
 * @author Francesco Corso
 * @date 7/30/2026
 * @brief
 */
export module prism.core:symbols.parameter_symbol;

import :symbols.symbol;

namespace prism
{
    class TypeSymbol;

    export class ParameterSymbol : public Symbol
    {
      protected:
        ParameterSymbol(const Name name, const Symbol *containing) : Symbol{SymbolKind::parameter, name, containing}
        {
        }

        ~ParameterSymbol() = default;

      public:
        [[nodiscard]] virtual const TypeSymbol &type() const = 0;

        [[nodiscard]] virtual bool is_mutable() const noexcept = 0;

        [[nodiscard]] static bool instance_of(const Symbol &symbol) noexcept
        {
            return symbol.kind() == SymbolKind::parameter;
        }

        void write_display_string(TextWriter &writer) const final;
    };
} // namespace prism
