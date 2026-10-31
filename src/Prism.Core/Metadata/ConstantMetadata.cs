// @file ConstantHandling.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JetBrains.Annotations;
using Prism.Core.Semantic;

namespace Prism.Core.Metadata;

[MustDisposeResource]
internal sealed class ConstantHeapBuilder()
    : SerializedHeapBuilder<ConstDataHandle, ConstantValue>(ConstantMetadata.HandleShift);

internal static class ConstantMetadata
{
    public const int HandleShift = 512;
}
