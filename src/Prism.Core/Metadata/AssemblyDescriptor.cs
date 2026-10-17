// @file AssemblyDescriptor.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Metadata;

public closed class AssemblyDescriptor : IMetadataTokenProvider
{
    public AssemblyIdentity Identity { get; }

    public string Name => Identity.Name;
    public Version Version => Identity.Version;

    public MetadataToken MetadataToken { get; private protected set; }

    private protected AssemblyDescriptor(AssemblyIdentity identity, MetadataToken metadataToken)
    {
        MetadataToken = metadataToken;
        Identity = identity;
    }
}
