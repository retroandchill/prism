// @file AssemblyNameDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Metadata;

public sealed class AssemblyNameDefinition : AssemblyNameReference
{
    public AssemblyNameDefinition(string name, Version version)
        : base(name, version)
    {
        MetadataToken = new MetadataToken(TokenType.Assembly, 1);
    }
}
