/**
 * @file function_symbol.cpp
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
module prism.core:symbols.function_symbol.impl;

import :symbols.function_symbol;
import :symbols.type_symbol;
import :symbols.assembly_symbol;
import :symbols.parameter_symbol;

namespace prism
{

    bool FunctionSymbol::returns_void() const
    {
        return return_type().is_void();
    }

    void FunctionSymbol::write_display_string(TextWriter &writer) const
    {
        auto containing = containing_symbol();
        if (containing.has_value() && !containing->is<AssemblySymbol>())
        {
            containing->write_display_string(writer);
            writer.write("::");
        }

        writer.write(name().as_string_view());
        writer.write('(');
        for (auto [i, param] : parameters() | std::views::enumerate)
        {
            if (i > 0)
                writer.write(", ");

            param->write_display_string(writer);
        }
        writer.write(')');

        if (!returns_void())
        {
            writer.write(": ");
            return_type().write_display_string(writer);
        }
    }
} // namespace prism
