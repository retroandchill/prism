using System.Diagnostics;
using Nito.Collections;
using Prism.Core.Syntax;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Parser;

internal sealed class TokenStream(string text)
{
    private readonly Lexer _lexer = new(text);
    private GreenToken? _previous;
    private readonly Deque<GreenToken> _lookahead = [];

    public bool AtEnd => Peek().Kind == SyntaxKind.EofToken;

    public int Position { get; private set; }

    public GreenToken Previous
    {
        get
        {
            Debug.Assert(_previous is not null);
            return _previous;
        }
    }

    public GreenToken Peek(int count = 1)
    {
        if (_lookahead.Count == 0)
        {
            BufferTokens();
        }

        return _lookahead[count - 1];
    }

    public GreenToken Consume()
    {
        var token = Peek();
        _lookahead.RemoveFromFront();
        _previous = token;
        Position++;
        return token;
    }

    public void Advance()
    {
        _ = Consume();
    }

    public void ReplaceNext(GreenToken token)
    {
        var next = Peek();
        Debug.Assert(next.Kind != SyntaxKind.EofToken);

        _lookahead.RemoveFromFront();
        _lookahead.AddToFront(token);
    }

    public void ReplaceNext(params ReadOnlySpan<GreenToken> tokens)
    {
        var next = Peek();
        Debug.Assert(next.Kind != SyntaxKind.EofToken);

        _lookahead.RemoveFromFront();
        foreach (var token in tokens)
        {
            _lookahead.AddToFront(token);
        }
    }

    private void BufferTokens()
    {
        const int maxTokens = 1024;
        for (var i = 0; i < maxTokens; i++)
        {
            var token = _lexer.Next();
            _lookahead.AddToBack(token);
            if (token.Kind == SyntaxKind.EofToken)
                break;
        }
    }
}
