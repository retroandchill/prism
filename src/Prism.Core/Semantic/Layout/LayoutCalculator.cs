using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Prism.Core.Compiling;
using Prism.Core.Configuration;
using Prism.Core.Symbols;
using ZLinq;

namespace Prism.Core.Semantic.Layout;

internal static class LayoutCalculator
{
    public static TypeLayout GetTypeLayout(
        Compilation compilation,
        TypeSymbol typeSymbol,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        return typeSymbol switch
        {
            NamedTypeSymbol namedTypeSymbol => GetTypeLayout(compilation, namedTypeSymbol),
            ArrayTypeSymbol arrayTypeSymbol => GetTypeLayout(
                compilation,
                arrayTypeSymbol,
                cancellationToken
            ),
            ReferenceTypeSymbol referenceTypeSymbol => GetTypeLayout(
                compilation,
                referenceTypeSymbol
            ),
            NullableTypeSymbol nullableTypeSymbol => GetTypeLayout(
                compilation,
                nullableTypeSymbol,
                cancellationToken
            ),
        };
    }

    private static TypeLayout GetTypeLayout(Compilation compilation, NamedTypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType switch
        {
            SpecialType.None => throw new NotImplementedException(),
            SpecialType.Void or SpecialType.Str => throw new ArgumentException(
                "Invalid special type"
            ),
            SpecialType.Bool => BoolLayout,
            SpecialType.I8 => GetIntegerLayout(8, true),
            SpecialType.I16 => GetIntegerLayout(16, true),
            SpecialType.I32 => GetIntegerLayout(32, true),
            SpecialType.I64 => GetIntegerLayout(64, true),
            SpecialType.I128 => GetIntegerLayout(128, true),
            SpecialType.ISize => GetIntegerLayout(compilation.Settings.PointerWidth.BitWidth, true),
            SpecialType.U8 => GetIntegerLayout(8, false),
            SpecialType.U16 => GetIntegerLayout(16, false),
            SpecialType.U32 => GetIntegerLayout(32, false),
            SpecialType.U64 => GetIntegerLayout(64, false),
            SpecialType.U128 => GetIntegerLayout(128, false),
            SpecialType.USize => GetIntegerLayout(
                compilation.Settings.PointerWidth.BitWidth,
                false
            ),
            SpecialType.F32 => new FloatLayout(FloatKind.Single),
            SpecialType.F64 => new FloatLayout(FloatKind.Double),
            SpecialType.Char => GetIntegerLayout(8, false),
            SpecialType.Char16 => GetIntegerLayout(16, false),
            SpecialType.Rune => GetIntegerLayout(32, false),
            _ => throw new ArgumentException("Invalid special type"),
        };
    }

    private static ArrayLayout GetTypeLayout(
        Compilation compilation,
        ArrayTypeSymbol typeSymbol,
        CancellationToken cancellationToken
    )
    {
        Debug.Assert(typeSymbol.Size is not null);
        var elementType = compilation.GetTypeLayout(typeSymbol.ElementType, cancellationToken);
        return new ArrayLayout(elementType, typeSymbol.Size.Value);
    }

    private static TypeLayout GetTypeLayout(Compilation compilation, ReferenceTypeSymbol typeSymbol)
    {
        var pointerWidth = compilation.Settings.PointerWidth;
        if (typeSymbol.ReferencedType.IsDynamicallySized)
        {
            return pointerWidth switch
            {
                PointerWidth.X32 => Slice32,
                PointerWidth.X64 => Slice64,
                _ => throw new ArgumentException("Invalid pointer width"),
            };
        }

        return pointerWidth switch
        {
            PointerWidth.X32 => Reference32,
            PointerWidth.X64 => Reference64,
            _ => throw new ArgumentException("Invalid pointer width"),
        };
    }

    private static TypeLayout GetTypeLayout(
        Compilation compilation,
        NullableTypeSymbol typeSymbol,
        CancellationToken cancellationToken
    )
    {
        var innerLayout = GetTypeLayout(compilation, typeSymbol.ElementType, cancellationToken);
        return CachedNullableLayouts.GetOrAdd(innerLayout, MakeNullableLayout, cancellationToken);
    }

    private static NullableLayout MakeNullableLayout(
        TypeLayout innerLayout,
        CancellationToken cancellationToken
    )
    {
        var optimizedLayout = GetOptimizedNullableLayout(innerLayout, cancellationToken);
        if (optimizedLayout is not null)
            return optimizedLayout;

        return new TaggedNullableLayout(innerLayout);
    }

    private static OptimizedNullableLayout? GetOptimizedNullableLayout(
        TypeLayout innerLayout,
        CancellationToken cancellationToken
    )
    {
        return CachedOptimizedNullableLayouts.GetOrAdd(
            innerLayout,
            MakeOptimizedNullableLayout,
            cancellationToken
        );
    }

    private static OptimizedNullableLayout? MakeOptimizedNullableLayout(
        TypeLayout innerLayout,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        return innerLayout switch
        {
            // We don't need to optimize an empty object because that doesn't get placed into the layout anyway,
            // and a nullable of a nullable could get nasty really fast, so we only optimize non-nullable values
            EmptyLayout or NullableLayout => null,
            ScalarLayout scalar => MakeOptimizedNullableLayout(scalar),
            StructLayout structLayout => MakeOptimizedNullableLayout(
                structLayout,
                cancellationToken
            ),
            CompositeLayout => null,
        };
    }

    private static OptimizedNullableLayout? MakeOptimizedNullableLayout(ScalarLayout innerLayout)
    {
        return innerLayout.Validity switch
        {
            UnconstrainedValidity => null,
            NicheValidity(var pattern) => new OptimizedNullableLayout(
                innerLayout,
                new ScalarNullValue(pattern)
            ),
            BoundedRangeValidity(var min, var max) => TryGetAdjacentNiche(
                innerLayout.BitWidth,
                min,
                max
            )
                is { } nicheValue
                ? new OptimizedNullableLayout(innerLayout, new ScalarNullValue(nicheValue))
                : null,
            EnumValidity(var values) => TryGetEnumNiche(innerLayout.BitWidth, values)
                is { } nicheValue
                ? new OptimizedNullableLayout(innerLayout, new ScalarNullValue(nicheValue))
                : null,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong GetMaxRepresentable(ulong bitWidth)
    {
        return bitWidth == 64 ? ulong.MaxValue : (1UL << (int)bitWidth) - 1;
    }

    private static ulong? TryGetAdjacentNiche(ulong bitWidth, ulong minValid, ulong maxValid)
    {
        var maxRepresentable = GetMaxRepresentable(bitWidth);

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

    private static OptimizedNullableLayout? MakeOptimizedNullableLayout(
        StructLayout innerLayout,
        CancellationToken cancellationToken
    )
    {
        foreach (var (i, field) in innerLayout.Fields.AsValueEnumerable().Index())
        {
            var nullableLayout = GetOptimizedNullableLayout(field.Type, cancellationToken);
            if (nullableLayout is null)
                continue;

            var nullValue = new CompositeNullValue(i, nullableLayout.NullValue);
            return new OptimizedNullableLayout(innerLayout, nullValue);
        }

        return null;
    }

    internal static readonly IntegerLayout BoolLayout = new(
        8,
        false,
        new BoundedRangeValidity(0, 1)
    );

    private static readonly ImmutableArray<IntegerLayout> SignedLayouts = MakeIntegerLayouts(
        128,
        true
    );
    private static readonly ImmutableArray<IntegerLayout> UnsignedLayouts = MakeIntegerLayouts(
        128,
        false
    );

    private const int ByteSize = (int)ScalarLayout.BitsPerByte;

    private static ImmutableArray<IntegerLayout> MakeIntegerLayouts(int maxBitSize, bool isSigned)
    {
        Debug.Assert(maxBitSize % ByteSize == 0);

        var size = maxBitSize / ByteSize;
        return
        [
            .. Enumerable
                .Range(0, size)
                .Select(bitSize => new IntegerLayout((ulong)(bitSize + 1) * ByteSize, isSigned)),
        ];
    }

    private static IntegerLayout GetIntegerLayout(int bitSize, bool isSigned)
    {
        Debug.Assert(bitSize % ByteSize == 0);
        var index = bitSize / ByteSize - 1;
        return isSigned ? SignedLayouts[index] : UnsignedLayouts[index];
    }

    private static readonly PointerLayout Reference32 = new(PointerWidth.X32, true);
    private static readonly PointerLayout Reference64 = new(PointerWidth.X64, true);

    private static readonly StructLayout Slice32 = MakeSliceLayout(PointerWidth.X32);
    private static readonly StructLayout Slice64 = MakeSliceLayout(PointerWidth.X64);

    private static StructLayout MakeSliceLayout(PointerWidth width)
    {
        var pointer = width switch
        {
            PointerWidth.X32 => Reference32,
            PointerWidth.X64 => Reference64,
            _ => throw new ArgumentOutOfRangeException(nameof(width), width, null),
        };

        var size = GetIntegerLayout(width.BitWidth, false);

        Debug.Assert(pointer.Alignment == size.Alignment);
        var typeLayoutInfo = new TypeLayoutInfo(pointer.Size + size.Size, pointer.Alignment);
        return new StructLayout(
            typeLayoutInfo,
            [new StructField("data", pointer, 0), new StructField("size", size, pointer.Size)]
        );
    }

    private static readonly ConditionalWeakTable<TypeLayout, NullableLayout> CachedNullableLayouts =
        new();
    private static readonly ConditionalWeakTable<
        TypeLayout,
        OptimizedNullableLayout?
    > CachedOptimizedNullableLayouts = new();
}
