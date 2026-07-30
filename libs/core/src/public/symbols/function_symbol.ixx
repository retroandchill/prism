/**
 * @file function_symbol.ixx
 * @author Francesco Corso
 * @date 7/30/2026
 * @brief
 */
export module prism.core:symbols.function_symbol;

import :symbols.symbol;
import :util.ref_span;

namespace prism
{
    class TypeSymbol;
    class ParameterSymbol;

    export class FunctionSymbol : public Symbol
    {
      protected:
        FunctionSymbol() = default;
        ~FunctionSymbol() = default;

        virtual const TypeSymbol &returnType() const noexcept = 0;

        virtual RefSpan<const ParameterSymbol> parameters() const noexcept = 0;
    };
} // namespace prism
