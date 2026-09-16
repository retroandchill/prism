using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Declarations;

internal closed class SingleDeclaration(
    string name,
    SyntaxReference syntaxReference,
    SourceLocation nameLocation,
    ImmutableHashSet<string> memberNames
) : Declaration(name)
{
    public sealed override bool IsMerged => false;

    public SourceLocation Location
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, new SourceLocation(SyntaxReference), null);
            return field;
        }
    }

    public SyntaxReference SyntaxReference { get; } = syntaxReference;

    public SourceLocation NameLocation { get; } = nameLocation;

    public ImmutableHashSet<string> MemberNames { get; } = memberNames;

    public ImmutableArray<Diagnostic> Diagnostics { get; init; } = [];

    public new ImmutableArray<SingleDeclaration> Members => GetSingleMembers();

    protected sealed override ImmutableArray<Declaration> GetMembers()
    {
        return ImmutableArray<Declaration>.CastUp(Members);
    }

    protected abstract ImmutableArray<SingleDeclaration> GetSingleMembers();
}
