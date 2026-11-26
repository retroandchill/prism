using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Prism.Abi;
using Prism.BoundTree;
using Prism.Compiler.Binding;
using Prism.Compiler.Semantic;
using Prism.Compiler.Symbols;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Semantic.Symbols.Error;
using Prism.Syntax;

namespace Prism.Compiler;

internal sealed class CompilationCache(Compilation compilation)
{
    private readonly record struct SymbolLookupKey(Symbol? Container, string Name);

    private readonly ConcurrentDictionary<
        NamespaceSymbol,
        NamespaceSymbol?
    > _compilationNamespaces = new(ReferenceEqualityComparer.Instance);
    private readonly ConcurrentDictionary<SyntaxTree, BinderFactory> _binderFactories = new(
        ReferenceEqualityComparer.Instance
    );
    private readonly ConcurrentDictionary<SyntaxTree, SemanticModel> _semanticModels = new(
        ReferenceEqualityComparer.Instance
    );
    private readonly ConcurrentDictionary<SymbolLookupKey, NamedTypeSymbol> _errorTypes = new();
    private readonly ConcurrentDictionary<SymbolLookupKey, NamespaceSymbol> _errorNamespaces =
        new();
    private readonly ConcurrentDictionary<SymbolLookupKey, VariableSymbol> _errorVariables = new();

    private readonly ConcurrentDictionary<
        VariableSymbol,
        BoundVariableInitializer
    > _variableInitializers = new(ReferenceEqualityComparer.Instance);
    private readonly ConcurrentDictionary<FunctionSymbol, BoundFunctionBody> _functionBodies = new(
        ReferenceEqualityComparer.Instance
    );

    private readonly ConcurrentDictionary<FunctionSymbol, FunctionAbi> _functionAbis = new(
        ReferenceEqualityComparer.Instance
    );

    private readonly ConcurrentDictionary<
        WellKnownAttribute,
        NamedTypeSymbol?
    > _wellKnownAttributes = new();

    private readonly ConcurrentDictionary<FunctionSymbol, IntrinsicFunction> _intrinsicFunctions =
        new();

    private ImmutableArray<VariableSymbol> _topLevelVariables;

    public NamespaceSymbol? GetCompilationNamespace(NamespaceSymbol symbol)
    {
        if (
            symbol.NamespaceKind == NamespaceKind.Compilation
            && symbol.ContainingCompilation == compilation
        )
            return symbol;

        return _compilationNamespaces.GetOrAdd(
            symbol,
            static (s, self) => self.ComputeCompilationNamespace(s),
            this
        );
    }

    private NamespaceSymbol? ComputeCompilationNamespace(NamespaceSymbol symbol)
    {
        var containingNamespace = symbol.ContainingNamespace;
        if (containingNamespace is null)
            return compilation.GlobalNamespace;

        var current = GetCompilationNamespace(containingNamespace);
        if (current is null)
            return null;

        var found = current.GetNestedNamespace(symbol.Name);
        Debug.Assert(found is not null);
        return found;
    }

    public SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
    {
        return _semanticModels.GetOrAdd(
            syntaxTree,
            static (t, c) => new SemanticModel(c, t),
            compilation
        );
    }

    public NamedTypeSymbol CreateErrorTypeSymbol(Symbol? container, string name)
    {
        var key = new SymbolLookupKey(container, name);
        return _errorTypes.GetOrAdd(key, static k =>
            new ErrorTypeSymbol(k.Name, k.Container).PublicSymbol
        );
    }

    public NamespaceSymbol CreateErrorNamespaceSymbol(NamespaceSymbol? container, string name)
    {
        var key = new SymbolLookupKey(container, name);
        return _errorNamespaces.GetOrAdd(key, static k =>
            new ErrorNamespaceSymbol(k.Name, k.Container).PublicSymbol
        );
    }

    internal VariableSymbol CreateErrorFieldSymbol(TypeSymbol container, string name)
    {
        var key = new SymbolLookupKey(container, name);
        return _errorVariables.GetOrAdd(key, static k =>
            new ErrorFieldSymbol(k.Name, Unsafe.As<TypeSymbol>(k.Container)!).PublicSymbol
        );
    }

    public NamespaceSymbol StdNamespace
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.Exchange(ref field, compilation.FindStdNamespace());
            return field;
        }
    }

    public NamedTypeSymbol? GetWellKnownAttribute(WellKnownAttribute attribute)
    {
        return _wellKnownAttributes.GetOrAdd(
            attribute,
            static (a, c) => c.FindWellKnownAttribute(a),
            compilation
        );
    }

    public IntrinsicFunction GetIntrinsicFunction(FunctionSymbol function)
    {
        return _intrinsicFunctions.GetOrAdd(
            function,
            static (f, c) => c.ComputeIntrinsicKind(f),
            compilation
        );
    }

    public BinderFactory GetBinderFactory(SyntaxTree syntaxTree)
    {
        return _binderFactories.GetOrAdd(
            syntaxTree,
            static (t, c) => new BinderFactory(c, t),
            compilation
        );
    }

    public BoundVariableInitializer GetBoundInitializer(
        VariableSymbol variable,
        CancellationToken cancellationToken
    )
    {
        return _variableInitializers.GetOrAdd(
            variable,
            static (v, t) =>
                CompilerDriver.BindVariableInitializer(t.compilation, v, t.cancellationToken),
            (compilation, cancellationToken)
        );
    }

    public BoundFunctionBody GetBoundFunctionBody(
        FunctionSymbol function,
        CancellationToken cancellationToken
    )
    {
        return _functionBodies.GetOrAdd(
            function,
            static (f, t) => CompilerDriver.BindFunctionBody(t.compilation, f, t.cancellationToken),
            (compilation, cancellationToken)
        );
    }

    public FunctionAbi GetFunctionAbi(IAbiClassifier classifier, FunctionSymbol functionSymbol)
    {
        return _functionAbis.GetOrAdd(functionSymbol, static (f, c) => c.Classify(f), classifier);
    }

    public ImmutableArray<VariableSymbol> GetGlobalVariables()
    {
        if (!_topLevelVariables.IsDefault)
            return _topLevelVariables;

        var variables = ImmutableArray.CreateBuilder<VariableSymbol>();
        CollectGlobalVariables(compilation.Assembly.GlobalNamespace, variables);
        variables.Sort((a, b) => a.GetLexicalSortKey().CompareTo(b.GetLexicalSortKey()));
        ImmutableInterlocked.InterlockedCompareExchange(
            ref _topLevelVariables,
            variables.DrainToImmutable(),
            default
        );
        return _topLevelVariables;
    }

    private static void CollectGlobalVariables(
        MemberContainerSymbol namespaceSymbol,
        ImmutableArray<VariableSymbol>.Builder variables
    )
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            switch (member)
            {
                case VariableSymbol { IsStaticStorage: true } variable:
                    variables.Add(variable);
                    break;
                case MemberContainerSymbol nestedNamespace:
                    CollectGlobalVariables(nestedNamespace, variables);
                    break;
            }
        }
    }
}
