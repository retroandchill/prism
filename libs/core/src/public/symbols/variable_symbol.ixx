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
        VariableSymbol() = default;
        ~VariableSymbol() = default;

      public:
        virtual const TypeSymbol &type() const noexcept = 0;

        virtual bool is_mutable() const noexcept = 0;
    };
} // namespace prism
