using Prism.Core.Compiling;

namespace Prism.Core.Metadata;

public closed record MetadataReference
{
    public abstract string Display { get; }

    internal virtual bool IsResolved => true;
}

public sealed record CompilationReference(Compilation Compilation) : MetadataReference
{
    public override string Display => Compilation.AssemblyName;
}

public sealed record LibraryReference(string FilePath) : MetadataReference
{
    public override string Display => FilePath;
}

public sealed record UnresolvedReference(string Display) : MetadataReference
{
    public override string Display { get; } = Display;

    internal override bool IsResolved => false;
}
