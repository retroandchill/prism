using Prism.Compiler.References;

namespace Prism.Tests.Helpers;

public sealed class TestReferenceResolver : IReferenceResolver
{
    public string ResolveReferencePath(
        AssemblyIdentity identity,
        ReadOnlySpan<string> additionalSearchPaths = default
    )
    {
        return identity.Name;
    }
}
