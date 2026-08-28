using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal sealed class GreenValueToken<T>(
    in T data,
    GreenNode? leadingTrivia,
    GreenNode? trailingTrivia
) : GreenToken(data.Kind, data.Text.Length, leadingTrivia, trailingTrivia)
    where T : struct, ISyntaxData
{
    public T Value { get; } = data;

    public override string Text => Value.Text;

    protected override GreenValueToken<T> UpdateInternal(
        GreenNode? leadingTrivia,
        GreenNode? trailingTrivia,
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        return new GreenValueToken<T>(Value, leadingTrivia, trailingTrivia)
        {
            Diagnostics = diagnostics,
        };
    }
}

internal static class GreenValueToken
{
    public static GreenValueToken<T> Create<T>(
        in T data,
        GreenNode? leadingTrivia = null,
        GreenNode? trailingTrivia = null,
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics = default
    )
        where T : struct, ISyntaxData
    {
        return new GreenValueToken<T>(data, leadingTrivia, trailingTrivia)
        {
            Diagnostics = diagnostics.IsDefault
                ? ImmutableArray<SyntaxDiagnosticInfo>.Empty
                : diagnostics,
        };
    }

    public static GreenValueToken<T> Create<T>(
        in T data,
        GreenSyntaxList<GreenTrivia> leadingTrivia,
        GreenSyntaxList<GreenTrivia> trailingTrivia = default,
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics = default
    )
        where T : struct, ISyntaxData
    {
        return Create(in data, leadingTrivia.Node, trailingTrivia.Node, diagnostics);
    }
}
