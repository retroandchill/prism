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
        TypeSymbol() = default;
        ~TypeSymbol() = default;
    };
} // namespace prism
