/**
 * @file member_container_symbol.ixx
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core:symbols.member_container_symbol;

import :symbols.symbol;
import :semantic.lookup_result;

namespace prism
{
    export class PRISM_CORE_API MemberContainerSymbol : public Symbol
    {
      protected:
        constexpr MemberContainerSymbol(const SymbolKind kind, const Name name, const Symbol *containing)
            : Symbol{kind, name, containing}
        {
        }

        ~MemberContainerSymbol() = default;

      public:
        [[nodiscard]] virtual SymbolSpan<Symbol> members() const = 0;

        [[nodiscard]] LookupResult members(Name name) const;

        [[nodiscard]] static constexpr bool instance_of(const Symbol &symbol) noexcept
        {
            return symbol.kind() == SymbolKind::namespace_ || symbol.kind() == SymbolKind::named_type;
        }
    };
} // namespace prism
