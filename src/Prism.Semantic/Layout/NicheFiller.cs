using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Prism.Utils;

namespace Prism.Semantic.Layout;

internal static class NicheFiller
{
    public static ulong? TryGetNiche(ValueValidity validity, ulong bitWidth)
    {
        return validity switch
        {
            UnconstrainedValidity => null,
            NoValuesValidity => 0,
            NicheValidity(var pattern) => pattern,
            BoundedRangeValidity(var min, var max) => TryGetAdjacentNiche(bitWidth, min, max),
            MultiRangeValidity(var ranges) => TryGetMultiRangeNiche(bitWidth, ranges),
            EnumValidity(var values) => TryGetEnumNiche(bitWidth, values),
            BitMaskValidity(var mask) => TryGetBitMaskNiche(bitWidth, mask),
        };
    }

    private static ulong? TryGetAdjacentNiche(ulong bitWidth, ulong minValid, ulong maxValid)
    {
        var maxRepresentable = Numerics.GetMaxRepresentable(bitWidth);

        // If 0 is available, then we should use that first
        if (minValid > 0)
        {
            return 0;
        }

        if (maxValid < maxRepresentable)
        {
            return maxValid + 1;
        }

        return null;
    }

    private static ulong? TryGetMultiRangeNiche(
        ulong bitWidth,
        ImmutableArray<(ulong MinValidValue, ulong MaxValidValue)> ranges
    )
    {
        Debug.Assert(!ranges.IsEmpty, "Cannot calculate niche for empty range");
        var maxRepresentable = Numerics.GetMaxRepresentable(bitWidth);

        var minValue = ranges[0].MinValidValue;
        if (minValue > 0)
        {
            return 0;
        }

        for (var i = 1; i < ranges.Length; i++)
        {
            var (_, previousMax) = ranges[i - 1];
            var (currentMin, _) = ranges[i];
            if (previousMax + 1 < currentMin)
            {
                return previousMax + 1;
            }
        }

        var maxValid = ranges[^1].MaxValidValue;
        if (maxValid < maxRepresentable)
        {
            return maxValid + 1;
        }

        return null;
    }

    private static ulong? TryGetEnumNiche(ulong bitWidth, ImmutableArray<ulong> values)
    {
        Debug.Assert(values.Length > 0);
        const int maxStackSize = 64;
        ulong[]? rentedArray = null;
        if (values.Length > maxStackSize)
        {
            rentedArray = ArrayPool<ulong>.Shared.Rent(values.Length);
        }
        var sortedValues = rentedArray is not null
            ? rentedArray.AsSpan(0, values.Length)
            : stackalloc ulong[maxStackSize];
        try
        {
            values.CopyTo(sortedValues);
            sortedValues.Sort();

            if (
                TryGetAdjacentNiche(bitWidth, sortedValues[0], sortedValues[^1]) is
                { } adjacentNiche
            )
            {
                return adjacentNiche;
            }

            // If we don't have a valid adjacent, then we end up iterating through looking for a hole
            for (var i = 1; i < sortedValues.Length; i++)
            {
                var previous = sortedValues[i - 1];
                var current = sortedValues[i];
                if (previous != current - 1)
                {
                    return current - 1;
                }
            }

            return null;
        }
        finally
        {
            if (rentedArray is not null)
            {
                ArrayPool<ulong>.Shared.Return(rentedArray);
            }
        }
    }

    private static ulong? TryGetBitMaskNiche(ulong bitWidth, ulong allowedMask)
    {
        var representableMask = Numerics.GetMaxRepresentable(bitWidth);
        var invalidBits = representableMask & ~allowedMask;

        if (invalidBits == 0)
            return null;

        return invalidBits & (~invalidBits + 1);
    }
}
