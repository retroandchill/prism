/**
 * @file binder_helpers.cpp
 * @author Francesco Corso
 * @date 8/6/2026
 * @brief
 */
module prism.core:binder.binding_helpers.impl;

import :binder.binding_helpers;
import :syntax.visit;
import :semantic.compilation;
import :symbols.named_type_symbol;

namespace prism
{
    namespace
    {
        SpecialType from_token(const SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind::void_keyword:
                    return SpecialType::void_;
                case SyntaxKind::bool_keyword:
                    return SpecialType::bool_;
                case SyntaxKind::i8_keyword:
                    return SpecialType::i8;
                case SyntaxKind::i16_keyword:
                    return SpecialType::i16;
                case SyntaxKind::i32_keyword:
                    return SpecialType::i32;
                case SyntaxKind::i64_keyword:
                    return SpecialType::i64;
                case SyntaxKind::i128_keyword:
                    return SpecialType::i128;
                case SyntaxKind::u8_keyword:
                    return SpecialType::u8;
                case SyntaxKind::u16_keyword:
                    return SpecialType::u16;
                case SyntaxKind::u32_keyword:
                    return SpecialType::u32;
                case SyntaxKind::u64_keyword:
                    return SpecialType::u64;
                case SyntaxKind::u128_keyword:
                    return SpecialType::u128;
                case SyntaxKind::isize_keyword:
                    return SpecialType::isize;
                case SyntaxKind::usize_keyword:
                    return SpecialType::usize;
                case SyntaxKind::f16_keyword:
                    return SpecialType::f16;
                case SyntaxKind::f32_keyword:
                    return SpecialType::f32;
                case SyntaxKind::f64_keyword:
                    return SpecialType::f64;
                case SyntaxKind::char_keyword:
                    return SpecialType::char_;
                case SyntaxKind::char16_keyword:
                    return SpecialType::char16;
                case SyntaxKind::rune_keyword:
                    return SpecialType::rune;
                case SyntaxKind::str_keyword:
                    return SpecialType::str;
                default:
                    [[unlikely]] throw std::invalid_argument{"unknown special type"};
            }
        }
    } // namespace

    const TypeSymbol &resolve_type(const TypeSyntax &syntax, const Compilation &compilation)
    {
        return visit(syntax,
                     Overload{[&](const PredefinedTypeSyntax &predefined) -> const TypeSymbol &
                              { return compilation.get_special_type(from_token(predefined.keyword().kind())); },
                              [](const NamedTypeSyntax &) -> const TypeSymbol &
                              {
                                  throw NotImplementedException{};
                              }});
    }
} // namespace prism
