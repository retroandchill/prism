// @file TokenStream.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Nito.Collections;

namespace Prism.Core.Parse;

public sealed class TokenStream(SourceFile sourceFile)
{
    private readonly string _text = sourceFile.Text;
    private readonly Lexer _lexer = new(sourceFile);
    private readonly List<Token> _consumedTokens = [];
    private readonly Deque<Token> _pendingTokens = [];

    public bool AtEnd => Peek().Kind == TokenKind.EOF;

    public IReadOnlyList<Token> ConsumedTokens => _consumedTokens;

    public Token Previous => _consumedTokens[^1];

    public Token Peek(int count = 1)
    {
        if (_pendingTokens.Count == 0)
        {
            BufferTokens();
        }

        RemoveComments();
        return _pendingTokens.Count >= count ? _pendingTokens[count - 1] : Previous;
    }

    private void RemoveComments()
    {
        while (_pendingTokens.Count > 0 && _pendingTokens[0].Kind == TokenKind.Comment)
        {
            _consumedTokens.Add(_pendingTokens[0]);
            _pendingTokens.RemoveFromFront();
        }
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
            _pendingTokens.AddToBack(token);

            if (token.Kind == TokenKind.EOF)
                break;
        }
    }
}
