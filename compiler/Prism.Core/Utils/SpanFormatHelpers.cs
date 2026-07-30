// @file SpanFormatHelpers.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;

namespace Prism.Core.Utils;

internal static class SpanFormatHelpers
{
    public static string ToString<T>(
        T value,
        string? format,
        IFormatProvider? formatProvider,
        int initialBufferSize = 32
    )
        where T : ISpanFormattable
    {
        Span<char> destination = stackalloc char[initialBufferSize];
        char[]? rentedArray = null;
        try
        {
            int charsWritten;
            while (!value.TryFormat(destination, out charsWritten, format, formatProvider))
            {
                var nextArray = ArrayPool<char>.Shared.Rent(destination.Length * 2);
                if (rentedArray is not null)
                    ArrayPool<char>.Shared.Return(rentedArray);

                rentedArray = nextArray;
                destination = rentedArray;
            }

            return new string(destination[..charsWritten]);
        }
        finally
        {
            if (rentedArray is not null)
                ArrayPool<char>.Shared.Return(rentedArray);
        }
    }
}
