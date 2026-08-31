using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Compiling;
using Prism.Core.Diagnostics;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Binding;

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
}
