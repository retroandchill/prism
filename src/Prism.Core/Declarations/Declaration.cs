using System.Collections.Immutable;

namespace Prism.Core.Declarations;

internal enum DeclarationKind
{
    Namespace,
}

internal abstract class Declaration(string name)
{
    public string Name { get; } = name;

    public abstract DeclarationKind Kind { get; }

    public abstract bool IsMerged { get; }

    public ImmutableArray<Declaration> Members => GetMembers();

    protected abstract ImmutableArray<Declaration> GetMembers();
}
