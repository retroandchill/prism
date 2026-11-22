using System.Collections.Immutable;

namespace Prism.Compiler.References;

public interface IReferenceResolver
{
    string ResolveReferencePath(
        AssemblyIdentity identity,
        ReadOnlySpan<string> additionalSearchPaths = default
    );
}
