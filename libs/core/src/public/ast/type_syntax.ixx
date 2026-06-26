/**
 * @file type_syntax.ixx
 * @author Francesco Corso
 * @date 6/24/2026
 * @brief
 */
export module prism.core.ast.type_syntax;

import std;
import prism.core.util;
import prism.core.ast.common_syntax;

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

    export struct NamedTypeSyntax
    {
        SharedString name;
    };

    export using TypeNameSyntax = std::variant<BuiltInType, NamedTypeSyntax>;

    export struct ValidTypeSyntax
    {
        TypeNameSyntax name;
    };

    export using TypeSyntax = std::variant<ValidTypeSyntax, ErrorSyntax>;
} // namespace prism
