using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Compiling;
using Prism.Core.Diagnostics;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Binding;

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
        LookupContext context
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
            _ => throw new ArgumentException("Invalid target type", nameof(targetType)),
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
}
