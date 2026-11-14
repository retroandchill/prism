// @file NamedTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Cysharp.Text;
using Prism.Semantic.Symbols.Data;

namespace Prism.Semantic.Symbols;

public sealed class NamedTypeSymbol : TypeSymbol, IAttributeHolderSymbolData
{
    internal NamedTypeSymbol(INamedTypeSymbolData data)
        : base(data) { }

    internal new INamedTypeSymbolData Data => Unsafe.As<INamedTypeSymbolData>(base.Data);

    public NamedTypeKind Kind => Data.Kind;

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        switch (SpecialType)
        {
            case SpecialType.Void:
                builder.Append("void");
                break;
            case SpecialType.Bool:
                builder.Append("bool");
                break;
            case SpecialType.I8:
                builder.Append("i8");
                break;
            case SpecialType.I16:
                builder.Append("i16");
                break;
            case SpecialType.I32:
                builder.Append("i32");
                break;
            case SpecialType.I64:
                builder.Append("i64");
                break;
            case SpecialType.I128:
                builder.Append("i128");
                break;
            case SpecialType.ISize:
                builder.Append("isize");
                break;
            case SpecialType.U8:
                builder.Append("u8");
                break;
            case SpecialType.U16:
                builder.Append("u16");
                break;
            case SpecialType.U32:
                builder.Append("u32");
                break;
            case SpecialType.U64:
                builder.Append("u64");
                break;
            case SpecialType.U128:
                builder.Append("u128");
                break;
            case SpecialType.USize:
                builder.Append("usize");
                break;
            case SpecialType.F32:
                builder.Append("f32");
                break;
            case SpecialType.F64:
                builder.Append("f64");
                break;
            case SpecialType.Char:
                builder.Append("char");
                break;
            case SpecialType.Char16:
                builder.Append("char16");
                break;
            case SpecialType.Rune:
                builder.Append("rune");
                break;
            case SpecialType.Str:
                builder.Append("str");
                break;
            case SpecialType.None:
            default:
            {
                if (ContainingNamespace is { IsGlobal: false } container)
                {
                    container.WriteDisplayString(ref builder);
                    builder.Append("::");
                }

                builder.Append(Name);
                break;
            }
        }
    }
}
