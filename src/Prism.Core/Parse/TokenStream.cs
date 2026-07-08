// @file TokenStream.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Nito.Collections;
using Prism.Core.Syntax;

namespace Prism.Core.Parse;

public sealed class TokenStream(SourceFile sourceFile)
{
    private readonly string _text = sourceFile.Text;
    private readonly Lexer _lexer = new(sourceFile);
    private readonly List<SyntaxToken> _consumedTokens = [];
    private readonly Deque<SyntaxToken> _pendingTokens = [];

    public bool AtEnd => Peek().Kind == SyntaxKind.EndOfFileToken;

    public IReadOnlyList<SyntaxToken> ConsumedTokens => _consumedTokens;

    public SyntaxToken Previous => _consumedTokens[^1];

    public SyntaxToken Peek(int count = 1)
    {
        if (_pendingTokens.Count == 0)
        {
            BufferTokens();
        }

        return _pendingTokens.Count >= count ? _pendingTokens[count - 1] : Previous;
    }

    public void Advance()
    {
        var token = Peek();
        _consumedTokens.Add(token);
        _pendingTokens.RemoveFromFront();
    }

    public bool ReplaceNext(SyntaxToken token)
    {
        var next = Peek();
        Debug.Assert(next.Kind != SyntaxKind.EndOfFileToken);

        _pendingTokens.RemoveFromFront();
        _pendingTokens.AddToFront(token);
        return true;
    }

    public bool ReplaceNext(params ReadOnlySpan<SyntaxToken> tokens)
    {
        var next = Peek();
        Debug.Assert(next.Kind != SyntaxKind.EndOfFileToken);

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

            if (token.Kind == SyntaxKind.EndOfFileToken)
                break;
        }
    }
}
