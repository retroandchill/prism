using System.Collections.Immutable;

namespace Prism.Core.Semantic.Layout;

/// <summary>
/// Describes restrictions on valid bit patterns for a type.
/// Used by the layout builder to identify scalar niches (e.g., null-pointer optimization).
/// </summary>
public closed record ValueValidity;

public sealed record UnconstrainedValidity : ValueValidity
{
    private UnconstrainedValidity() { }

    public static readonly UnconstrainedValidity Instance = new UnconstrainedValidity();
}

/// <summary>
/// A single bit pattern (or set of bit patterns) is invalid and available as a "niche".
/// Example: NonNull pointers (0x0 is invalid) or Rust-like NonZeroInt types.
/// </summary>
public sealed record NicheValidity(ulong Validity) : ValueValidity
{
    public static readonly NicheValidity NonNull = new NicheValidity(0);
}

/// <summary>
/// Valid values fall strictly within an inclusive integer range [Min, Max].
/// Example: Ada/Pascal ranged integers, or sub-byte bitfields.
/// </summary>
public sealed record BoundedRangeValidity(ulong MinValidValue, ulong MaxValidValue) : ValueValidity;

/// <summary>
/// Only a explicit set of bit patterns are valid.
/// Example: Sparse C-style enums or bool (where only 0x0 and 0x1 are valid).
/// </summary>
public sealed record EnumValidity(ImmutableArray<ulong> ValidValues) : ValueValidity;
