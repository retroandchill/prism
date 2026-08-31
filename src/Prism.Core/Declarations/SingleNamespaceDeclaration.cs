using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using Prism.Core.Utils;

namespace Prism.Core.Declarations;

internal abstract class SingleNamespaceDeclaration(
    string name,
    SyntaxReference syntaxReference,
    SourceLocation nameLocation,
    ImmutableHashSet<string> memberNames,
    ImmutableArray<SingleDeclaration> members
) : SingleDeclaration(name, syntaxReference, nameLocation, memberNames)
{
    public bool HasUsings { get; init; }

    public sealed override DeclarationKind Kind => DeclarationKind.Namespace;

    public new ImmutableArray<SingleDeclaration> Members { get; } = members;

    protected override ImmutableArray<SingleDeclaration> GetSingleMembers()
    {
        return Members;
    }
}

internal sealed class SingleNamedNamespaceDeclaration(
    string name,
    SyntaxReference syntaxReference,
    SourceLocation nameLocation,
    ImmutableHashSet<string> memberNames,
    ImmutableArray<SingleDeclaration> members
) : SingleNamespaceDeclaration(name, syntaxReference, nameLocation, memberNames, members);

internal sealed class SingleRootNamespaceDeclaration(
    SyntaxReference syntaxReference,
    SourceLocation nameLocation,
    ImmutableHashSet<string> memberNames,
    ImmutableArray<SingleDeclaration> members
)
    : SingleNamespaceDeclaration(
        CommonNames.Global,
        syntaxReference,
        nameLocation,
        memberNames,
        members
    );
