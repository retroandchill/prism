/**
 * @file variable_symbol.cpp
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
module prism.core:symbols.variable_symbol.impl;

import :symbols.variable_symbol;
import :symbols.type_symbol;

namespace prism
{
    void VariableSymbol::write_display_string(TextWriter &writer) const
    {
        writer.write(name().as_string_view());
        writer.write(": ");
        type().write_display_string(writer);
    }
} // namespace prism
