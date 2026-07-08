// @file NullabilityExtensions.cs
// 
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Prism.Core.Utils;

public static class NullabilityExtensions
{
    public static T RequireNonNull<T>([NotNull] this T? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);
        return value;
    }
}