using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal sealed class GreenValueToken<T>(
    in T data,
    string text,
    GreenNode? leadingTrivia,
    GreenNode? trailingTrivia
) : GreenToken(data.Kind, text.Length, leadingTrivia, trailingTrivia)
    where T : struct, ISyntaxData
{
    public T Value { get; } = data;

    public override string Text { get; } = text;

    protected override GreenValueToken<T> UpdateInternal(
        GreenNode? leadingTrivia,
        GreenNode? trailingTrivia,
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        return new GreenValueToken<T>(Value, Text, leadingTrivia, trailingTrivia)
        {
            Diagnostics = diagnostics,
        };
    }
}
