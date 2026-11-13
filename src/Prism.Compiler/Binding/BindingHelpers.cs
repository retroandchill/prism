using System.Diagnostics;
using System.Numerics;
using Prism.Compiler.Configuration;
using Prism.Compiler.Semantic;
using Prism.Compiler.Symbols;
using Prism.Constants;
using Prism.Diagnostics;
using Prism.Metadata;
using Prism.Semantic;
using Prism.Syntax;
using Prism.Utils;
using ConstantValue = Prism.Constants.ConstantValue;

namespace Prism.Compiler.Binding;

internal enum IntegerTargetKind
{
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
    BestFit,
}

internal static class BindingHelpers
{
    public static void DiagnoseLookupFailure(
        LookupResult result,
        NameSyntax syntax,
        LookupOptions expected,
        BindingContext context
    )
    {
        switch (result.Kind)
        {
            case LookupResultKind.Viable or LookupResultKind.Error:
                break;
            case LookupResultKind.NotFound:
                context.ReportDiagnostic(
                    Diagnostic.UnresolvedSymbol(syntax.Location, syntax.UnqualifiedName)
                );
                break;
            case LookupResultKind.Ambiguous:
            {
                var name = !result.Symbols.IsEmpty
                    ? result.Symbols[0].Name
                    : syntax.UnqualifiedName;
                context.ReportDiagnostic(Diagnostic.AmbiguousSymbol(syntax.Location, name));
                break;
            }
            case LookupResultKind.Inaccessible:
            {
                var name = !result.Symbols.IsEmpty
                    ? result.Symbols[0].Name
                    : CollectNames(syntax)[^1].UnqualifiedName;
                context.ReportDiagnostic(Diagnostic.SymbolInaccessible(syntax.Location, name));
                break;
            }
            case LookupResultKind.WrongKind:
                context.ReportDiagnostic(
                    Diagnostic.InvalidSymbol(
                        syntax.Location,
                        syntax.UnqualifiedName,
                        expected.ToDisplayString()
                    )
                );
                break;
        }
    }

    public static NamedTypeSymbol CreateErrorTypeSymbol(
        Symbol? owningSymbol,
        Compilation compilation,
        ReadOnlySpan<SimpleNameSyntax> names
    )
    {
        Debug.Assert(!names.IsEmpty);
        foreach (var syntax in names[..^1])
        {
            var name = syntax.Identifier.IdentifierName;
            owningSymbol ??= compilation.CreateErrorNamespaceSymbol(null, name);

            owningSymbol = owningSymbol switch
            {
                NamespaceSymbol symbol => compilation.CreateErrorNamespaceSymbol(symbol, name),
                TypeSymbol symbol => compilation.CreateErrorTypeSymbol(symbol, name),
                _ => owningSymbol,
            };
        }

        var last = names[^1].Identifier.IdentifierName;
        return compilation.CreateErrorTypeSymbol(owningSymbol, last);
    }

    public static List<SimpleNameSyntax> CollectNames(NameSyntax syntax)
    {
        var stack = new List<SimpleNameSyntax>();

        var current = syntax;
        while (current is not null)
        {
            switch (current)
            {
                case SimpleNameSyntax simple:
                    stack.Add(simple);
                    current = null;
                    break;
                case QualifiedNameSyntax qualified:
                    stack.Add(qualified.Right);
                    current = qualified.Left;
                    break;
                default:
                    throw new InvalidOperationException("Unexpected name syntax");
            }
        }

        stack.Reverse();
        return stack;
    }

    extension(SyntaxToken token)
    {
        public string IdentifierName
        {
            get
            {
                Debug.Assert(token.Kind == SyntaxKind.IdentifierToken);
                return token.GetValue<IdentifierData>().Value;
            }
        }
    }

    extension(NameSyntax syntax)
    {
        public string UnqualifiedName
        {
            get
            {
                return syntax switch
                {
                    QualifiedNameSyntax qualifiedNameSyntax => qualifiedNameSyntax
                        .Right
                        .UnqualifiedName,
                    SimpleNameSyntax simpleNameSyntax => simpleNameSyntax.UnqualifiedName,
                    _ => throw new ArgumentOutOfRangeException(nameof(syntax)),
                };
            }
        }
    }

    extension(SimpleNameSyntax syntax)
    {
        public string UnqualifiedName => syntax.Identifier.IdentifierName;
    }

    extension(IntegerSuffix suffix)
    {
        public bool CanBeSigned =>
            suffix
                is not IntegerSuffix.U8
                    and not IntegerSuffix.U16
                    and not IntegerSuffix.U32
                    and not IntegerSuffix.U64
                    and not IntegerSuffix.U128
                    and not IntegerSuffix.USize;
    }

    public static IntegerTargetKind GetIntegerTargetKind(TypeSymbol? targetType)
    {
        if (targetType is null)
            return IntegerTargetKind.BestFit;

        return targetType.SpecialType switch
        {
            SpecialType.I8 => IntegerTargetKind.I8,
            SpecialType.I16 => IntegerTargetKind.I16,
            SpecialType.I32 => IntegerTargetKind.I32,
            SpecialType.I64 => IntegerTargetKind.I64,
            SpecialType.I128 => IntegerTargetKind.I128,
            SpecialType.ISize => IntegerTargetKind.ISize,
            SpecialType.U8 => IntegerTargetKind.U8,
            SpecialType.U16 => IntegerTargetKind.U16,
            SpecialType.U32 => IntegerTargetKind.U32,
            SpecialType.U64 => IntegerTargetKind.U64,
            SpecialType.U128 => IntegerTargetKind.U128,
            SpecialType.USize => IntegerTargetKind.USize,
            SpecialType.F32 => IntegerTargetKind.F32,
            SpecialType.F64 => IntegerTargetKind.F64,
            _ => IntegerTargetKind.BestFit,
        };
    }

    public static IntegerTargetKind GetIntegerTargetKind(
        in IntegerLiteralData literal,
        TypeSymbol? targetType
    )
    {
        return literal.Suffix switch
        {
            IntegerSuffix.None => GetIntegerTargetKind(targetType),
            IntegerSuffix.I8 => IntegerTargetKind.I8,
            IntegerSuffix.I16 => IntegerTargetKind.I16,
            IntegerSuffix.I32 => IntegerTargetKind.I32,
            IntegerSuffix.I64 => IntegerTargetKind.I64,
            IntegerSuffix.I128 => IntegerTargetKind.I128,
            IntegerSuffix.ISize => IntegerTargetKind.ISize,
            IntegerSuffix.U8 => IntegerTargetKind.U8,
            IntegerSuffix.U16 => IntegerTargetKind.U16,
            IntegerSuffix.U32 => IntegerTargetKind.U32,
            IntegerSuffix.U64 => IntegerTargetKind.U64,
            IntegerSuffix.U128 => IntegerTargetKind.U128,
            IntegerSuffix.USize => IntegerTargetKind.USize,
            _ => throw new ArgumentException("Invalid integer suffix", nameof(literal)),
        };
    }

    public static bool FitsIn(
        this BigInteger value,
        IntegerTargetKind kind,
        CompilationSettings settings
    )
    {
        return kind switch
        {
            IntegerTargetKind.I8 => value.FitsIn<sbyte>(),
            IntegerTargetKind.I16 => value.FitsIn<short>(),
            IntegerTargetKind.I32 => value.FitsIn<int>(),
            IntegerTargetKind.I64 => value.FitsIn<long>(),
            IntegerTargetKind.I128 => value.FitsIn<Int128>(),
            IntegerTargetKind.ISize => settings.PointerWidth switch
            {
                PointerWidth.X32 => value.FitsIn<int>(),
                PointerWidth.X64 => value.FitsIn<long>(),
                _ => throw new ArgumentException("Invalid pointer width", nameof(settings)),
            },
            IntegerTargetKind.U8 => value.FitsIn<byte>(),
            IntegerTargetKind.U16 => value.FitsIn<ushort>(),
            IntegerTargetKind.U32 => value.FitsIn<uint>(),
            IntegerTargetKind.U64 => value.FitsIn<ulong>(),
            IntegerTargetKind.U128 => value.FitsIn<UInt128>(),
            IntegerTargetKind.USize => settings.PointerWidth switch
            {
                PointerWidth.X32 => value.FitsIn<uint>(),
                PointerWidth.X64 => value.FitsIn<ulong>(),
                _ => throw new ArgumentException("Invalid pointer width", nameof(settings)),
            },
            IntegerTargetKind.F32 => value.FitsIn<float>(),
            IntegerTargetKind.F64 => value.FitsIn<double>(),
            IntegerTargetKind.BestFit => value.FitsIn<UInt128>() || value.FitsIn<Int128>(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    extension(in ConstantValue value)
    {
        public ConstantValue? TryConvert(TypeSymbol typeSymbol, CompilationSettings settings)
        {
            while (true)
            {
                if (typeSymbol is NullableTypeSymbol nullableType)
                {
                    if (value.IsNull)
                        return value;
                    typeSymbol = nullableType.ElementType;
                    continue;
                }

                if (value.IsNull)
                {
                    return null;
                }

                if (typeSymbol.SpecialType.IsCharacter)
                {
                    return value.TryConvertToCharacter(typeSymbol.SpecialType);
                }

                return typeSymbol.SpecialType switch
                {
                    SpecialType.Bool => value is bool ? value : (ConstantValue?)null,
                    { IsCharacter: true } => value.TryConvertToCharacter(typeSymbol.SpecialType),

                    { IsInteger: true } => value.TryConvertToInteger(
                        typeSymbol.SpecialType,
                        settings.PointerWidth
                    ),

                    { IsFloatingPoint: true } => value.TryConvertToFloat(typeSymbol.SpecialType),

                    SpecialType.Str => value is string ? value : (ConstantValue?)null,

                    _ => null,
                };
            }
        }

        private ConstantValue? TryConvertToCharacter(SpecialType type)
        {
            return value switch
            {
                CharacterConstant ch => ch.ConvertTo(ToCharacterKind(type)),
                _ => null,
            };
        }

        private IntegerKind ToIntegerKind(SpecialType type, PointerWidth width)
        {
            return type switch
            {
                SpecialType.I8 => IntegerKind.I8,
                SpecialType.I16 => IntegerKind.I16,
                SpecialType.I32 => IntegerKind.I32,
                SpecialType.I64 => IntegerKind.I64,
                SpecialType.I128 => IntegerKind.I128,
                SpecialType.ISize => width switch
                {
                    PointerWidth.X32 => IntegerKind.I32,
                    PointerWidth.X64 => IntegerKind.I64,
                    _ => throw new ArgumentOutOfRangeException(nameof(width), width, null),
                },
                SpecialType.U8 => IntegerKind.U8,
                SpecialType.U16 => IntegerKind.U16,
                SpecialType.U32 => IntegerKind.U32,
                SpecialType.U64 => IntegerKind.U64,
                SpecialType.U128 => IntegerKind.U128,
                SpecialType.USize => width switch
                {
                    PointerWidth.X32 => IntegerKind.U32,
                    PointerWidth.X64 => IntegerKind.U64,
                    _ => throw new ArgumentOutOfRangeException(nameof(width), width, null),
                },
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
        }

        private ConstantValue? TryConvertToInteger(SpecialType type, PointerWidth pointerWidth)
        {
            var integerKind = value.ToIntegerKind(type, pointerWidth);
            return value switch
            {
                IntegerConstant i => i.ConvertTo(integerKind),
                FloatConstant f => f.ConvertTo(integerKind),
                _ => null,
            };
        }

        private ConstantValue? TryConvertToFloat(SpecialType type)
        {
            var floatKind = ToFloatKind(type);
            return value switch
            {
                IntegerConstant i => i.ConvertTo(floatKind),
                FloatConstant f => f.ConvertTo(floatKind),
                _ => null,
            };
        }
    }

    private static CharacterKind ToCharacterKind(SpecialType type)
    {
        return type switch
        {
            SpecialType.Char => CharacterKind.C8,
            SpecialType.Char16 => CharacterKind.C16,
            SpecialType.Rune => CharacterKind.C32,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    private static FloatKind ToFloatKind(SpecialType type)
    {
        return type switch
        {
            SpecialType.F32 => FloatKind.F32,
            SpecialType.F64 => FloatKind.F64,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}
