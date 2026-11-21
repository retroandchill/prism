using System.Diagnostics;
using Prism.Syntax.Green;

namespace Prism.Syntax.Parser;

internal sealed class TokenStream(string text)
{
    private readonly Lexer _lexer = new(text);
    private GreenToken? _current;
    private readonly List<GreenToken> _tokens = [];

    public bool AtEnd => Current.Kind == SyntaxKind.EofToken;

    public int Position { get; private set; }

    public GreenToken Current
    {
        get
        {
            _current ??= GetCurrent();
            return _current;
        }
    }

    private GreenToken GetCurrent()
    {
        if (Position >= _tokens.Count)
        {
            BufferTokens();
        }

        return _tokens[Position];
    }

    private GreenToken? GetCurrentWithoutBuffering()
    {
        return Position < _tokens.Count ? _tokens[Position] : null;
    }

    public GreenToken Peek(int count)
    {
        Debug.Assert(count >= 1);
        var index = Position + count;
        BufferThrough(index);
        return _tokens[index];
    }

    public GreenToken Consume()
    {
        var token = Current;
        Position++;
        _current = GetCurrentWithoutBuffering();
        return token;
    }

    public void Advance()
    {
        _ = Consume();
    }

    public void Reset(int position)
    {
        Debug.Assert(position >= 0);
        Debug.Assert(position <= _tokens.Count);
        Position = position;
        _current = GetCurrentWithoutBuffering();
    }

    private void BufferThrough(int index)
    {
        while (_tokens.Count <= index)
        {
            BufferTokens();
        }

        Debug.Assert(index < _tokens.Count);
    }

    private void BufferTokens()
    {
        if (_tokens.Count > 0 && _tokens[^1].Kind == SyntaxKind.EofToken)
            return;

        const int maxTokens = 1024;
        for (var i = 0; i < maxTokens; i++)
        {
            var token = _lexer.Next();
            _tokens.Add(token);
            if (token.Kind == SyntaxKind.EofToken)
                break;
        }
    }
}
