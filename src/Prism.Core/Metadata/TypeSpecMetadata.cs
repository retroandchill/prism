// @file TypeSpecMetadata.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JetBrains.Annotations;

namespace Prism.Core.Metadata;

[MustDisposeResource]
internal sealed class TypeSpecHeapBuilder()
    : SerializedHeapBuilder<TypeSpecHandle, TypeSpecRow>(TypeSpecMetadata.HandleShift);

internal static class TypeSpecMetadata
{
    public const int HandleShift = 1;
}
