/**
 * @file assembly_symbol.ixx
 * @author Francesco Corso
 * @date 7/31/2026
 * @brief
 */
export module prism.core:symbols.assembly_symbol;

import :symbols.symbol;

namespace prism
{
    export class AssemblySymbol : public Symbol
    {
      protected:
        constexpr explicit AssemblySymbol(const Name name) : Symbol{SymbolKind::assembly, name}
        {
        }
        ~AssemblySymbol() = default;

      public:
        [[nodiscard]] static constexpr bool instance_of(const Symbol &symbol) noexcept
        {
            return symbol.kind() == SymbolKind::assembly;
        }
    };
} // namespace prism
