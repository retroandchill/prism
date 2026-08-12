/**
 * @file named_type_symbol.cpp
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
module prism.core:symbols.named_type_symbol.impl;

import :symbols.named_type_symbol;

namespace prism
{
    void NamedTypeSymbol::write_display_string(TextWriter &writer) const
    {
        switch (special_type())
        {
            case SpecialType::void_:
                writer.write("void");
                break;
            case SpecialType::bool_:
                writer.write("bool");
                break;
            case SpecialType::i8:
                writer.write("i8");
                break;
            case SpecialType::i16:
                writer.write("i16");
                break;
            case SpecialType::i32:
                writer.write("i32");
                break;
            case SpecialType::i64:
                writer.write("i64");
                break;
            case SpecialType::i128:
                writer.write("i128");
                break;
            case SpecialType::isize:
                writer.write("isize");
                break;
            case SpecialType::u8:
                writer.write("u8");
                break;
            case SpecialType::u16:
                writer.write("u16");
                break;
            case SpecialType::u32:
                writer.write("u32");
                break;
            case SpecialType::u64:
                writer.write("u64");
                break;
            case SpecialType::u128:
                writer.write("u128");
                break;
            case SpecialType::usize:
                writer.write("usize");
                break;
            case SpecialType::f16:
                writer.write("f16");
                break;
            case SpecialType::f32:
                writer.write("f32");
                break;
            case SpecialType::f64:
                writer.write("f64");
                break;
            case SpecialType::char_:
                writer.write("char");
                break;
            case SpecialType::char16:
                writer.write("char16");
                break;
            case SpecialType::rune:
                writer.write("rune");
                break;
            case SpecialType::str:
                writer.write("str");
                break;
            default:
                writer.write(name().as_string_view());
        }
    }
} // namespace prism
