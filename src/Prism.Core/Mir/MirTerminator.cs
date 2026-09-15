// @file MirTerminator.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Diagnostics;

namespace Prism.Core.Mir;

internal abstract record MirTerminator
{
    public SourceLocation? Location { get; init; }
}

internal sealed record MirGotoTerminator(MirBlockId Target) : MirTerminator;

internal sealed record MirBranchTerminator(
    MirValue Condition,
    MirBlockId WhenTrue,
    MirBlockId WhenFalse
) : MirTerminator;

internal sealed record MirReturnTerminator(MirValue? Value = null) : MirTerminator
{
    public static readonly MirReturnTerminator Void = new();
}

internal sealed record MirUnreachableTerminator : MirTerminator
{
    private MirUnreachableTerminator() { }

    public static readonly MirUnreachableTerminator Instance = new();
}
