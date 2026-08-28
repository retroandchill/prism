// @file SyntaxToken.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Prism.Core.Diagnostics;
using Prism.Core.Strings;
using Prism.Core.Syntax.Green;
using Prism.Core.Text;

namespace Prism.Core.Syntax;

public readonly record struct IdentifierInfo(Name Identifier, bool IsEscaped = false)
{
    public int Length => Identifier.ToString().Length + (IsEscaped ? 1 : 0);

    public override string ToString()
    {
        return IsEscaped ? $"@{Identifier}" : Identifier.ToString();
    }
}

public readonly struct SyntaxToken
{
    internal SyntaxToken(GreenToken token, int position)
    {
        Green = token;
        Position = position;
    }

    internal SyntaxToken(GreenToken token, SyntaxNode? parent, int position)
    {
        Green = token;
        Parent = parent;
        Position = position;
    }

    internal GreenToken Green { get; }

    public SyntaxKind Kind => Green.Kind;
    internal int Position { get; }
    internal int Width => Green.Width;
    internal int FullWidth => Green.FullWidth;
    public TextSpan FullSpan => new(Position, Green.FullWidth);
    public TextSpan Span => new(Position + Green.LeadingTriviaWidth, Green.Width);

    public SyntaxNode? Parent { get; }
    public SyntaxTree? SyntaxTree => Parent?.SyntaxTree;
    public Location Location => throw new NotImplementedException();

    public bool IsMissing => Green.IsMissing;
    public bool ContainsDiagnostics => Green.ContainsDiagnostics;

    public SyntaxTriviaList LeadingTrivia => new(in this, Green.LeadingTrivia, Position);
    public bool HasLeadingTrivia => Green.HasLeadingTrivia;

    public SyntaxTriviaList TrailingTrivia
    {
        get
        {
            var trailingGreen = Green.TrailingTrivia;
            var trailingPosition = Position + FullWidth;
            if (trailingGreen is not null)
            {
                trailingPosition -= trailingGreen.FullWidth;
            }

            return new SyntaxTriviaList(in this, Green.TrailingTrivia, trailingPosition);
        }
    }

    public bool HasTrailingTrivia => Green.HasTrailingTrivia;

    public T GetValue<T>()
        where T : struct, ISyntaxData
    {
        return TryGetValue<T>()
            ?? throw new InvalidOperationException("Token does not have a value");
    }

    public T? TryGetValue<T>()
        where T : struct, ISyntaxData
    {
        return Green.TryGetValue<T>();
    }
}
