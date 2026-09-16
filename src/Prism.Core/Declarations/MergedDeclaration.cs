using System.Collections.Immutable;

namespace Prism.Core.Declarations;

internal closed class MergedDeclaration(string name) : Declaration(name)
{
    public sealed override bool IsMerged => true;

    public new ImmutableArray<MergedDeclaration> Members => GetMergedMembers();

    protected sealed override ImmutableArray<Declaration> GetMembers()
    {
        return ImmutableArray<Declaration>.CastUp(Members);
    }

    protected abstract ImmutableArray<MergedDeclaration> GetMergedMembers();
}
