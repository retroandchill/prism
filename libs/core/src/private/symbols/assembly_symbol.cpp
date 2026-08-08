/**
 * @file assembly_symbol.cpp
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
module prism.core:symbols.assembly_symbol.impl;

import :symbols.assembly_symbol;

namespace prism
{
    void AssemblySymbol::write_display_string(TextWriter &writer) const
    {
        writer.write(name().as_string_view());
    }
} // namespace prism
