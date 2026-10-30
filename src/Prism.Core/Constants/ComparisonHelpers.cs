// @file ComparisonHelpers.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Prism.Core.Constants;

internal enum ComparisonOperation
{
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
}

internal static class ComparisonHelpers
{
    public static bool CompareValues<T>(T a, T b, ComparisonOperation operation)
        where T : unmanaged, INumber<T>
    {
        return operation switch
        {
            ComparisonOperation.Equal => a == b,
            ComparisonOperation.NotEqual => a != b,
            ComparisonOperation.LessThan => a < b,
            ComparisonOperation.LessThanOrEqual => a <= b,
            ComparisonOperation.GreaterThan => a > b,
            ComparisonOperation.GreaterThanOrEqual => a >= b,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
        };
    }
}
