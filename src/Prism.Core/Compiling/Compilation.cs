using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Binding;
using Prism.Core.BoundTree;
using Prism.Core.Codegen;
using Prism.Core.Configuration;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Symbols.Intrinsic;
using Prism.Core.Syntax;

namespace Prism.Core.Compiling;

internal sealed record EntryPoint(FunctionSymbol? Function, ImmutableArray<Diagnostic> Diagnostics);

public class Compilation
{
    private readonly SyntaxAndDeclarationManager _syntaxAndDeclarationManager;

    internal Compilation(
        string assemblyName,
        CompilationSettings settings,
        SyntaxAndDeclarationManager manager
    )
    {
        AssemblyName = assemblyName;
        Settings = settings;
        _syntaxAndDeclarationManager = manager;
    }

    public static Compilation Create(
        string assemblyName,
        ImmutableArray<SyntaxTree> trees,
        CompilationSettings? settings = null
    )
    {
        if (trees.IsDefaultOrEmpty)
            throw new InvalidOperationException("Cannot create compilation with no syntax trees");

        return new Compilation(
            assemblyName,
            settings ?? CompilationSettings.CurrentPlatform,
            new SyntaxAndDeclarationManager(trees)
        );
    }

    public CompilationSettings Settings { get; }

    public string AssemblyName { get; }

    public AssemblySymbol Assembly
    {
        get { throw new NotImplementedException(); }
    }

    public NamespaceSymbol GlobalNamespace
    {
        get { throw new NotImplementedException(); }
    }

    public NamespaceSymbol? GetCompilationNamespace(NamespaceSymbol assemblyNamespace)
    {
        return Cache.GetCompilationNamespace(assemblyNamespace);
    }

    public ImmutableArray<SyntaxTree> SyntaxTrees => _syntaxAndDeclarationManager.State.SyntaxTrees;

    public bool ContainsSyntaxTree(SyntaxTree tree)
    {
        return _syntaxAndDeclarationManager.State.RootNamespaceDeclarations.ContainsKey(tree);
    }

    public SemanticModel GetSemanticModel(SyntaxTree tree)
    {
        return Cache.GetSemanticModel(tree);
    }

#pragma warning disable CA1822
    public NamedTypeSymbol GetSpecialType(SpecialType specialType)
    {
        return IntrinsicSymbols.GetType(specialType);
    }
#pragma warning restore CA1822

    public NamedTypeSymbol CreateErrorTypeSymbol(Symbol? containingSymbol, string name)
    {
        return Cache.CreateErrorTypeSymbol(containingSymbol, name);
    }

    public NamespaceSymbol CreateErrorNamespaceSymbol(
        NamespaceSymbol? containingNamespace,
        string name
    )
    {
        return Cache.CreateErrorNamespaceSymbol(containingNamespace, name);
    }

    public EmitResult Emit(string filepath)
    {
        throw new NotImplementedException();
    }

    public FunctionSymbol? GetEntryPoint()
    {
        return GetEntryPointAndDiagnostics().Function;
    }

    internal DeclarationTable Declarations => _syntaxAndDeclarationManager.State.Declarations;

    internal MergedNamespaceDeclaration MergedNamespaceDeclaration =>
        Declarations.GetMergedRoot(this);

    internal int CompareSourceLocations(Location x, Location y)
    {
        if (x is not SourceLocation sourceX || y is not SourceLocation sourceY)
        {
            throw new ArgumentException("Arguments must be a SourceLocation");
        }

        return CompareSourceLocations(sourceX, sourceY);
    }

    internal int CompareSourceLocations(SourceLocation x, SourceLocation y)
    {
        var comparison = CompareSyntaxTreeOrdering(x.SyntaxTree, y.SyntaxTree);
        return comparison != 0 ? comparison : x.SourceSpan.Start.CompareTo(y.SourceSpan.Start);
    }

    internal int CompareSourceLocations(SyntaxReference x, SyntaxReference y)
    {
        var comparison = CompareSyntaxTreeOrdering(x.SyntaxTree, y.SyntaxTree);
        return comparison != 0 ? comparison : x.SourceSpan.Start.CompareTo(y.SourceSpan.Start);
    }

    internal int CompareSourceLocations(SyntaxNode x, SyntaxNode y)
    {
        var comparison = CompareSyntaxTreeOrdering(x.SyntaxTree, y.SyntaxTree);
        return comparison != 0 ? comparison : x.Span.Start.CompareTo(y.Span.Start);
    }

    internal int CompareSyntaxTreeOrdering(SyntaxTree x, SyntaxTree y)
    {
        if (ReferenceEquals(x, y))
            return 0;

        Debug.Assert(ContainsSyntaxTree(x));
        Debug.Assert(ContainsSyntaxTree(y));
        return GetSyntaxTreeOrdinal(x).CompareTo(GetSyntaxTreeOrdinal(y));
    }

    internal int GetSyntaxTreeOrdinal(SyntaxTree tree)
    {
        Debug.Assert(ContainsSyntaxTree(tree));
        return _syntaxAndDeclarationManager.State.SyntaxTreeIndices[tree];
    }

    internal BinderFactory GetBinderFactory(SyntaxTree tree)
    {
        return Cache.GetBinderFactory(tree);
    }

    internal Binder RootBinder => Cache.RootBinder;

    internal ImmutableArray<VariableSymbol> GetGlobalVariables()
    {
        return Cache.GetGlobalVariables();
    }

    internal ImmutableArray<FunctionSymbol> GetGlobalFunctions()
    {
        return Cache.GetGlobalFunctions();
    }

    internal BoundExpression? GetBoundInitializer(VariableSymbol variable)
    {
        throw new NotImplementedException();
    }

    internal BoundStatement? GetBoundBody(FunctionSymbol function)
    {
        throw new NotImplementedException();
    }

    private EntryPoint GetEntryPointAndDiagnostics()
    {
        throw new NotImplementedException();
    }

    private EntryPoint ComputeEntryPoint()
    {
        throw new NotImplementedException();
    }

    private FunctionSymbol? FindEntryPoint(DiagnosticBag diagnostics)
    {
        throw new NotImplementedException();
    }

    private static void AppendEntryPoints(
        NamespaceSymbol ns,
        IList<FunctionSymbol> entryPoints,
        DiagnosticBag diagnostics
    )
    {
        throw new NotImplementedException();
    }

    private static bool IsValidEntryPoint(FunctionSymbol function, DiagnosticBag diagnostics)
    {
        throw new NotImplementedException();
    }

    internal DiagnosticBag DeclarationDiagnostics { get; } = DiagnosticBag.Create();

    private CompilationCache Cache
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, new CompilationCache(this), null);
            return field;
        }
    }
}
