using Prism.Core.Compiling;

namespace Prism.Core.Metadata;

public closed class MetadataReference
{
    public string Display { get; }

    internal virtual bool IsResolved => true;

    private protected MetadataReference(string display)
    {
        Display = display;
    }
}

public sealed class CompilationReference(Compilation compilation)
    : MetadataReference(compilation.AssemblyName)
{
    public Compilation Compilation { get; } = compilation;
}

public sealed class LibraryReference(string filePath, string? display = null)
    : MetadataReference(display ?? filePath)
{
    public string FilePath { get; } = filePath;
}

public sealed class UnresolvedReference(string display) : MetadataReference(display)
{
    internal override bool IsResolved => false;
}
