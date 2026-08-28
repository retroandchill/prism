using System.Diagnostics;
using Nito.Collections;
using Prism.Core.Syntax;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Parser;

internal sealed class TokenStream(string text)
{
    private readonly Lexer _lexer = new(text);
    private readonly List<GreenToken> _tokens = [];
    private readonly Deque<GreenToken> _lookahead = [];

    public bool AtEnd => Peek().Kind == SyntaxKind.EofToken;

    public IReadOnlyList<GreenToken> Tokens => _tokens;

    public GreenToken Previous
    {
        get
        {
            Debug.Assert(_tokens.Count > 0);
            return _tokens[^1];
        }
    }

    public GreenToken Peek(int count = 1)
    {
        if (_lookahead.Count == 0)
        {
            BufferTokens();
        }

        return _lookahead.Count > 0 ? _lookahead[count - 1] : Previous;
    }

    public GreenToken Consume()
    {
        var token = Peek();
        _tokens.Add(token);
        _lookahead.RemoveFromFront();
        return token;
    }

    public void Advance()
    {
        var token = Peek();
        _tokens.Add(token);
        _lookahead.RemoveFromFront();
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
