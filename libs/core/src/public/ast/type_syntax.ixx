/**
 * @file type_syntax.ixx
 * @author Francesco Corso
 * @date 6/24/2026
 * @brief
 */
module;

#include "ast/macros.hpp"

export module prism.core.ast:type_syntax;

import std;
import prism.core.util;
import :syntax_node;
import :identifier_syntax;
import prism.core.source.source_file;

namespace prism
{
    export enum class BuiltInType
    {
        void_type,
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

    export constexpr std::string_view to_string(const BuiltInType type)
    {
        switch (type)
        {
            case BuiltInType::void_type:
                return "void";
            case BuiltInType::boolean:
                return "bool";
            case BuiltInType::i8:
                return "i8";
            case BuiltInType::i16:
                return "i16";
            case BuiltInType::i32:
                return "i32";
            case BuiltInType::i64:
                return "i64";
            case BuiltInType::i128:
                return "i128";
            case BuiltInType::u8:
                return "u8";
            case BuiltInType::u16:
                return "u16";
            case BuiltInType::u32:
                return "u32";
            case BuiltInType::u64:
                return "u64";
            case BuiltInType::u128:
                return "u128";
            case BuiltInType::isize:
                return "isize";
            case BuiltInType::usize:
                return "usize";
            case BuiltInType::f16:
                return "f16";
            case BuiltInType::f32:
                return "f32";
            case BuiltInType::f64:
                return "f64";
            case BuiltInType::char8:
                return "char";
            case BuiltInType::char16:
                return "char16";
            case BuiltInType::rune:
                return "rune";
            case BuiltInType::str:
                return "str";
            default:
                throw std::invalid_argument{"Not a valid built-in type"};
        }
    }

    export class TypeSyntax : public SyntaxNode
    {
      protected:
        constexpr TypeSyntax(SyntaxKind kind, const SourceRange range, const SyntaxFlags flags = SyntaxFlags::none)
            : SyntaxNode{SyntaxCategory::type, kind, range, flags}
        {
        }

        AST_NODE_BOILERPLATE(TypeSyntax)
    };

    export class BuiltInTypeSyntax final : public TypeSyntax
    {
      public:
        constexpr BuiltInTypeSyntax(const BuiltInType type,
                                    const SourceRange range,
                                    const SyntaxFlags flags = SyntaxFlags::none)
            : TypeSyntax{SyntaxKind::built_in_type, range, flags}, type_{type}
        {
        }

        VALUE_PROPERTY(BuiltInType, type)
    };

    export class NamedTypeSyntax final : public TypeSyntax
    {
      public:
        constexpr NamedTypeSyntax(const IdentifierSyntax &name,
                                  const SourceRange range,
                                  const SyntaxFlags flags = SyntaxFlags::none)
            : TypeSyntax{SyntaxKind::named_type, range, flags}, name_{name}
        {
        }

        OWNED_REF_PROPERTY(IdentifierSyntax, name)
    };
} // namespace prism
