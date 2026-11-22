using System.IO.Abstractions;

namespace Prism.Compiler.References;

public sealed class FileSystemReferenceResolver(IFileSystem fileSystem) : IReferenceResolver
{
    public string ResolveReferencePath(
        AssemblyIdentity identity,
        ReadOnlySpan<string> additionalSearchPaths = default
    )
    {
        return NormalizePath($"{identity}.prlib", additionalSearchPaths);
    }

    private string NormalizePath(string path, ReadOnlySpan<string> additionalSearchPaths)
    {
        if (fileSystem.Path.IsPathFullyQualified(path))
            return fileSystem.Path.GetFullPath(path);

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var referencePath in additionalSearchPaths)
        {
            var fullPath = fileSystem.Path.GetFullPath(path, referencePath);
            if (fileSystem.File.Exists(fullPath))
                return fullPath;
        }

        return fileSystem.Path.GetFullPath(path);
    }
}
