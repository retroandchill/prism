/**
 * @file variable_symbol.ixx
 * @author Francesco Corso
 * @date 7/30/2026
 * @brief
 */
export module prism.core:symbols.variable_symbol;

import :symbols.symbol;

namespace prism
{
    class TypeSymbol;

    export class VariableSymbol : public Symbol
    {
      protected:
        VariableSymbol(const Name name, const Symbol *containing) : Symbol{SymbolKind::variable, name, containing}
        {
        }

        ~VariableSymbol() = default;

      public:
        virtual const TypeSymbol &type() const = 0;

        virtual bool is_mutable() const noexcept = 0;

        [[nodiscard]] static bool instance_of(const Symbol &symbol) noexcept
        {
            return symbol.kind() == SymbolKind::variable;
        }
    };
} // namespace prism
