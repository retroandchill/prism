/**
 * @file type_syntax.ixx
 * @author Francesco Corso
 * @date 6/24/2026
 * @brief
 */
export module prism.core.ast.type_syntax;

import std;

namespace prism
{
    export enum class BuiltInType
    {
        boolean,
        i8,
        i16,
        i32,
        i64,
        i128,
        u8,
        u16,
        u32,
        u64,
        u128,
        isize,
        usize,
        f16,
        f32,
        f64,
        char8,
        char16,
        rune,
        str,
    };

    export struct BuiltInTypeSyntax
    {
        BuiltInType type{};
    };

    export struct NamedTypeSyntax
    {
        std::string name;
    };

    export using TypeSyntax = std::variant<BuiltInTypeSyntax, NamedTypeSyntax>;
} // namespace prism
