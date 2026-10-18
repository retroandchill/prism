// @file AssemblyReference.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using MagicArchive;

namespace Prism.Core.Metadata;

public sealed class AssemblyReference : AssemblyDescriptor
{
    internal AssemblyReference(AssemblyIdentity identity)
        : base(identity, new MetadataToken(TokenType.AssemblyRef)) { }

    internal uint Id
    {
        set => MetadataToken = MetadataToken with { Id = value };
    }
}
