// @file StringUtils.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Prism.Core.Utils;

public static class StringUtils
{
    extension(ReadOnlySpan<char> str)
    {
        public bool StartsWithAny(
            [NotNullWhen(true)] out string? matchedPrefix,
            params ReadOnlySpan<string> prefixes
        )
        {
            foreach (var prefix in prefixes)
            {
                if (!str.StartsWith(prefix, StringComparison.Ordinal))
                    continue;
                matchedPrefix = prefix;
                return true;
            }

            matchedPrefix = null;
            return false;
        }

        public bool StartsWithAny(out char matchedPrefix, params ReadOnlySpan<char> prefixes)
        {
            foreach (var prefix in prefixes)
            {
                if (!str.StartsWith(prefix))
                    continue;
                matchedPrefix = prefix;
                return true;
            }

            matchedPrefix = '\0';
            return false;
        }
    }
}
