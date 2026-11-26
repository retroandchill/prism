using NetEscapades.EnumGenerators;

namespace Prism;

[EnumExtensions(ExtensionClassName = "SpecialTypes")]
public enum SpecialType : byte
{
    None,
    Void,
    Bool,
    I8,
    I16,
    I32,
    I64,
    I128,
    ISize,
    U8,
    U16,
    U32,
    U64,
    U128,
    USize,
    F32,
    F64,
    Char,
    Char16,
    Rune,
    Str,
    Ordering,
}

public static class SpecialTypeExtensions
{
    extension(SpecialType type)
    {
        public bool IsInteger =>
            type
                is SpecialType.I8
                    or SpecialType.I16
                    or SpecialType.I32
                    or SpecialType.I64
                    or SpecialType.I128
                    or SpecialType.ISize
                    or SpecialType.U8
                    or SpecialType.U16
                    or SpecialType.U32
                    or SpecialType.U64
                    or SpecialType.U128
                    or SpecialType.USize;

        public bool IsSignedInteger =>
            type
                is SpecialType.I8
                    or SpecialType.I16
                    or SpecialType.I32
                    or SpecialType.I64
                    or SpecialType.I128
                    or SpecialType.ISize;

        public bool IsUnsignedInteger =>
            type
                is SpecialType.U8
                    or SpecialType.U16
                    or SpecialType.U32
                    or SpecialType.U64
                    or SpecialType.U128
                    or SpecialType.USize;

        public bool IsFloatingPoint => type is SpecialType.F32 or SpecialType.F64;

        public bool IsNumeric => type.IsInteger || type.IsFloatingPoint;

        public bool IsCharacter =>
            type is SpecialType.Char or SpecialType.Char16 or SpecialType.Rune;

        public string MetadataName
        {
            get
            {
                return type switch
                {
                    SpecialType.None => throw new InvalidOperationException("Not a special type"),
                    SpecialType.Void => $"{CommonNames.Std}::{CommonNames.Void}",
                    SpecialType.Bool => $"{CommonNames.Std}::{CommonNames.Bool}",
                    SpecialType.I8 => $"{CommonNames.Std}::{CommonNames.Int8}",
                    SpecialType.I16 => $"{CommonNames.Std}::{CommonNames.Int16}",
                    SpecialType.I32 => $"{CommonNames.Std}::{CommonNames.Int32}",
                    SpecialType.I64 => $"{CommonNames.Std}::{CommonNames.Int64}",
                    SpecialType.I128 => $"{CommonNames.Std}::{CommonNames.Int128}",
                    SpecialType.ISize => $"{CommonNames.Std}::{CommonNames.ISize}",
                    SpecialType.U8 => $"{CommonNames.Std}::{CommonNames.UInt8}",
                    SpecialType.U16 => $"{CommonNames.Std}::{CommonNames.UInt16}",
                    SpecialType.U32 => $"{CommonNames.Std}::{CommonNames.UInt32}",
                    SpecialType.U64 => $"{CommonNames.Std}::{CommonNames.UInt64}",
                    SpecialType.U128 => $"{CommonNames.Std}::{CommonNames.UInt128}",
                    SpecialType.USize => $"{CommonNames.Std}::{CommonNames.USize}",
                    SpecialType.F32 => $"{CommonNames.Std}::{CommonNames.Float32}",
                    SpecialType.F64 => $"{CommonNames.Std}::{CommonNames.Float64}",
                    SpecialType.Char => $"{CommonNames.Std}::{CommonNames.Char}",
                    SpecialType.Char16 => $"{CommonNames.Std}::{CommonNames.Char16}",
                    SpecialType.Rune => $"{CommonNames.Std}::{CommonNames.Rune}",
                    SpecialType.Str => $"{CommonNames.Std}::{CommonNames.Str}",
                    SpecialType.Ordering => $"{CommonNames.Std}::{CommonNames.Ordering}",
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
                };
            }
        }
    }
}
