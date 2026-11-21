// @file DefaultAssemblyLoaderFactory.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

namespace Prism.Metadata;

public sealed class DefaultAssemblyLoader(IFileSystem fileSystem) : IAssemblyLoader
{
    private readonly ConcurrentDictionary<
        string,
        WeakReference<AssemblyDefinition>
    > _loadedAssemblies = new();
    private readonly ConcurrentDictionary<string, Lock> _loadLocks = new();

    public AssemblyDefinition? LoadAssembly(
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedPath = fileSystem.Path.GetFullPath(filePath);
        var fileInfo = fileSystem.FileInfo.New(normalizedPath);
        if (!fileInfo.Exists)
            return null;

        if (TryGetLoadedAssembly(normalizedPath, out var cachedAssembly))
            return cachedAssembly;

        var loadLock = _loadLocks.GetOrAdd(normalizedPath, static _ => new Lock());

        using var scope = loadLock.EnterScope();

        if (TryGetLoadedAssembly(normalizedPath, out cachedAssembly))
            return cachedAssembly;

        cancellationToken.ThrowIfCancellationRequested();

        var assembly = LoadAssemblyCore(fileInfo, cancellationToken);
        _loadedAssemblies[normalizedPath] = new WeakReference<AssemblyDefinition>(assembly);
        return assembly;
    }

    private bool TryGetLoadedAssembly(
        string filePath,
        [NotNullWhen(true)] out AssemblyDefinition? assembly
    )
    {
        if (
            _loadedAssemblies.TryGetValue(filePath, out var weakReference)
            && weakReference.TryGetTarget(out assembly)
        )
        {
            return true;
        }

        assembly = null;
        return false;
    }

    private static AssemblyDefinition LoadAssemblyCore(
        IFileInfo filePath,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        return AssemblyDefinition.Load(filePath);
    }
}
