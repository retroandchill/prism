using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Binding;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Symbols.Error;
using Prism.Core.Syntax;

namespace Prism.Core.Compiling;

internal sealed class CompilationCache(Compilation compilation)
{
    private readonly record struct SymbolLookupKey(Symbol? Container, string Name);

    private readonly record struct ArrayLookupKey(TypeSymbol ElementType, ulong? Size);

    private readonly record struct ReferenceLookupKey(TypeSymbol ReferencedType, bool IsMutable);

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
    private readonly ConcurrentDictionary<ArrayLookupKey, ArrayTypeSymbol> _arrayTypes = new();
    private readonly ConcurrentDictionary<ReferenceLookupKey, ReferenceTypeSymbol> _referenceTypes =
        new();
    private readonly ConcurrentDictionary<SymbolLookupKey, NamedTypeSymbol> _errorTypes = new();
    private readonly ConcurrentDictionary<SymbolLookupKey, NamespaceSymbol> _errorNamespaces =
        new();

    private ImmutableArray<VariableSymbol> _topLevelVariables;
    private ImmutableArray<FunctionSymbol> _topLevelFunctions;

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

    public ArrayTypeSymbol CreateArrayTypeSymbol(TypeSymbol elementType, ulong? size)
    {
        return _arrayTypes.GetOrAdd(
            new ArrayLookupKey(elementType, size),
            static k => new ArrayTypeSymbol(k.ElementType, k.Size)
        );
    }

    public ReferenceTypeSymbol CreateReferenceTypeSymbol(TypeSymbol elementType, bool isMutable)
    {
        return _referenceTypes.GetOrAdd(
            new ReferenceLookupKey(elementType, isMutable),
            static k => new ReferenceTypeSymbol(k.ReferencedType, k.IsMutable)
        );
    }

    public NamedTypeSymbol CreateErrorTypeSymbol(Symbol? container, string name)
    {
        var key = new SymbolLookupKey(container, name);
        return _errorTypes.GetOrAdd(key, static k => new ErrorTypeSymbol(k.Name, k.Container));
    }

    public NamespaceSymbol CreateErrorNamespaceSymbol(NamespaceSymbol? container, string name)
    {
        var key = new SymbolLookupKey(container, name);
        return _errorNamespaces.GetOrAdd(
            key,
            static k => new ErrorNamespaceSymbol(k.Name, k.Container)
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

    public ImmutableArray<VariableSymbol> GetGlobalVariables()
    {
        if (!_topLevelVariables.IsDefault)
            return _topLevelVariables;

        var variables = ImmutableArray.CreateBuilder<VariableSymbol>();
        CollectGlobalVariables(compilation.Assembly.GlobalNamespace, variables);
        ImmutableInterlocked.InterlockedCompareExchange(
            ref _topLevelVariables,
            variables.DrainToImmutable(),
            default
        );
        return _topLevelVariables;
    }

    private static void CollectGlobalVariables(
        NamespaceSymbol namespaceSymbol,
        ImmutableArray<VariableSymbol>.Builder variables
    )
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            switch (member)
            {
                case VariableSymbol variable:
                    variables.Add(variable);
                    break;
                case NamespaceSymbol nestedNamespace:
                    CollectGlobalVariables(nestedNamespace, variables);
                    break;
            }
        }
    }

    public ImmutableArray<FunctionSymbol> GetGlobalFunctions()
    {
        if (!_topLevelFunctions.IsDefault)
            return _topLevelFunctions;

        var functions = ImmutableArray.CreateBuilder<FunctionSymbol>();
        CollectGlobalFunctions(compilation.Assembly.GlobalNamespace, functions);
        ImmutableInterlocked.InterlockedCompareExchange(
            ref _topLevelFunctions,
            functions.DrainToImmutable(),
            default
        );
        return _topLevelFunctions;
    }

    private static void CollectGlobalFunctions(
        NamespaceSymbol namespaceSymbol,
        ImmutableArray<FunctionSymbol>.Builder functions
    )
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            switch (member)
            {
                case FunctionSymbol function:
                    functions.Add(function);
                    break;
                case NamespaceSymbol nestedNamespace:
                    CollectGlobalFunctions(nestedNamespace, functions);
                    break;
            }
        }
    }
}
