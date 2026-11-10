// @file UnionInterlocked.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Prism.Utils;

/// <summary>
/// Collection of interlocked operations for union types.
/// <remarks>
/// These functions assume that the union types are the ones defined by the union keyword, which consist of a single
/// field of type <see cref="object"/>. Any unions that are manually defined using the <see cref="UnionAttribute"/>
/// and the <see cref="IUnion"/> interface that do not conform to this pattern will result in undefined behavior.
/// </remarks>
/// </summary>
public static class UnionInterlocked
{
    public static void Initialize<T>(ref T location, T value)
        where T : struct, IUnion
    {
        Interlocked.CompareExchange(ref Unsafe.As<T, object?>(ref location), value.Value, null);
    }
}
