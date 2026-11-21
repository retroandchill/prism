using System.Collections.Immutable;

namespace Prism.Semantic.Layout;

/// <summary>
/// Describes restrictions on valid bit patterns for a type.
/// Used by the layout builder to identify scalar niches (e.g., null-pointer optimization).
/// </summary>
public closed record ValueValidity;

public sealed record UnconstrainedValidity : ValueValidity
{
    private UnconstrainedValidity() { }

    public static readonly UnconstrainedValidity Instance = new();
}

public sealed record NoValuesValidity : ValueValidity
{
    private NoValuesValidity() { }

    public static readonly NoValuesValidity Instance = new();
}

/// <summary>
/// A single bit pattern (or set of bit patterns) is invalid and available as a "niche".
/// Example: NonNull pointers (0x0 is invalid) or Rust-like NonZeroInt types.
/// </summary>
public sealed record NicheValidity(ulong Validity) : ValueValidity
{
    public static readonly NicheValidity NonNull = new(0);
}

/// <summary>
/// Valid values fall strictly within an inclusive integer range [Min, Max].
/// Example: Ada/Pascal ranged integers, or sub-byte bitfields.
/// </summary>
public sealed record BoundedRangeValidity(ulong MinValidValue, ulong MaxValidValue) : ValueValidity;

/// <summary>
/// Valid values fall strictly within multiple inclusive integer range [Min, Max].
/// Example: Ada/Pascal ranged integers, or sub-byte bitfields.
/// </summary>
public sealed record MultiRangeValidity(
    ImmutableArray<(ulong MinValidValue, ulong MaxValidValue)> Ranges
) : ValueValidity;

/// <summary>
/// Only a explicit set of bit patterns are valid.
/// Example: Sparse C-style enums
/// </summary>
public sealed record EnumValidity(ImmutableArray<ulong> ValidValues) : ValueValidity;

/// <summary>
/// Uses a bit mask to determine valid values.
/// Example: Flag-based enums
/// </summary>
public sealed record BitMaskValidity(ulong Mask) : ValueValidity;
