// @file NullableTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Cysharp.Text;
using Prism.Semantic.Layout;
using Prism.Semantic.Symbols.Data;
using ZLinq;

namespace Prism.Semantic.Symbols;

public sealed class NullableTypeSymbol : TypeSymbol
{
    private static readonly ConditionalWeakTable<
        TypeLayout,
        NullableLayout
    > CachedNullableLayouts = [];
    private static readonly ConditionalWeakTable<
        TypeLayout,
        OptimizedNullableLayout?
    > CachedOptimizedNullableLayouts = [];

    internal NullableTypeSymbol(TypeSymbol elementType)
        : base(new DataInternal(elementType)) { }

    private new DataInternal Data => Unsafe.As<DataInternal>(base.Data);

    public TypeSymbol ElementType => Data.ElementType;

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        ElementType.WriteDisplayString(ref builder);
        builder.Append('?');
    }

    public override TypeLayout GetLayout(CancellationToken cancellationToken = default)
    {
        var innerLayout = ElementType.GetLayout(cancellationToken);
        return CachedNullableLayouts.GetOrAdd(innerLayout, MakeNullableLayout, cancellationToken);
    }

    public override bool Equals(TypeSymbol other, SymbolCompareKind kind)
    {
        return Equals(other as NullableTypeSymbol, kind);
    }

    private bool Equals(NullableTypeSymbol? other, SymbolCompareKind kind)
    {
        if (ReferenceEquals(this, other))
            return true;

        return other is not null && ElementType.Equals(other.ElementType, kind);
    }

    public override int GetHashCode(SymbolCompareKind kind) =>
        HashCode.Combine(ElementType.GetHashCode(kind));

    private sealed record DataInternal(TypeSymbol ElementType) : ITypeSymbolData
    {
        public string Name => $"{ElementType.Name}?";
        public Symbol? ContainingSymbol => null;
        public bool IsDynamicallySized => false;
        public bool IsDefaultConstructible => true;
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
        return NicheFiller.TryGetNiche(innerLayout.Validity, innerLayout.BitWidth) is { } nicheValue
            ? new OptimizedNullableLayout(innerLayout, new ScalarNullValue(nicheValue))
            : null;
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
}
