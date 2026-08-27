// @file TokenKindExtensions.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Syntax;

public static partial class SyntaxKindExtensions
{
    extension(SyntaxKind kind)
    {
        public static SyntaxKind? MatchKeyword(ReadOnlySpan<char> text)
        {
            foreach (var (token, keyword) in Keywords)
            {
                if (text.Equals(keyword, StringComparison.Ordinal))
                    return token;
            }

            return null;
        }

        public static (SyntaxKind token, int Length)? MatchPunctuation(ReadOnlySpan<char> text)
        {
            foreach (var (token, punctuation) in Punctuations)
            {
                if (text.StartsWith(punctuation))
                    return (token, punctuation.Length);
            }

            return null;
        }

        public bool IsTrivia => kind.AsUnderlyingType() is > 0 and < 100;

        public bool IsToken => kind.AsUnderlyingType() is >= 100 and <= 1000;

        public bool IsNode => kind.AsUnderlyingType() is >= 1000;
    }
}
