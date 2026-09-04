using System.Collections.Immutable;
using System.Diagnostics;
using Cysharp.Text;
using Prism.Core.Binding;
using Prism.Core.BoundTree;
using Prism.Core.Codegen;
using Prism.Core.Configuration;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Symbols.Intrinsic;
using Prism.Core.Symbols.Source;
using Prism.Core.Syntax;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Compiling;

internal sealed record EntryPoint(FunctionSymbol? Function, ImmutableArray<Diagnostic> Diagnostics)
{
    public static readonly EntryPoint Empty = new(null, []);
}

public class Compilation
{
    private readonly SyntaxAndDeclarationManager _syntaxAndDeclarationManager;
    private EntryPoint? _entryPoint;

    private Compilation(
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
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, new SourceAssemblySymbol(this), null);
            return field;
        }
    }

    public NamespaceSymbol GlobalNamespace
    {
        get
        {
            if (field is not null)
                return field;

            var mergedNamespace = MergedNamespaceSymbol.Create(
                this,
                null,
                [Assembly.GlobalNamespace, IntrinsicSymbols.GlobalNamespace]
            );
            Interlocked.CompareExchange(ref field, mergedNamespace, null);
            return field;
        }
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

    public ArrayTypeSymbol CreateArrayTypeSymbol(TypeSymbol elementType, ulong? size = null)
    {
        return Cache.CreateArrayTypeSymbol(elementType, size);
    }

    public ReferenceTypeSymbol CreateReferenceTypeSymbol(
        TypeSymbol elementType,
        bool isMutable = false
    )
    {
        return Cache.CreateReferenceTypeSymbol(elementType, isMutable);
    }

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
        using var emitter = new LlvmCodeEmitter(
            this,
            new CodeGenOptions { OutputDirectory = filepath }
        );
        return emitter.Emit();
    }

    public FunctionSymbol? GetEntryPoint()
    {
        return GetEntryPointAndDiagnostics().Function;
    }

    internal DeclarationTable Declarations => _syntaxAndDeclarationManager.State.Declarations;

    internal MergedNamespaceDeclaration MergedRootDeclaration => Declarations.GetMergedRoot(this);

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
        var context = BindingContext.Create(DeclarationDiagnostics);
        foreach (var reference in variable.DeclaringSyntaxReferences)
        {
            var semanticModel = GetSemanticModel(reference.SyntaxTree);
            if (reference.Syntax is not VariableDeclarationSyntax declaration)
                continue;

            return semanticModel.GetBoundVariableInitializer(declaration, context);
        }

        return null;
    }

    internal BoundStatement? GetBoundBody(FunctionSymbol function)
    {
        var context = BindingContext.Create(DeclarationDiagnostics);
        foreach (var reference in function.DeclaringSyntaxReferences)
        {
            var semanticModel = GetSemanticModel(reference.SyntaxTree);
            if (reference.Syntax is not FunctionDeclarationSyntax declaration)
                continue;

            return semanticModel.GetBoundFunctionBody(declaration, context);
        }

        return null;
    }

    private EntryPoint GetEntryPointAndDiagnostics()
    {
        if (_entryPoint is not null)
            return _entryPoint;

        Interlocked.CompareExchange(ref _entryPoint, ComputeEntryPoint(), null);
        return _entryPoint;
    }

    private EntryPoint ComputeEntryPoint()
    {
        if (!Settings.IsApplication)
            return EntryPoint.Empty;

        var diagnostics = DiagnosticBag.Create();
        var function = FindEntryPoint(diagnostics);
        return new EntryPoint(function, diagnostics.ToImmutableAndClear());
    }

    private FunctionSymbol? FindEntryPoint(DiagnosticBag diagnostics)
    {
        var globalNamespace = Assembly.GlobalNamespace;

        var candidates = new List<FunctionSymbol>();
        AppendEntryPoints(globalNamespace, candidates, diagnostics);
        if (candidates.Count == 0)
        {
            diagnostics.Add(Diagnostic.NoEntryPointDefined(Location.None));
            return null;
        }

        if (candidates.Count == 1)
            return candidates[0];

        var writer = new ZStringWriter();
        var locations = new List<Location>();
        foreach (var (i, candidate) in candidates.AsValueEnumerable().Index())
        {
            if (i > 0)
                writer.Write(", ");

            candidate.WriteDisplayString(writer);
            locations.AddRange(candidate.Locations.AsSpan());
        }

        diagnostics.Add(
            Diagnostic.AmbiguousEntryPoint(locations[0], locations.Skip(1), writer.ToString())
        );
        return null;
    }

    private static void AppendEntryPoints(
        NamespaceSymbol ns,
        List<FunctionSymbol> entryPoints,
        DiagnosticBag diagnostics
    )
    {
        foreach (var member in ns.GetMembers())
        {
            switch (member)
            {
                case NamespaceSymbol n:
                    AppendEntryPoints(n, entryPoints, diagnostics);
                    break;
                case FunctionSymbol f:
                    if (IsValidEntryPoint(f, diagnostics))
                        entryPoints.Add(f);
                    break;
            }
        }
    }

    private static bool IsValidEntryPoint(FunctionSymbol function, DiagnosticBag diagnostics)
    {
        if (function.Name != CommonNames.Main)
            return false;

        var declaration = (FunctionDeclarationSyntax)function.DeclaringSyntaxReferences[0].Syntax;

        var returnType = function.ReturnType;
        if (!returnType.IsVoid && returnType.SpecialType != SpecialType.I32)
        {
            diagnostics.Add(
                Diagnostic.InvalidEntryPointReturnType(
                    declaration.ReturnType!.Type.Location,
                    function.ReturnType.ToDisplayString(),
                    function.ToDisplayString()
                )
            );
            return false;
        }

        // TODO: For now we're going to take in no arguments, but eventually we'll allow one with a span of strings.
        if (function.Parameters.IsEmpty)
            return true;

        var paramTypes = string.Join(
            ", ",
            function.Parameters.Select(p => p.Type.ToDisplayString())
        );
        diagnostics.Add(
            Diagnostic.InvalidEntryPointParameters(
                declaration.Parameters.Location,
                paramTypes,
                function.ToDisplayString()
            )
        );
        return false;
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
