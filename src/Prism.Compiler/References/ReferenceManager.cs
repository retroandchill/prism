// @file ReferenceManager.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO.Abstractions;
using Prism.Compiler.Symbols;
using Prism.Compiler.Symbols.Metadata;
using Prism.Configuration;
using Prism.Metadata;
using Prism.Semantic.Symbols;

namespace Prism.Compiler.References;

internal sealed class ReferenceManager
{
    private readonly IAssemblyLoader _loader;
    private readonly IFileSystem _fileSystem;
    private readonly CompilationSettings _settings;

    private readonly ConcurrentDictionary<string, AssemblySymbol> _pathCache = [];
    private readonly ConcurrentDictionary<AssemblyIdentity, AssemblySymbol> _identityCache = [];

    public ReferenceManager(
        IAssemblyLoader loader,
        IFileSystem fileSystem,
        CompilationSettings settings
    )
    {
        _loader = loader;
        _fileSystem = fileSystem;
        _settings = settings;
    }

    public AssemblySymbol GetReferencedAssembly(MetadataReference reference)
    {
        return reference switch
        {
            CompilationReference(var compilation) => compilation.Assembly,
            LibraryReference(var filePath) => LoadFromLibraryReference(filePath),
            UnresolvedReference(var referenceText) => ResolveAssemblyReference(referenceText),
        };
    }

    private AssemblySymbol LoadFromLibraryReference(
        string filePath,
        AssemblyIdentity? identity = null
    )
    {
        var normalizedPath = GetTargetPath(filePath);

        var symbol = _pathCache.GetOrAdd(
            normalizedPath,
            static (path, t) =>
            {
                var (@this, identity) = t;

                var definition = @this._loader.LoadAssembly(path);
                if (definition is not null)
                    return new MetadataAssemblySymbol(definition, @this).PublicModel;

                if (identity is not null)
                    return new MissingAssemblySymbol(
                        identity,
                        @this._settings.TargetInfo
                    ).PublicSymbol;

                var baseName = @this._fileSystem.Path.GetFileNameWithoutExtension(path);
                return new MissingAssemblySymbol(
                    new AssemblyIdentity(baseName),
                    @this._settings.TargetInfo
                ).PublicSymbol;
            },
            (this, identity)
        );

        _identityCache.TryAdd(symbol.Identity, symbol);
        return symbol;
    }

    private string GetTargetPath(string path)
    {
        if (_fileSystem.Path.IsPathFullyQualified(path))
            return _fileSystem.Path.GetFullPath(path);

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var referencePath in _settings.AdditionalReferencePaths)
        {
            var fullPath = _fileSystem.Path.GetFullPath(path, referencePath);
            if (_fileSystem.File.Exists(fullPath))
                return fullPath;
        }

        return _fileSystem.Path.GetFullPath(path);
    }

    private AssemblySymbol ResolveAssemblyReference(string referenceText)
    {
        if (!AssemblyIdentity.TryParse(referenceText, out var identity))
        {
            // We just want to use the full string no matter how malformed it is
            identity = new AssemblyIdentity(referenceText);
        }

        return ResolveAssemblyReference(identity);
    }

    private AssemblySymbol ResolveAssemblyReference(AssemblyIdentity identity)
    {
        if (_identityCache.TryGetValue(identity, out var symbol))
            return symbol;

        var fileName = $"{identity.Name}.prlib";
        return LoadFromLibraryReference(fileName, identity);
    }

    public AssemblyReferences ResolveAssemblyReference(ImmutableArray<AssemblyReference> references)
    {
        if (references.IsEmpty)
            return AssemblyReferences.Empty;

        var identities = references.Select(r => r.Identity).ToImmutableArray();
        var symbols = identities.Select(ResolveAssemblyReference).ToImmutableArray();
        var unifiedSymbols = symbols
            .Select(s => new UnifiedAssembly(s.Identity, s))
            .ToImmutableArray();

        return new AssemblyReferences(identities, symbols, unifiedSymbols);
    }

    public AssemblyReferences ResolveAssemblyReference(ImmutableArray<MetadataReference> references)
    {
        if (references.IsEmpty)
            return AssemblyReferences.Empty;

        var symbols = references.Select(GetReferencedAssembly).ToImmutableArray();
        var identities = symbols.Select(r => r.Identity).ToImmutableArray();
        var unifiedSymbols = symbols
            .Select(s => new UnifiedAssembly(s.Identity, s))
            .ToImmutableArray();

        return new AssemblyReferences(identities, symbols, unifiedSymbols);
    }
}
