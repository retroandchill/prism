// @file ComparisonHelpers.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Prism.Constants;

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

    public static ConstantValue ThreeWayCompareValue<T>(T a, T b)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        const int sentinel = 2;

        var comparison1 = a == b ? 0 : sentinel;
        var comparison2 = a > b ? 1 : comparison1;
        var comparison3 = a < b ? -1 : comparison2;

        if (comparison3 != sentinel)
        {
            return IntegerConstant.From(IntegerKind.I8, unchecked((UInt128)comparison3));
        }

        return new NullConstant();
    }
}
