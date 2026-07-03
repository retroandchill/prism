// @file TokenStream.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Nito.Collections;

namespace Prism.Core.Parse;

public sealed class TokenStream(SourceFile sourceFile)
{
    private readonly Lexer _lexer = new(sourceFile);
    private readonly List<Token> _consumedTokens = [];
    private readonly Deque<Token> _pendingTokens = [];

    public bool AtEnd => Peek().Kind == TokenKind.EOF;

    public IReadOnlyList<Token> ConsumedTokens => _consumedTokens;

    public Token Previous => _consumedTokens[^1];

    public Token Peek()
    {
        if (_pendingTokens.Count == 0)
        {
            BufferTokens();
        }

        var token = _pendingTokens[0];
        if (token.Kind != TokenKind.Comment)
            return token;

        // If we see a comment, for now just skip it
        _consumedTokens.Add(token);
        _pendingTokens.RemoveFromFront();
        token = _pendingTokens[0];
        return token;
    }

    public void Advance()
    {
        var token = Peek();
        _consumedTokens.Add(token);
        _pendingTokens.RemoveFromFront();
    }

    public bool ReplaceNext(Token token)
    {
        var next = Peek();
        Debug.Assert(next.Kind != TokenKind.EOF);

        _pendingTokens.RemoveFromFront();
        _pendingTokens.AddToFront(token);
        return true;
    }

    public bool ReplaceNext(params ReadOnlySpan<Token> tokens)
    {
        var next = Peek();
        Debug.Assert(next.Kind != TokenKind.EOF);

        _pendingTokens.RemoveFromFront();
        foreach (var token in tokens)
        {
            _pendingTokens.AddToFront(token);
        }

        return true;
    }

    private void BufferTokens()
    {
        const int maxTokens = 1024;

        for (var i = 0; i < maxTokens; i++)
        {
            var token = _lexer.Next();
            _pendingTokens.AddToFront(token);

            if (token.Kind == TokenKind.EOF)
                break;
        }
    }
}
