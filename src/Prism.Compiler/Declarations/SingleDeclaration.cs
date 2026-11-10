using System.Collections.Immutable;
using Prism.Diagnostics;
using Prism.Syntax;

namespace Prism.Compiler.Declarations;

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

            Interlocked.CompareExchange(ref field, SourceLocation.Create(SyntaxReference), null);
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
