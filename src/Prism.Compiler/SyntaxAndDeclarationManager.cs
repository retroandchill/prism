using System.Collections.Immutable;
using Prism.Compiler.Declarations;
using Prism.Syntax;
using Prism.Text;

namespace Prism.Compiler;

using LazyRootNamespace = Lazy<SingleRootNamespaceDeclaration>;

internal sealed class SyntaxState
{
    public required ImmutableArray<SyntaxTree> SyntaxTrees { get; init; }
    public required ImmutableDictionary<ISourceDocument, int> SyntaxTreeIndices { get; init; }
    public required ImmutableDictionary<
        ISourceDocument,
        LazyRootNamespace
    > RootNamespaceDeclarations { get; init; }
    public required DeclarationTable Declarations { get; init; }
}

internal sealed class SyntaxAndDeclarationManager(
    ImmutableArray<SyntaxTree> externalSyntaxTrees,
    SyntaxState? state = null
)
{
    public SyntaxState State
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, CreateState(externalSyntaxTrees), null);
            return field;
        }
        private init;
    } = state;

    private static SyntaxState CreateState(ImmutableArray<SyntaxTree> externalSyntaxTrees)
    {
        var treeBuilder = ImmutableArray.CreateBuilder<SyntaxTree>();
        var ordinalMapBuilder = ImmutableDictionary.CreateBuilder<ISourceDocument, int>(
            ReferenceEqualityComparer.Instance
        );
        var declMapBuilder = ImmutableDictionary.CreateBuilder<ISourceDocument, LazyRootNamespace>(
            ReferenceEqualityComparer.Instance
        );
        var tableBuilder = DeclarationTable.Empty.ToBuilder();

        foreach (var tree in externalSyntaxTrees)
        {
            AppendAllSyntaxTrees(
                treeBuilder,
                tree,
                ordinalMapBuilder,
                declMapBuilder,
                tableBuilder
            );
        }

        return new SyntaxState
        {
            SyntaxTrees = treeBuilder.DrainToImmutable(),
            SyntaxTreeIndices = ordinalMapBuilder.ToImmutable(),
            RootNamespaceDeclarations = declMapBuilder.ToImmutable(),
            Declarations = tableBuilder.Build(),
        };
    }

    private static void AppendAllSyntaxTrees(
        ImmutableArray<SyntaxTree>.Builder treeBuilder,
        SyntaxTree tree,
        ImmutableDictionary<ISourceDocument, int>.Builder ordinalMapBuilder,
        ImmutableDictionary<ISourceDocument, LazyRootNamespace>.Builder declMapBuilder,
        DeclarationTable.Builder tableBuilder
    )
    {
        AddSyntaxTreeToDeclarationMapAndTable(tree, declMapBuilder, tableBuilder);
        ordinalMapBuilder.Add(tree, treeBuilder.Count);
        treeBuilder.Add(tree);
    }

    private static void AddSyntaxTreeToDeclarationMapAndTable(
        SyntaxTree tree,
        ImmutableDictionary<ISourceDocument, LazyRootNamespace>.Builder declMapBuilder,
        DeclarationTable.Builder tableBuilder
    )
    {
        var lazyRoot = new LazyRootNamespace(() => DeclarationTreeBuilder.ForTree(tree));
        declMapBuilder.Add(tree, lazyRoot);
        tableBuilder.AddRootDeclaration(lazyRoot);
    }
}
