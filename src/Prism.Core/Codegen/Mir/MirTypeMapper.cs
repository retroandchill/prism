// @file MirTypeMapper.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Compiling;
using Prism.Core.Configuration;
using Prism.Core.Mir;
using Prism.Core.Symbols;

namespace Prism.Core.Codegen.Mir;

internal sealed class MirTypeMapper(Compilation compilation)
{
    public MirType Map(TypeSymbol type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type switch
        {
            ReferenceTypeSymbol reference => MapReferenceType(reference),
            ArrayTypeSymbol { ElementType: var elementType, Size: { } size } => new MirArrayType(
                Map(elementType),
                size
            ),
            ArrayTypeSymbol { ElementType: var elementType, Size: null } => new MirSliceType(
                Map(elementType),
                false
            ),
            _ => MapSpecialType(type),
        };
    }

    private MirType MapReferenceType(ReferenceTypeSymbol type)
    {
        if (type.ReferencedType.IsDynamicallySized)
        {
            return new MirSliceType(GetElementTypeForUnsized(type.ReferencedType), type.IsMutable);
        }

        var referencedType = Map(type.ReferencedType);
        return new MirPointerType(referencedType, type.IsMutable);
    }

    private MirType MapSpecialType(TypeSymbol type)
    {
        return type.SpecialType switch
        {
            SpecialType.Void => MirVoidType.Instance,
            SpecialType.Bool => MirBoolType.Instance,

            SpecialType.I8 => new MirIntType(8, true),
            SpecialType.I16 => new MirIntType(16, true),
            SpecialType.I32 => new MirIntType(32, true),
            SpecialType.I64 => new MirIntType(64, true),
            SpecialType.I128 => new MirIntType(128, true),

            SpecialType.U8 => new MirIntType(8, false),
            SpecialType.U16 => new MirIntType(16, false),
            SpecialType.U32 => new MirIntType(32, false),
            SpecialType.U64 => new MirIntType(64, false),
            SpecialType.U128 => new MirIntType(128, false),

            SpecialType.ISize => new MirIntType(compilation.Settings.PointerWidth.BitWidth, true),
            SpecialType.USize => new MirIntType(compilation.Settings.PointerWidth.BitWidth, false),

            SpecialType.F32 => new MirFloatType(32),
            SpecialType.F64 => new MirFloatType(64),

            SpecialType.Char => new MirIntType(8, false),
            SpecialType.Char16 => new MirIntType(16, false),
            SpecialType.Rune => new MirIntType(32, false),

            _ => throw new NotSupportedException($"Cannot map semantic type '{type}' to MIR."),
        };
    }

    private MirType GetElementTypeForUnsized(TypeSymbol type)
    {
        return type switch
        {
            ArrayTypeSymbol { ElementType: var elementType } => Map(elementType),
            { SpecialType: SpecialType.Str } => Map(compilation.GetSpecialType(SpecialType.Char)),
            _ => Map(type),
        };
    }
}
