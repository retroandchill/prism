// @file BuiltInTypes.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Semantic.Symbols;
using Prism.Core.Strings;
using Prism.Core.Syntax;

namespace Prism.Core.Semantic;

public static partial class BuiltInTypes
{
    extension(BuiltInType type)
    {
        public bool IsSignedInteger =>
            type
                is BuiltInType.I8
                    or BuiltInType.I16
                    or BuiltInType.I32
                    or BuiltInType.I64
                    or BuiltInType.I128
                    or BuiltInType.ISize;

        public bool IsUnsigned =>
            type
                is BuiltInType.U8
                    or BuiltInType.U16
                    or BuiltInType.U32
                    or BuiltInType.U64
                    or BuiltInType.U128
                    or BuiltInType.USize;

        public bool IsInteger => type.IsSignedInteger || type.IsUnsigned;

        public bool IsFloatingPoint =>
            type is BuiltInType.F16 or BuiltInType.F32 or BuiltInType.F64;

        public bool IsSigned => type.IsSignedInteger || type.IsFloatingPoint;

        public bool IsNumeric => type.IsInteger || type.IsFloatingPoint;

        public bool IsCharacter =>
            type is BuiltInType.Char or BuiltInType.Char16 or BuiltInType.Rune;

        public ConversionKind ClassifyConversion(BuiltInType other, TargetPlatform platform)
        {
            if (type == other)
                return ConversionKind.Identity;

            if (type.IsNumeric && other.IsNumeric)
            {
                return type.IsImplicitNumericConversion(other, platform)
                    ? ConversionKind.Implicit
                    : ConversionKind.Explicit;
            }

            if (type.IsCharacter && other.IsCharacter)
            {
                return type.AsUnderlyingType() <= other.AsUnderlyingType()
                    ? ConversionKind.Implicit
                    : ConversionKind.Explicit;
            }

            return ConversionKind.None;
        }

        public int GetByteWidth(TargetPlatform platform)
        {
            return type switch
            {
                BuiltInType.I8 or BuiltInType.U8 => 1,
                BuiltInType.I16 or BuiltInType.U16 or BuiltInType.F16 => 2,
                BuiltInType.I32 or BuiltInType.U32 or BuiltInType.F32 => 4,
                BuiltInType.I64 or BuiltInType.U64 or BuiltInType.F64 => 8,
                BuiltInType.I128 or BuiltInType.U128 => 16,
                BuiltInType.ISize or BuiltInType.USize => platform.PointerSize,
                _ => throw new ArgumentException("Type is not numeric"),
            };
        }

        public int GetIntegerPrecisionBits(TargetPlatform platform)
        {
            if (!type.IsNumeric)
                throw new ArgumentException("Type is not numeric");

            return type switch
            {
                BuiltInType.F16 => 11,
                BuiltInType.F32 => 24,
                BuiltInType.F64 => 53,
                _ => type.GetByteWidth(platform) * 8,
            };
        }

        private bool IsImplicitNumericConversion(BuiltInType other, TargetPlatform platform)
        {
            if (type.IsInteger && other.IsInteger)
            {
                var sourceWidth = type.GetByteWidth(platform);
                var targetWidth = other.GetByteWidth(platform);

                if (type.IsSigned == other.IsSigned)
                {
                    return targetWidth >= sourceWidth;
                }

                if (type.IsUnsigned && other.IsSigned)
                {
                    return targetWidth > sourceWidth;
                }
            }
            else if (type.IsFloatingPoint && other.IsFloatingPoint)
            {
                var sourceWidth = type.GetByteWidth(platform);
                var targetWidth = other.GetByteWidth(platform);

                return targetWidth >= sourceWidth;
            }
            else if (type.IsInteger && other.IsFloatingPoint)
            {
                var sourcePrecision = type.GetIntegerPrecisionBits(platform);
                var targetPrecision = other.GetIntegerPrecisionBits(platform);

                return targetPrecision >= sourcePrecision;
            }

            return false;
        }
    }
}
