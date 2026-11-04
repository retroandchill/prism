// @file AssemblyDescriptor.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Metadata;

internal closed class AssemblyDescriptor(AssemblyIdentity identity)
{
    public AssemblyIdentity Identity { get; } = identity;
}

internal sealed class AssemblyReference(AssemblyRefHandle token, AssemblyIdentity identity)
    : AssemblyDescriptor(identity)
{
    public AssemblyRefHandle Handle { get; } = token;
}
