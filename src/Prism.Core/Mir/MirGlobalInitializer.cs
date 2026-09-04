// @file MirGlobalInitializer.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Mir;

namespace Prism.Core.Mir;

internal abstract record MirGlobalInitializer;

internal sealed record MirNoGlobalInitializer : MirGlobalInitializer
{
    private MirNoGlobalInitializer() { }

    public static readonly MirNoGlobalInitializer Instance = new();
}

internal sealed record MirConstantGlobalInitializer(MirConstantValue Value) : MirGlobalInitializer;

internal sealed record MirComputedGlobalInitializer : MirGlobalInitializer
{
    private MirComputedGlobalInitializer() { }

    public static readonly MirComputedGlobalInitializer Instance = new();
}
