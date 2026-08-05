/**
 * @file function_symbol.ixx
 * @author Francesco Corso
 * @date 7/30/2026
 * @brief
 */
export module prism.core:symbols.function_symbol;

import :symbols.symbol;

namespace prism
{
    class TypeSymbol;
    class ParameterSymbol;

    export class FunctionSymbol : public Symbol
    {
      protected:
        constexpr explicit FunctionSymbol(const Name name, const Symbol *containing)
            : Symbol{SymbolKind::function, name, containing}
        {
        }

        ~FunctionSymbol() = default;

      public:
        [[nodiscard]] virtual const TypeSymbol &return_type() const = 0;

        [[nodiscard]] virtual SymbolSpan<ParameterSymbol> parameters() const noexcept = 0;

        [[nodiscard]] static bool instance_of(const Symbol &symbol) noexcept
        {
            return symbol.kind() == SymbolKind::function;
        }
    };
} // namespace prism
