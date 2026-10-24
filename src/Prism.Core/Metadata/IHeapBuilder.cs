// @file IHeapBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Metadata;

internal interface IHeapBuilder
{
    ReadOnlySpan<byte> Data { get; }

    int Count { get; }
}
