using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal sealed class GreenMissingToken : GreenToken
{
    public override string Text => "";

    public GreenMissingToken(
        SyntaxKind kind,
        GreenNode? leadingTrivia = null,
        GreenNode? trailingTrivia = null
    )
        : base(kind, 0, leadingTrivia, trailingTrivia)
    {
        ClearFlags(SyntaxFlags.NotMissing);
    }

    protected override GreenMissingToken UpdateInternal(
        GreenNode? leadingTrivia,
        GreenNode? trailingTrivia,
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        return new GreenMissingToken(Kind, leadingTrivia, trailingTrivia)
        {
            Diagnostics = diagnostics,
        };
    }
}
