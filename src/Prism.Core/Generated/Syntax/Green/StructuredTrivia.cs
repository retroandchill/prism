// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal sealed class GreenSkippedTokensTrivia : GreenStructuredTrivia
{
    GreenSkippedTokensTrivia(GreenSyntaxList<GreenToken> tokens)
        : base(SyntaxKind.SkippedTokensTrivia)
    {
        Tokens = tokens;
    }

    public GreenSyntaxList<GreenToken> Tokens { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index == 0 ? Tokens.Node : null;
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenSkippedTokensTrivia WithTokens(GreenSyntaxList<GreenToken> tokens)
    {
        if (Tokens == tokens)
            return this;

        return new GreenSkippedTokensTrivia(tokens) { Diagnostics = Diagnostics };
    }

    public override GreenSkippedTokensTrivia WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenSkippedTokensTrivia(Tokens) { Diagnostics = diagnostics };
    }

    public GreenSkippedTokensTrivia Update(GreenSyntaxList<GreenToken> tokens)
    {
        if (Tokens == tokens)
        {
            return this;
        }

        return new GreenSkippedTokensTrivia(tokens) { Diagnostics = Diagnostics };
    }
}
