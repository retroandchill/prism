// @file TokenStream.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Nito.Collections;

namespace Prism.Core.Parse;

public sealed class TokenStream(string source)
{
    private Lexer _lexer = new(source);
    private readonly List<Token> _consumed = [];
    private readonly Deque<Token> _pending = [];

    public bool AtEnd => _consumed.Count > 0 && Peek().Kind == TokenKind.Eof;

    public IReadOnlyList<Token> Consumed => _consumed;

    public Token Previous => _consumed[^1];

    public Token Peek()
    {
        if (_pending.Count == 0)
        {
            BufferTokens();
        }

        return _pending[0];
    }

    public Token Consume()
    {
        Advance();
        return Previous;
    }

    public void Advance()
    {
        var token = Peek();
        _pending.RemoveFromFront();
        _consumed.Add(token);
    }

    public void ReplaceNext(Token token)
    {
        if (_pending.Count == 0)
        {
            BufferTokens();
        }

        _pending[0] = token;
    }

    public void ReplaceNext(params Token[] tokens)
    {
        if (_pending.Count == 0)
        {
            BufferTokens();
        }

        _pending.RemoveFromFront();
        foreach (var token in tokens)
        {
            _pending.AddToFront(token);
        }
    }

    private void BufferTokens()
    {
        const int maxTokens = 1024;

        for (var i = 0; i < maxTokens; i++)
        {
            var token = _lexer.NextToken();
            _pending.AddToBack(token);

            if (token.Kind == TokenKind.Eof)
            {
                break;
            }
        }
    }
}
