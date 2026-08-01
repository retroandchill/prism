/**
 * @file namespace_symbol.cpp
 * @author Francesco Corso
 * @date 8/1/2026
 * @brief
 */
module prism.core:symbols.namespace_symbol.impl;

import :symbols.namespace_symbol;
import :symbols.assembly_symbol;

namespace prism
{
    namespace
    {
        bool is_valid_namespace_member(const Symbol &symbol)
        {
            return !symbol.is<AssemblySymbol>();
        }
    } // namespace

    void NamespaceSymbol::add_member(const Symbol &member)
    {
        if (!is_valid_namespace_member(member))
            throw std::invalid_argument{"Invalid namespace member kind"};

        members_.emplace_back(member);
    }
} // namespace prism
