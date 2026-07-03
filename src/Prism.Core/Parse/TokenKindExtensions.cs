// @file TokenKindExtensions.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Parse;

public static partial class TokenKindExtensions
{
    extension(TokenKind)
    {
        public static TokenKind? MatchKeyword(ReadOnlySpan<char> text)
        {
            foreach (var (token, keyword) in Keywords)
            {
                if (text.Equals(keyword, StringComparison.Ordinal))
                    return token;
            }

            return null;
        }

        public static (TokenKind token, int Length)? MatchPunctuation(ReadOnlySpan<char> text)
        {
            foreach (var (token, punctuation) in Punctuations)
            {
                if (text.StartsWith(punctuation))
                    return (token, punctuation.Length);
            }

            return null;
        }
    }
}
