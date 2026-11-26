// @file NamedTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using Prism.Configuration;
using Prism.Constants;
using Prism.Semantic.Layout;
using Prism.Semantic.Symbols.Data;
using Prism.Utils;
using ZLinq;
using FloatKind = Prism.Semantic.Layout.FloatKind;

namespace Prism.Semantic.Symbols;

public sealed class NamedTypeSymbol : TypeSymbol, IAttributeHolderSymbolData
{
    private TypeLayout? _layout;

    internal NamedTypeSymbol(INamedTypeSymbolData data)
        : base(data) { }

    internal new INamedTypeSymbolData Data => Unsafe.As<INamedTypeSymbolData>(base.Data);

    public NamedTypeKind Kind => Data.Kind;

    [MemberNotNullWhen(true, nameof(EnumUnderlyingType))]
    public bool IsEnum => Kind == NamedTypeKind.Enum;

    public TypeSymbol? EnumUnderlyingType => Data.EnumUnderlyingType;

    public ImmutableArray<EnumOrdinalSymbol> EnumOrdinals => Data.EnumOrdinals;

    public ImmutableArray<ParameterSymbol> AttributeParameters => Data.AttributeParameters;

    public override TypeLayout GetLayout(CancellationToken cancellationToken = default)
    {
        if (IsDynamicallySized)
            throw new InvalidOperationException(
                "Cannot get the layout of a dynamically sized type"
            );

        return LazyComputation.LazyCompute(ref _layout, (this, cancellationToken), static t =>
        {
            var (@this, cancellationToken) = t;
            return @this.ComputeTypeLayout(cancellationToken);
        });
    }

    private TypeLayout ComputeTypeLayout(CancellationToken cancellationToken)
    {
        return Kind switch
        {
            NamedTypeKind.Class => GetTypeLayoutForClass(),
            NamedTypeKind.Enum => GetTypeLayoutForEnum(),
            NamedTypeKind.Variant => GetTypeLayoutForVariant(),
            _ => throw new ArgumentException("Type kind does not have a layout"),
        };

        TypeLayout GetTypeLayoutForClass()
        {
            var assembly = ContainingAssembly;
            Debug.Assert(assembly is not null);
            var pointerWidth = assembly.TargetInfo.PointerWidth;
            return SpecialType switch
            {
                SpecialType.None => CreateClassLayout(cancellationToken),
                SpecialType.Void or SpecialType.Str => throw new ArgumentException(
                    "Invalid special type"
                ),
                SpecialType.Bool => BoolLayout,
                SpecialType.I8 => GetIntegerLayout(8, true),
                SpecialType.I16 => GetIntegerLayout(16, true),
                SpecialType.I32 => GetIntegerLayout(32, true),
                SpecialType.I64 => GetIntegerLayout(64, true),
                SpecialType.I128 => GetIntegerLayout(128, true),
                SpecialType.ISize => GetIntegerLayout(pointerWidth.BitWidth, true),
                SpecialType.U8 => GetIntegerLayout(8, false),
                SpecialType.U16 => GetIntegerLayout(16, false),
                SpecialType.U32 => GetIntegerLayout(32, false),
                SpecialType.U64 => GetIntegerLayout(64, false),
                SpecialType.U128 => GetIntegerLayout(128, false),
                SpecialType.USize => GetIntegerLayout(pointerWidth.BitWidth, false),
                SpecialType.F32 => new FloatLayout(FloatKind.Single),
                SpecialType.F64 => new FloatLayout(FloatKind.Double),
                SpecialType.Char => GetIntegerLayout(8, false),
                SpecialType.Char16 => GetIntegerLayout(16, false),
                SpecialType.Rune => GetIntegerLayout(32, false),
                _ => throw new ArgumentException("Invalid special type"),
            };
        }

        TypeLayout GetTypeLayoutForEnum()
        {
            var underlyingLayout = (IntegerLayout)
                EnumUnderlyingType.RequireNonNull().GetLayout(cancellationToken);
            return underlyingLayout.WithValueValidity(GetEnumValidity(underlyingLayout.BitWidth));
        }

        ValueValidity? GetEnumValidity(ulong bitWidth)
        {
            if (EnumOrdinals.IsEmpty)
                return NoValuesValidity.Instance;

            var maxRepresentable = Numerics.GetMaxRepresentable(bitWidth);

            using var ordinals = EnumOrdinals
                .AsValueEnumerable()
                .Select(ordinal =>
                {
                    if (ordinal.Value is not IntegerConstant integerConstant)
                        throw new InvalidOperationException("Invalid ordinal value");

                    return (ulong)integerConstant.RawValue;
                })
                .ToArrayPool();

            ordinals.Span.Sort();

            var ranges = new List<(ulong Min, ulong Max)>();

            var start = ordinals.Span[0];
            var previous = start;

            foreach (var value in ordinals.Span.AsValueEnumerable().Skip(1))
            {
                if (value > previous + 1)
                {
                    ranges.Add((start, previous));
                    start = value;
                }

                previous = value;
            }

            ranges.Add((start, previous));

            if (ranges.Count == 1)
            {
                var (min, max) = ranges[0];
                if (min == 0 && max == maxRepresentable)
                    return null;

                return new BoundedRangeValidity(min, max);
            }

            if (ShouldUseExplicitSet(ordinals.Size, ranges.Count))
            {
                return new EnumValidity([.. ordinals.Span]);
            }

            return new MultiRangeValidity([.. ranges]);
        }

        static bool ShouldUseExplicitSet(int valueCount, int rangeCount)
        {
            return valueCount <= 8 && rangeCount >= valueCount / 2;
        }

        TypeLayout GetTypeLayoutForVariant()
        {
            throw new NotImplementedException();
        }
    }

    private TypeLayout CreateClassLayout(CancellationToken cancellationToken)
    {
        using var fields = GetMembers()
            .AsValueEnumerable()
            .OfType<VariableSymbol>()
            .Where(v => v.IsInstance)
            .ToArrayPool();

        if (fields.Size == 0)
        {
            return EmptyLayout.Instance;
        }

        ulong size = 0;
        ulong alignment = 1;
        var builder = ImmutableArray.CreateBuilder<StructField>(fields.Size);
        foreach (var field in fields.Span)
        {
            var layout = field.Type.GetLayout(cancellationToken);
            if (layout.Size == 0)
                continue;

            var alignedOffset = (size + layout.Alignment - 1) & ~(layout.Alignment - 1);
            size = alignedOffset + layout.Size;
            alignment = Math.Max(layout.Alignment, alignedOffset);

            builder.Add(new StructField(field.Name, layout, alignedOffset));
        }

        if (size == 0)
        {
            return EmptyLayout.Instance;
        }

        size = (size + alignment - 1) & ~(alignment - 1);

        return new StructLayout(new TypeLayoutInfo(size, alignment), builder.DrainToImmutable());
    }

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        switch (SpecialType)
        {
            case SpecialType.Void:
                builder.Append("void");
                break;
            case SpecialType.Bool:
                builder.Append("bool");
                break;
            case SpecialType.I8:
                builder.Append("i8");
                break;
            case SpecialType.I16:
                builder.Append("i16");
                break;
            case SpecialType.I32:
                builder.Append("i32");
                break;
            case SpecialType.I64:
                builder.Append("i64");
                break;
            case SpecialType.I128:
                builder.Append("i128");
                break;
            case SpecialType.ISize:
                builder.Append("isize");
                break;
            case SpecialType.U8:
                builder.Append("u8");
                break;
            case SpecialType.U16:
                builder.Append("u16");
                break;
            case SpecialType.U32:
                builder.Append("u32");
                break;
            case SpecialType.U64:
                builder.Append("u64");
                break;
            case SpecialType.U128:
                builder.Append("u128");
                break;
            case SpecialType.USize:
                builder.Append("usize");
                break;
            case SpecialType.F32:
                builder.Append("f32");
                break;
            case SpecialType.F64:
                builder.Append("f64");
                break;
            case SpecialType.Char:
                builder.Append("char");
                break;
            case SpecialType.Char16:
                builder.Append("char16");
                break;
            case SpecialType.Rune:
                builder.Append("rune");
                break;
            case SpecialType.Str:
                builder.Append("str");
                break;
            case SpecialType.None:
            default:
            {
                if (ContainingNamespace is { IsGlobal: false } container)
                {
                    container.WriteDisplayString(ref builder);
                    builder.Append("::");
                }

                builder.Append(Name);
                break;
            }
        }
    }
}
