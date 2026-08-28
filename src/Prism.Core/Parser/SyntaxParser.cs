using System.Diagnostics;
using System.Runtime.CompilerServices;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Parser;

internal abstract class SyntaxParser(string text)
{
    private readonly TokenStream _stream = new(text);

    protected bool AtEnd => _stream.AtEnd;

    protected GreenToken PeekToken(int offset = 1)
    {
        return _stream.Peek(offset);
    }

    protected GreenToken ConsumeToken()
    {
        return _stream.Consume();
    }

    protected GreenToken? MatchToken(SyntaxKind kind)
    {
        var token = PeekToken();
        if (token.Kind != kind)
            return null;

        _stream.Advance();
        return token;
    }

    protected GreenToken ExpectToken(SyntaxKind kind)
    {
        var next = PeekToken();
        if (next.Kind != kind)
        {
            return GreenToken
                .GetMissing(kind)
                .WithDiagnostics([
                    new SyntaxDiagnosticInfo(DiagnosticInfo.UnexpectedToken(next.ToString())),
                ]);
        }

        _stream.Advance();
        return next;
    }

    protected static T AddLeadingSkippedSyntax<T>(T node, GreenNode skippedSyntax)
        where T : GreenNode
    {
        if (node is GreenToken token)
        {
            return Unsafe.As<T>(AddSkippedSyntax(token, skippedSyntax, false));
        }

        var firstToken = node.FirstToken;
        Debug.Assert(firstToken is not null);
        firstToken = AddSkippedSyntax(firstToken, skippedSyntax, false);
        return (T)TokenReplacer.ReplaceFirstToken(node, firstToken);
    }

    protected static T AddTrailingSkippedSyntax<T>(T node, GreenNode skippedSyntax)
        where T : GreenNode
    {
        if (node is GreenToken token)
        {
            return Unsafe.As<T>(AddSkippedSyntax(token, skippedSyntax, true));
        }

        var firstToken = node.FirstToken;
        Debug.Assert(firstToken is not null);
        firstToken = AddSkippedSyntax(firstToken, skippedSyntax, true);
        throw new NotImplementedException();
    }

    protected static GreenToken AddSkippedSyntax(
        GreenToken target,
        GreenNode skippedSyntax,
        bool trailing
    )
    {
        var builder = new GreenListNode.Builder();

        int currentOffset;
        if (trailing)
        {
            currentOffset = target.Width + target.TrailingTriviaWidth;

            var trivia = target.TrailingTrivia;
            if (trivia is not null)
                builder.Add(trivia);
        }
        else
        {
            currentOffset = -target.LeadingTriviaWidth - skippedSyntax.FullWidth;
        }

        SyntaxDiagnosticInfo? diagnostic = null;
        int finalDiagnosticOffset = 0;

        foreach (var node in skippedSyntax.EnumerateNodes())
        {
            if (node is GreenToken token)
            {
                var leadingTrivia = token.LeadingTrivia;
                if (leadingTrivia is not null)
                    builder.Add(leadingTrivia);

                if (token.Width > 0)
                {
                    builder.Add(new GreenSkippedTokensTrivia(token.Update((GreenNode?)null, null)));
                }
                else
                {
                    if (!token.Diagnostics.IsEmpty)
                    {
                        diagnostic = token.Diagnostics[0];
                        finalDiagnosticOffset =
                            currentOffset + token.LeadingTriviaWidth + diagnostic.Value.Offset;
                    }
                }

                var trailingTrivia = token.TrailingTrivia;
                if (trailingTrivia is not null)
                    builder.Add(trailingTrivia);

                currentOffset += token.FullWidth;
            }
            else if (node.ContainsDiagnostics && !diagnostic.HasValue)
            {
                if (!node.Diagnostics.IsEmpty)
                {
                    diagnostic = node.Diagnostics[0];
                    finalDiagnosticOffset =
                        currentOffset + node.TrailingTriviaWidth + node.FullWidth;
                }
            }
        }

        if (diagnostic.HasValue)
        {
            target = target.WithDiagnostics(
                target.Diagnostics.Add(
                    new SyntaxDiagnosticInfo(
                        diagnostic.Value.Info,
                        finalDiagnosticOffset,
                        diagnostic.Value.Width
                    )
                )
            );
        }

        if (trailing)
        {
            return target.WithTrailingTrivia(builder.BuildAndClear());
        }

        builder.AddRange(target.LeadingTrivia);
        return target.WithLeadingTrivia(builder.BuildAndClear());
    }
}
