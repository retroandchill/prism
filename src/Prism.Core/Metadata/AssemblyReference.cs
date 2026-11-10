// @file AssemblyDescriptor.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Metadata;

namespace Prism.Core.Metadata;

public sealed class AssemblyReference(AssemblyRefHandle token, AssemblyIdentity identity)
{
    public AssemblyIdentity Identity { get; } = identity;

    public AssemblyRefHandle Handle { get; } = token;
}
