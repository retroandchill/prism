// @file IAssemblyLoader.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Metadata;

public interface IAssemblyLoader
{
    AssemblyDefinition? LoadAssembly(
        string filePath,
        CancellationToken cancellationToken = default
    );
}
