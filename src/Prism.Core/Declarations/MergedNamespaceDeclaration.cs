using System.Collections.Immutable;
using Prism.Core.Compiling;
using Prism.Core.Diagnostics;
using Prism.Core.Symbols;
using ZLinq;

namespace Prism.Core.Declarations;

internal sealed class MergedNamespaceDeclaration : MergedDeclaration
{
    private MergedNamespaceDeclaration(ImmutableArray<SingleNamespaceDeclaration> declarations)
        : base(!declarations.IsEmpty ? declarations[0].Name : "")
    {
        Declarations = declarations;
    }

    public static MergedNamespaceDeclaration Create(
        ImmutableArray<SingleNamespaceDeclaration> declarations
    )
    {
        return new MergedNamespaceDeclaration(declarations);
    }

    public static MergedNamespaceDeclaration Create(SingleNamespaceDeclaration declaration)
    {
        return Create([declaration]);
    }

    public override DeclarationKind Kind => DeclarationKind.Namespace;

    public LexicalSortKey GetLexicalSortKey(Compilation compilation)
    {
        var sortKey = new LexicalSortKey(Declarations[0].NameLocation, compilation);
        foreach (var declaration in Declarations.AsValueEnumerable().Skip(1))
        {
            sortKey = LexicalSortKey.First(
                sortKey,
                new LexicalSortKey(declaration.NameLocation, compilation)
            );
        }
        return sortKey;
    }

    public ImmutableArray<SingleNamespaceDeclaration> Declarations { get; }

    public ImmutableArray<Location> NameLocations
    {
        get { return [.. Declarations.Select(Location (d) => d.NameLocation)]; }
    }

    public new ImmutableArray<MergedDeclaration> Members
    {
        get
        {
            if (!field.IsDefault)
            {
                return field;
            }

            ImmutableInterlocked.InterlockedCompareExchange(ref field, MakeChildren(), default);
            return field;
        }
    }

    public ImmutableHashSet<string> MemberNames
    {
        get
        {
            if (field is not null)
            {
                return field;
            }

            Interlocked.CompareExchange(
                ref field,
                [.. Declarations.AsValueEnumerable().SelectMany(d => d.MemberNames)],
                null
            );
            return field;
        }
    }

    protected override ImmutableArray<MergedDeclaration> GetMergedMembers()
    {
        return Members;
    }

    private ImmutableArray<MergedDeclaration> MakeChildren()
    {
        var namespaces = ImmutableArray.CreateBuilder<SingleNamespaceDeclaration>();
        var allNamespacesHaveSameName = true;

        foreach (
            var child in Declarations
                .AsValueEnumerable()
                .SelectMany(d => d.Members.AsValueEnumerable())
        )
        {
            if (child is not SingleNamespaceDeclaration asNamespace)
                continue;
            if (
                namespaces.Count > 0
                && allNamespacesHaveSameName
                && asNamespace.Name != namespaces[0].Name
            )
            {
                allNamespacesHaveSameName = false;
            }

            namespaces.Add(asNamespace);
        }

        var children = ImmutableArray.CreateBuilder<MergedDeclaration>();
        AddNamespacesToChildren(namespaces, allNamespacesHaveSameName, children);

        return children.DrainToImmutable();
    }

    private static void AddNamespacesToChildren(
        ImmutableArray<SingleNamespaceDeclaration>.Builder namespaces,
        bool allNamespacesHaveSameName,
        ImmutableArray<MergedDeclaration>.Builder children
    )
    {
        if (namespaces.Count == 0)
            return;

        if (allNamespacesHaveSameName)
        {
            children.Add(Create(namespaces.DrainToImmutable()));
            return;
        }

        foreach (var grouping in namespaces.GroupBy(ns => ns.Name))
        {
            children.Add(Create([.. grouping]));
        }
    }
}
