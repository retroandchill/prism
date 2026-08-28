// @file GreenToken.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal class GreenToken : GreenNode
{
    public virtual string Text => Kind.DisplayText;

    public sealed override GreenNode? LeadingTrivia { get; }
    public sealed override int LeadingTriviaWidth => LeadingTrivia?.FullWidth ?? 0;
    public sealed override GreenNode? TrailingTrivia { get; }
    public sealed override int TrailingTriviaWidth => TrailingTrivia?.FullWidth ?? 0;

    public GreenToken(
        SyntaxKind kind,
        GreenNode? leadingTrivia = null,
        GreenNode? trailingTrivia = null
    )
        : this(kind, kind.DisplayText.Length, leadingTrivia, trailingTrivia) { }

    public GreenToken(
        SyntaxKind kind,
        int width,
        GreenNode? leadingTrivia = null,
        GreenNode? trailingTrivia = null
    )
        : base(kind, width)
    {
        LeadingTrivia = leadingTrivia;
        TrailingTrivia = trailingTrivia;

        SetFlags(SyntaxFlags.NotMissing);
        if (leadingTrivia is not null)
            AdjustFlagsAndWidth(leadingTrivia);
        if (trailingTrivia is not null)
            AdjustFlagsAndWidth(trailingTrivia);
    }

    [DoesNotReturn]
    public override GreenNode GetSlot(int index)
    {
        throw new InvalidOperationException("Tokens don't have indexable slots");
    }

    public T GetValue<T>()
        where T : struct, ISyntaxData
    {
        return TryGetValue<T>()
            ?? throw new InvalidOperationException("Token does not have a value");
    }

    public T? TryGetValue<T>()
        where T : struct, ISyntaxData
    {
        return this is GreenValueToken<T> valueToken ? valueToken.Value : default;
    }

    public GreenToken WithLeadingTrivia(GreenNode? leadingTrivia)
    {
        return leadingTrivia == LeadingTrivia
            ? this
            : UpdateInternal(leadingTrivia, TrailingTrivia, Diagnostics);
    }

    public GreenToken WithLeadingTrivia(GreenSyntaxList<GreenTrivia> leadingTrivia)
    {
        return WithLeadingTrivia(leadingTrivia.Node);
    }

    public GreenToken WithTrailingTrivia(GreenNode? trailingTrivia)
    {
        return trailingTrivia == TrailingTrivia
            ? this
            : UpdateInternal(LeadingTrivia, trailingTrivia, Diagnostics);
    }

    public GreenToken WithTrailingTrivia(GreenSyntaxList<GreenTrivia> trailingTrivia)
    {
        return WithLeadingTrivia(trailingTrivia.Node);
    }

    public sealed override GreenToken WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        return diagnostics == Diagnostics
            ? this
            : UpdateInternal(LeadingTrivia, TrailingTrivia, diagnostics);
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int positon = 0)
    {
        throw new InvalidOperationException("Cannot create a red node from a Green Token");
    }

    public GreenToken Update(GreenNode? leadingTrivia, GreenNode? trailingTrivia)
    {
        return leadingTrivia == LeadingTrivia && trailingTrivia == TrailingTrivia
            ? this
            : UpdateInternal(leadingTrivia, trailingTrivia, Diagnostics);
    }

    public GreenToken Update(
        GreenSyntaxList<GreenTrivia> leadingTrivia,
        GreenSyntaxList<GreenTrivia> trailingTrivia
    )
    {
        return Update(leadingTrivia.Node, trailingTrivia.Node);
    }

    protected virtual GreenToken UpdateInternal(
        GreenNode? leadingTrivia,
        GreenNode? trailingTrivia,
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        return new GreenToken(Kind, Width, leadingTrivia, trailingTrivia)
        {
            Diagnostics = diagnostics,
        };
    }

    public override void WriteTo(TextWriter writer)
    {
        LeadingTrivia?.WriteTo(writer);
        writer.Write(Text);
        TrailingTrivia?.WriteTo(writer);
    }

    public static GreenToken EOF { get; } = new(SyntaxKind.EofToken);
    public static GreenToken BadToken { get; } = new(SyntaxKind.BadToken);

    private static ImmutableArray<GreenToken> GetMissingTokenList(SyntaxKind start, SyntaxKind end)
    {
        var size = end.AsUnderlyingType() - start.AsUnderlyingType();
        Debug.Assert(size > 0);
        var result = new GreenToken[size];
        for (var i = 0; i < size; i++)
        {
            result[i] = new GreenMissingToken((SyntaxKind)(start.AsUnderlyingType() + i));
        }

        return ImmutableCollectionsMarshal.AsImmutableArray(result);
    }

    private static ImmutableArray<GreenToken> GetStaticTokenList(SyntaxKind start, SyntaxKind end)
    {
        var size = end.AsUnderlyingType() - start.AsUnderlyingType();
        Debug.Assert(size > 0);
        var result = new GreenToken[size];
        for (var i = 0; i < size; i++)
        {
            result[i] = new GreenToken((SyntaxKind)(start.AsUnderlyingType() + i));
        }

        return ImmutableCollectionsMarshal.AsImmutableArray(result);
    }

    private static readonly ImmutableArray<GreenToken> Keywords = GetStaticTokenList(
        SyntaxKind.KeywordStart,
        SyntaxKind.KeywordEnd
    );
    private static readonly ImmutableArray<GreenToken> Punctuators = GetStaticTokenList(
        SyntaxKind.PunctuationStart,
        SyntaxKind.PunctuationEnd
    );

    private static readonly ImmutableArray<GreenToken> MissingKeywords = GetMissingTokenList(
        SyntaxKind.KeywordStart,
        SyntaxKind.KeywordEnd
    );
    private static readonly ImmutableArray<GreenToken> MissingPunctuators = GetMissingTokenList(
        SyntaxKind.PunctuationStart,
        SyntaxKind.PunctuationEnd
    );
    private static readonly ImmutableArray<GreenToken> MissingOperators = GetMissingTokenList(
        SyntaxKind.OtherTokenStart,
        SyntaxKind.OtherTokenEnd
    );

    public static GreenToken? From(SyntaxKind kind)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (kind)
        {
            case SyntaxKind.EofToken:
                return EOF;
            case SyntaxKind.BadToken:
                return BadToken;
            default:
                if (kind.IsKeyword)
                {
                    return Keywords[
                        kind.AsUnderlyingType() - SyntaxKind.KeywordStart.AsUnderlyingType()
                    ];
                }

                return kind.IsPunctuation
                    ? Punctuators[
                        kind.AsUnderlyingType() - SyntaxKind.PunctuationStart.AsUnderlyingType()
                    ]
                    : null;
        }
    }

    public static GreenToken GetMissing(SyntaxKind kind)
    {
        if (kind.IsKeyword)
        {
            return MissingKeywords[
                kind.AsUnderlyingType() - SyntaxKind.KeywordStart.AsUnderlyingType()
            ];
        }

        if (kind.IsPunctuation)
        {
            return MissingPunctuators[
                kind.AsUnderlyingType() - SyntaxKind.PunctuationStart.AsUnderlyingType()
            ];
        }

        Debug.Assert(kind.IsOtherToken);
        return MissingOperators[
            kind.AsUnderlyingType() - SyntaxKind.OtherTokenStart.AsUnderlyingType()
        ];
    }
}
