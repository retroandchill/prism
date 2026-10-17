// @file AssemblyDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Metadata;

public sealed class AssemblyDefinition : IDisposable
{
    public required AssemblyNameDefinition Name { get; set; }

    internal AssemblyDefinition() { }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
