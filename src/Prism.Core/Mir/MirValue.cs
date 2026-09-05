// @file MirValue.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Semantic;

namespace Prism.Core.Mir;

internal abstract record MirValue(MirType Type);

internal sealed record MirConstantValue(ConstantValue Constant, MirType Type) : MirValue(Type);

internal sealed record MirReadValue(MirPlace Place, MirType Type) : MirValue(Type);

internal sealed record MirAddressOfValue(MirPlace Place, MirType Type) : MirValue(Type);

internal sealed record MirNullValue : MirValue
{
    private MirNullValue()
        : base(MirVoidType.Instance) { }

    public static MirNullValue Instance { get; } = new();
}
