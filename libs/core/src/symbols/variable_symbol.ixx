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
    export class TypeSymbol;

    export class VariableSymbol : public Symbol
    {
      protected:
        VariableSymbol(const Name name, const Symbol *containing) : Symbol{SymbolKind::variable, name, containing}
        {
        }

        ~VariableSymbol() = default;

      public:
        /**
         * @brief Get the type of the variable.
         * @return The type of the variable
         */
        [[nodiscard]] virtual const TypeSymbol &type() const = 0;

        /**
         * @brief Check if the variable is mutable.
         * @return True if the variable is mutable, false otherwise
         */
        [[nodiscard]] virtual bool is_mutable() const noexcept = 0;

        [[nodiscard]] virtual bool has_initializer() const noexcept = 0;

        /**
         * @brief Check if the target symbol is a variable.
         * @param symbol The target symbol
         * @return If the target symbol is a variable
         */
        [[nodiscard]] static bool instance_of(const Symbol &symbol) noexcept
        {
            return symbol.kind() == SymbolKind::variable;
        }

        void write_display_string(TextWriter &writer) const final;
    };
} // namespace prism
