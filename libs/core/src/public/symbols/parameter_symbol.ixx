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
        ParameterSymbol() = default;
        ~ParameterSymbol() = default;

      public:
        virtual const TypeSymbol &type() const noexcept = 0;

        virtual bool is_mutable() const noexcept = 0;
    };
} // namespace prism
