namespace Prism.Core.Metadata;

internal closed class MemberContainerDefinition(string name, AssemblyDefinition assembly)
{
    public string Name { get; } = name;
    public AssemblyDefinition Assembly { get; } = assembly;

    protected abstract MemberContainerDefHandle Handle { get; }
}
