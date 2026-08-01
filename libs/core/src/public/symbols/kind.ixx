/**
 * @file kind.ixx
 * @author Francesco Corso
 * @date 7/31/2026
 * @brief
 */
export module prism.core:symbols.kind;

import std;

namespace prism
{
    export enum class SymbolKind : std::uint16_t
    {
        assembly,
        namespace_,
        variable,
        function,
        parameter,
        named_type,
    };
} // namespace prism
