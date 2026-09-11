using System.Collections.Immutable;
using System.Diagnostics;
using Cysharp.Text;
using Prism.Core.Abi;
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
using Prism.Core.Text;
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
        using var context = BindingContext.Create();
        FunctionCompiler.CompileFunctions(this, emitter, context, CancellationToken.None);
        return emitter.Emit(context);
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

    private AbiClassifierFactory AbiClassifierFactory
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, new AbiClassifierFactory(this), null);
            return field;
        }
    }

    internal FunctionAbi GetFunctionAbi(FunctionSymbol function)
    {
        var classifier = AbiClassifierFactory.Get(function.AbiKind);
        return Cache.GetFunctionAbi(classifier, function);
    }

    internal ImmutableArray<VariableSymbol> GetGlobalVariables()
    {
        return Cache.GetGlobalVariables();
    }

    internal ImmutableArray<FunctionSymbol> GetGlobalFunctions()
    {
        return Cache.GetGlobalFunctions();
    }

    internal BoundVariableInitializer GetBoundInitializer(
        VariableSymbol variable,
        CancellationToken cancellationToken = default
    )
    {
        return Cache.GetBoundInitializer(variable, cancellationToken);
    }

    internal BoundFunctionBody GetBoundBody(
        FunctionSymbol function,
        CancellationToken cancellationToken = default
    )
    {
        return Cache.GetBoundFunctionBody(function, cancellationToken);
    }

    internal EntryPoint GetEntryPointAndDiagnostics()
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

        using var diagnostics = DiagnosticBag.Create();
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

    public ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default)
    {
        return GetDiagnostics(CompilationStage.Compile, true, null, cancellationToken);
    }

    public ImmutableArray<Diagnostic> GetParseDiagnostics(
        CancellationToken cancellationToken = default
    )
    {
        return GetDiagnostics(CompilationStage.Parse, false, null, cancellationToken);
    }

    public ImmutableArray<Diagnostic> GetDeclarationDiagnostics(
        CancellationToken cancellationToken = default
    )
    {
        return GetDiagnostics(CompilationStage.Declare, false, null, cancellationToken);
    }

    public ImmutableArray<Diagnostic> GetFunctionBodyDiagnostics(
        CancellationToken cancellationToken = default
    )
    {
        return GetDiagnostics(CompilationStage.Compile, false, null, cancellationToken);
    }

    private ImmutableArray<Diagnostic> GetDiagnostics(
        CompilationStage stage,
        bool includeEarlierStages,
        Predicate<Symbol>? symbolFilter,
        CancellationToken cancellationToken
    )
    {
        using var diagnostics = DiagnosticBag.Create();
        GetDiagnostics(stage, includeEarlierStages, diagnostics, symbolFilter, cancellationToken);
        return diagnostics.ToImmutableAndClear();
    }

    private void GetDiagnostics(
        CompilationStage stage,
        bool includeEarlierStages,
        DiagnosticBag diagnostics,
        Predicate<Symbol>? symbolFilter,
        CancellationToken cancellationToken
    )
    {
        using var context = BindingContext.Create();
        GetAllDiagnostics(stage, includeEarlierStages, context, symbolFilter, cancellationToken);
        diagnostics.AddRange(context.AccumulatedDiagnostics);
    }

    private void GetAllDiagnostics(
        CompilationStage stage,
        bool includeEarlierStages,
        BindingContext context,
        Predicate<Symbol>? symbolFilter,
        CancellationToken cancellationToken
    )
    {
        if (
            stage == CompilationStage.Parse
            || (stage > CompilationStage.Parse && includeEarlierStages)
        )
        {
            foreach (var syntaxTree in SyntaxTrees)
            {
                cancellationToken.ThrowIfCancellationRequested();
                context.ReportDiagnostics(syntaxTree.GetDiagnostics());
            }
        }

        if (
            stage == CompilationStage.Declare
            || (stage > CompilationStage.Declare && includeEarlierStages)
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            context.ReportDiagnostics(
                GetSourceDeclarationDiagnostics(
                    symbolFilter: symbolFilter,
                    cancellationToken: cancellationToken
                )
            );
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (
            stage != CompilationStage.Compile
            && (stage <= CompilationStage.Compile || !includeEarlierStages)
        )
            return;

        cancellationToken.ThrowIfCancellationRequested();
        GetDiagnosticsForAllFunctionBodies(context, cancellationToken);
    }

    private ImmutableArray<Diagnostic> GetSourceDeclarationDiagnostics(
        SyntaxTree? syntaxTree = null,
        TextSpan? filterSpanWithinTree = null,
        Func<
            IEnumerable<Diagnostic>,
            SyntaxTree,
            TextSpan?,
            IEnumerable<Diagnostic>
        >? locationFilterOpt = null,
        Predicate<Symbol>? symbolFilter = null,
        CancellationToken cancellationToken = default
    )
    {
        SourceLocation? location = null;
        if (syntaxTree is not null)
        {
            var root = syntaxTree.Root;
            location = filterSpanWithinTree is not null
                ? new SourceLocation(syntaxTree, filterSpanWithinTree.Value)
                : new SourceLocation(root);
        }

        Assembly.ForceComplete(location, symbolFilter, cancellationToken);

        var result = DeclarationDiagnostics.AsEnumerable();
        if (locationFilterOpt is null)
            return [.. result];

        Debug.Assert(syntaxTree is not null);
        result = locationFilterOpt(result, syntaxTree, filterSpanWithinTree);

        return [.. result];
    }

    private void GetDiagnosticsForAllFunctionBodies(
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        FunctionCompiler.CompileFunctions(this, null, context, cancellationToken);
    }

    internal DiagnosticBag DeclarationDiagnostics { get; } = [];

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
