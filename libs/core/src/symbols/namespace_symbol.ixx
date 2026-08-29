/**
 * @file namespace_symbol.ixx
 * @author Francesco Corso
 * @date 7/31/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core:symbols.namespace_symbol;

import :symbols.member_container_symbol;
import :util.ref;
import :type_traits.visitor;
import :binder.lookup_result;

namespace prism
{
    export class AssemblySymbol;

    export enum class NamespaceKind : std::uint8_t
    {
        assembly,
        compilation
    };

    export class PRISM_CORE_API NamespaceSymbol : public MemberContainerSymbol
    {
      protected:
        constexpr NamespaceSymbol(const Name name, const Symbol *containing)
            : MemberContainerSymbol{SymbolKind::namespace_, name, containing}
        {
        }

        ~NamespaceSymbol() = default;

      public:
        [[nodiscard]] constexpr Optional<const TypeSymbol &> containing_type() const noexcept final
        {
            return std::nullopt;
        }

        [[nodiscard]] constexpr bool is_global() const noexcept
        {
            return name() == KnownName::global;
        }

        [[nodiscard]] constexpr bool is_implicitly_declared() const noexcept final
        {
            return is_global();
        }

        [[nodiscard]] virtual NamespaceKind namespace_kind() const noexcept = 0;

        [[nodiscard]] virtual Optional<const Compilation &> containing_compilation() const noexcept = 0;

        [[nodiscard]] Optional<const NamespaceSymbol &> get_nested_namespace(Name name) const;

        [[nodiscard]] static constexpr bool instance_of(const Symbol &symbol) noexcept
        {
            return symbol.kind() == SymbolKind::namespace_;
        }

        void write_display_string(TextWriter &writer) const final;
    };
} // namespace prism
