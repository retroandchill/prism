using System.Collections.Frozen;
using Cysharp.Text;
using Prism.SourceGenerator.Models.Resolved;
using Prism.SourceGenerator.Models.Spec;
using Prism.SourceGenerator.Utilities;
using ZLinq;

namespace Prism.SourceGenerator.Resolution;

public sealed class DiagnosticModelBuilder
{
    private readonly List<DiagnosticCategory> _diagnostics = [];

    public DiagnosticModel Build(DiagnosticsSpecification spec)
    {
        LoadDiagnostics(spec);
        return new DiagnosticModel([.. _diagnostics]);
    }

    private void LoadDiagnostics(DiagnosticsSpecification spec)
    {
        const int start = 1000;
        const int step = 1000;

        foreach (var (i, category) in spec.Diagnostics.AsValueEnumerable().Index())
        {
            var firstInstance = start + step * i;
            var resolvedCategory = new DiagnosticCategory(
                category.Name,
                firstInstance,
                firstInstance + step - 1
            );
            foreach (var (j, diagnostic) in category.Diagnostics.AsValueEnumerable().Index())
            {
                var resolvedDiagnostic = new Diagnostic(
                    diagnostic.Name,
                    firstInstance + j,
                    resolvedCategory,
                    diagnostic.Severity,
                    diagnostic.Format,
                    diagnostic.Explanation,
                    diagnostic.HelpLink,
                    diagnostic.Tags
                );
                var arguments = ResolveArguments(diagnostic, resolvedDiagnostic);
                ResolveMessageParts(resolvedDiagnostic, arguments);
                resolvedCategory.AddDiagnostic(resolvedDiagnostic);
            }
            _diagnostics.Add(resolvedCategory);
        }
    }

    private static FrozenDictionary<string, DiagnosticArgument> ResolveArguments(
        DiagnosticDefinition definition,
        Diagnostic diagnostic
    )
    {
        if (definition.Args.Length == 0)
            return FrozenDictionary<string, DiagnosticArgument>.Empty;

        var dictionary = new Dictionary<string, DiagnosticArgument>();
        foreach (var arg in definition.Args)
        {
            var resolvedArgument = new DiagnosticArgument(arg.Name, arg.Type, diagnostic);
            diagnostic.AddArgument(resolvedArgument);
            dictionary.Add(arg.Name, resolvedArgument);
        }
        return dictionary.ToFrozenDictionary();
    }

    private static void ResolveMessageParts(
        Diagnostic diagnostic,
        FrozenDictionary<string, DiagnosticArgument> arguments
    )
    {
        using var currentTextSegment = ZString.CreateStringBuilder();

        var cursor = new TextCursor(diagnostic.Format);
        while (!cursor.IsAtEnd)
        {
            if (cursor.TryConsume('{'))
            {
                if (cursor.TryConsume('{'))
                {
                    currentTextSegment.Append('{');
                }
                else
                {
                    if (currentTextSegment.Length > 0)
                    {
                        diagnostic.AddMessagePart(
                            new DiagnosticMessageTextPart(currentTextSegment.ToString())
                        );
                        currentTextSegment.Clear();
                    }

                    var remaining = cursor.Remaining;
                    var start = cursor.Position;
                    while (cursor.Current != '}')
                    {
                        if (cursor.IsAtEnd)
                        {
                            throw new InvalidOperationException(
                                $"Unexpected end of file at position {cursor.Position}"
                            );
                        }

                        cursor.Advance();
                    }

                    var chunk = remaining[..(cursor.Position - start)].ToString();
                    if (!arguments.TryGetValue(chunk, out var arg))
                        throw new InvalidOperationException($"Unexpected argument name {chunk}");

                    cursor.Advance();
                    diagnostic.AddMessagePart(new DiagnosticMessageArgumentPart(arg));
                }
            }
            else if (cursor.TryConsume('}'))
            {
                if (cursor.TryConsume('}'))
                {
                    currentTextSegment.Append('}');
                }
                else
                {
                    throw new InvalidOperationException("Unmatched '}'");
                }
            }
            else
            {
                currentTextSegment.Append(cursor.Current);
                cursor.Advance();
            }
        }

        if (currentTextSegment.Length > 0)
        {
            diagnostic.AddMessagePart(new DiagnosticMessageTextPart(currentTextSegment.ToString()));
        }
    }
}
