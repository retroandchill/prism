// @file GreenToken.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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

    public GreenToken WithLeadingTrivia(GreenNode? leadingTrivia)
    {
        return leadingTrivia == LeadingTrivia
            ? this
            : UpdateInternal(leadingTrivia, TrailingTrivia, Diagnostics);
    }

    public GreenToken WithTrailingTrivia(GreenNode? trailingTrivia)
    {
        return trailingTrivia == TrailingTrivia
            ? this
            : UpdateInternal(LeadingTrivia, trailingTrivia, Diagnostics);
    }

    public sealed override GreenToken WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        return diagnostics == Diagnostics
            ? this
            : UpdateInternal(LeadingTrivia, TrailingTrivia, diagnostics);
    }

    public override SyntaxNode CrateRed(SyntaxNode? parent, int positon = 0)
    {
        throw new InvalidOperationException("Cannot create a red node from a Green Token");
    }

    public GreenToken Update(GreenNode? leadingTrivia, GreenNode? trailingTrivia)
    {
        return leadingTrivia == LeadingTrivia && trailingTrivia == TrailingTrivia
            ? this
            : UpdateInternal(leadingTrivia, trailingTrivia, Diagnostics);
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
}
