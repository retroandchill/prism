// @file SyntaxToken.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Prism.Core.Parse;
using Prism.Core.Strings;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public readonly record struct IdentifierInfo(Name Identifier, bool IsEscaped = false)
{
    public int Length => Identifier.ToString().Length + (IsEscaped ? 1 : 0);

    public override string ToString()
    {
        return IsEscaped ? $"@{Identifier}" : Identifier.ToString();
    }
}

public readonly struct SyntaxToken : ISyntaxElement
{
    public SyntaxNode? Parent { get; }
    internal GreenToken Green { get; }
    internal int Position { get; }

    public SyntaxKind Kind => Green.Kind;
    public TextSpan FullSpan => new(Position, Green.FullWidth);

    public TextSpan Span => new(Position, Green.Width);

    public string Text => Green.Text;

    public bool IsMissing => Green.IsMissing;

    public bool ContainsDiagnostics => Green.ContainsDiagnostics;

    public SyntaxTriviaList LeadingTrivia => new(in this, Green.LeadingTrivia);

    public bool HasLeadingTrivia => Green.HasLeadingTrivia;

    public SyntaxTriviaList TrailingTrivia => new(in this, Green.TrailingTrivia);

    public bool HasTrailingTrivia => Green.HasTrailingTrivia;

    internal SyntaxToken(SyntaxNode parent, GreenToken green, int position)
    {
        Parent = parent;
        Green = green;
        Position = position;
    }

    internal SyntaxToken(GreenToken green, int position)
    {
        Green = green;
        Position = position;
    }

    public bool TryGetIdentifierInfo(out IdentifierInfo info)
    {
        if (Green is GreenIdentifierToken identifierToken)
        {
            info = identifierToken.IdentifierInfo;
            return true;
        }

        info = default;
        return false;
    }

    public bool TryGetLiteralValue<T>([NotNullWhen(true)] out T? value)
        where T : ISyntaxLiteral
    {
        if (Green is GreenLiteralToken<T> literalToken)
        {
            value = literalToken.Value;
            return true;
        }

        value = default;
        return false;
    }
}
