// @file MetadataEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Prism.Compiler.Mappers;
using Prism.Compiler.Symbols;
using Prism.Metadata;
using Prism.Semantic;

namespace Prism.Compiler.Codegen;

internal sealed class MetadataEmitter
{
    private readonly Compilation _compilation;
    private readonly AssemblyDefinitionBuilder _builder;

    private readonly Dictionary<AssemblySymbol, AssemblyReference> _assemblyRefs = [];
    private readonly Dictionary<NamespaceSymbol, NamespaceDefinitionBuilder> _namespaceDefs = [];
    private readonly Dictionary<NamespaceSymbol, NamespaceReference> _namespaceRefs = [];
    private readonly Dictionary<TypeSymbol, TypeDefinitionBuilder> _typeDefs = [];
    private readonly Dictionary<TypeSymbol, TypeReference> _typeRefs = [];
    private readonly Dictionary<TypeSymbol, TypeSpecBuilder> _typeSpecs = [];

    private readonly Dictionary<FunctionSymbol, FunctionDefinitionBuilder> _functions = [];

    private readonly struct TrackedSymbols()
    {
        public List<NamespaceSymbol> Namespaces { get; init; } = [];
        public List<NamedTypeSymbol> Types { get; init; } = [];
        public List<VariableSymbol> Variables { get; init; } = [];
        public List<FunctionSymbol> Functions { get; init; } = [];
        public List<ParameterSymbol> Parameters { get; init; } = [];
    }

    public MetadataEmitter(Compilation compilation)
    {
        _compilation = compilation;
        var assembly = compilation.Assembly;
        var assemblyIdentity = assembly.Identity;
        var targetInfo = new TargetInfo(
            compilation.Settings.Architecture,
            compilation.Settings.OperatingSystem,
            compilation.Settings.Toolchain
        );

        _builder = new AssemblyDefinitionBuilder(assemblyIdentity, targetInfo);
    }

    public async ValueTask EmitAsync(string fileName, CancellationToken cancellationToken)
    {
        AddSymbols(cancellationToken);

        await using var file = File.Create(fileName);
        var definition = _builder.Build();
        await definition.WriteAsync(file, cancellationToken);
    }

    private void AddSymbols(CancellationToken cancellationToken)
    {
        var trackedSymbols = new TrackedSymbols();

        foreach (var member in _compilation.GlobalNamespace.GetMembers())
        {
            AddSymbol(member, in trackedSymbols, cancellationToken);
        }

        foreach (var ns in trackedSymbols.Namespaces)
        {
            AddNamespaceDef(ns);
        }

        foreach (var type in trackedSymbols.Types)
        {
            AddTypeDef(type);
        }

        foreach (var variable in trackedSymbols.Variables)
        {
            AddVariable(variable);
        }

        foreach (var function in trackedSymbols.Functions)
        {
            AddFunction(function);
        }

        foreach (var parameter in trackedSymbols.Parameters)
        {
            AddParameter(parameter);
        }
    }

    private static void AddSymbol(
        Symbol symbol,
        in TrackedSymbols symbols,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        switch (symbol)
        {
            case NamespaceSymbol ns:
            {
                symbols.Namespaces.Add(ns);
                foreach (var member in ns.GetMembers())
                {
                    AddSymbol(member, in symbols, cancellationToken);
                }
                break;
            }
            case NamedTypeSymbol type:
            {
                symbols.Types.Add(type);
                foreach (var member in type.GetMembers())
                {
                    AddSymbol(member, in symbols, cancellationToken);
                }

                break;
            }
            case VariableSymbol variable:
                symbols.Variables.Add(variable);
                break;
            case FunctionSymbol function:
                // We don't want to add global constructors to the metadata
                if (function.Kind == FunctionKind.GlobalConstructor)
                    return;

                symbols.Functions.Add(function);
                symbols.Parameters.AddRange(function.Parameters.AsSpan());
                break;
            case ParameterSymbol parameterSymbol:
                symbols.Parameters.Add(parameterSymbol);
                break;
        }
    }

    private AssemblyReference GetOrAddAssemblyRef(AssemblySymbol assemblySymbol)
    {
        Debug.Assert(assemblySymbol != _compilation.Assembly);
        if (_assemblyRefs.TryGetValue(assemblySymbol, out var handle))
        {
            return handle;
        }

        handle = _builder.AddAssemblyReference(assemblySymbol.Identity);
        _assemblyRefs[assemblySymbol] = handle;
        return handle;
    }

    private void AddNamespaceDef(NamespaceSymbol namespaceSymbol)
    {
        var parent = _builder.GlobalNamespace;
        if (namespaceSymbol.ContainingNamespace is { IsGlobal: false } containing)
        {
            parent = _namespaceDefs[containing];
        }

        var handle = parent.AddNestedNamespace(namespaceSymbol.Name);
        _namespaceDefs[namespaceSymbol] = handle;
    }

    private NamespaceReference GetOrAddNamespaceRef(NamespaceSymbol namespaceSymbol)
    {
        var compilationNamespace = _compilation.GetCompilationNamespace(namespaceSymbol);
        Debug.Assert(compilationNamespace is not null);
        if (_namespaceRefs.TryGetValue(compilationNamespace, out var handle))
        {
            return handle;
        }

        NamespaceOrAssemblyReference parent;
        if (compilationNamespace.ContainingNamespace is { IsGlobal: false } containing)
        {
            parent = GetOrAddNamespaceRef(containing);
        }
        else if (compilationNamespace.ContainingAssembly is { } assembly)
        {
            parent = GetOrAddAssemblyRef(assembly);
        }
        else
        {
            throw new InvalidOperationException("Namespace or assembly reference not found.");
        }

        handle = _builder.AddNamespaceReference(namespaceSymbol.Name, parent);
        _namespaceRefs[compilationNamespace] = handle;
        return handle;
    }

    private TypeDefinitionBuilder AddTypeDef(NamedTypeSymbol namedTypeSymbol)
    {
        if (_typeDefs.TryGetValue(namedTypeSymbol, out var typeHandle))
        {
            return typeHandle;
        }

        var parent = GetContainerDef(namedTypeSymbol);
        var type = parent.AddType(namedTypeSymbol.Name);
        type.Kind = namedTypeSymbol.Kind;
        type.Visibility = namedTypeSymbol.DeclaredVisibility;
        type.SpecialType = namedTypeSymbol.SpecialType;
        _typeDefs.Add(namedTypeSymbol, type);
        AddCustomAttributes(namedTypeSymbol, type);
        return type;
    }

    private MemberContainerDefinitionBuilder GetContainerDef(Symbol symbol)
    {
        if (symbol.ContainingType is { } containingType)
        {
            return _typeDefs[containingType];
        }

        if (symbol.ContainingNamespace is { IsGlobal: false } containing)
        {
            return _namespaceDefs[containing];
        }

        return _builder.GlobalNamespace;
    }

    [return: NotNullIfNotNull(nameof(typeSymbol))]
    private TypeDescriptorBuilder? GetOrAddType(TypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
            return null;

        return typeSymbol switch
        {
            NamedTypeSymbol namedTypeSymbol => GetOrAddNamedType(namedTypeSymbol),
            _ => GetOrAddTypeSpec(typeSymbol),
        };
    }

    private NamedTypeBuilder GetOrAddNamedType(NamedTypeSymbol namedTypeSymbol)
    {
        var assembly = namedTypeSymbol.ContainingAssembly;
        Debug.Assert(assembly is not null);
        if (assembly == _compilation.Assembly)
            return AddTypeDef(namedTypeSymbol);

        return GetOrAddTypeReference(namedTypeSymbol);
    }

    private TypeReference GetOrAddTypeReference(NamedTypeSymbol namedTypeSymbol)
    {
        if (_typeRefs.TryGetValue(namedTypeSymbol, out var reference))
            return reference;

        var container = GetContainerRef(namedTypeSymbol);
        reference = _builder.AddTypeReference(namedTypeSymbol.Name, container);
        _typeRefs.TryAdd(namedTypeSymbol, reference);
        return reference;
    }

    private TypeSpecBuilder GetOrAddTypeSpec(TypeSymbol symbol)
    {
        if (_typeSpecs.TryGetValue(symbol, out var typeSpec))
            return typeSpec;

        typeSpec = symbol switch
        {
            ArrayTypeSymbol array => _builder.AddArrayType(
                GetOrAddType(array.ElementType).Value,
                array.Size
            ),
            ReferenceTypeSymbol reference => _builder.AddReferenceType(
                GetOrAddType(reference.ReferencedType).Value,
                reference.IsMutable
            ),
            NullableTypeSymbol nullable => _builder.AddNullableType(
                GetOrAddType(nullable.ElementType).Value
            ),
            NamedTypeSymbol => throw new InvalidOperationException(
                "This should already have been handled above"
            ),
        };
        _typeSpecs.Add(symbol, typeSpec);
        return typeSpec;
    }

    private MemberContainerReference GetContainerRef(Symbol symbol)
    {
        if (symbol.ContainingType is NamedTypeSymbol containingType)
        {
            return GetOrAddTypeReference(containingType);
        }

        if (symbol.ContainingNamespace is { IsGlobal: false } containing)
        {
            return GetOrAddNamespaceRef(containing);
        }

        if (symbol.ContainingAssembly is { } containingAssembly)
        {
            return GetOrAddAssemblyRef(containingAssembly);
        }

        throw new InvalidOperationException($"Could not find container for symbol {symbol}");
    }

    private void AddVariable(VariableSymbol variableSymbol)
    {
        var type = GetOrAddType(variableSymbol.Type).Value;
        var container = GetContainerDef(variableSymbol);
        var variable = container.AddVariable(variableSymbol.Name, type);
        variable.ConstantValue = variableSymbol.ConstantValue;
        AddCustomAttributes(variableSymbol, variable);
    }

    private void AddFunction(FunctionSymbol functionSymbol)
    {
        var returnType = GetOrAddType(functionSymbol.ReturnType).Value;
        var container = GetContainerDef(functionSymbol);
        var function = container.AddFunction(functionSymbol.Name, returnType);
        function.ReceiverType = GetOrAddType(functionSymbol.ReceiverType);
        function.Visibility = functionSymbol.DeclaredVisibility;
        function.IsExtern = function.IsExtern;
        function.AbiKind = functionSymbol.AbiKind;
        function.Kind = functionSymbol.Kind;
        _functions.Add(functionSymbol, function);
        AddCustomAttributes(functionSymbol, function);
    }

    private void AddParameter(ParameterSymbol parameterSymbol)
    {
        var type = GetOrAddType(parameterSymbol.Type).Value;
        FunctionOrAttributeBuilder owner;
        if (parameterSymbol.ContainingFunction is { } func)
        {
            owner = _functions[func];
        }
        else
        {
            var containingType = parameterSymbol.ContainingType;
            Debug.Assert(containingType is not null);
            owner = _typeDefs[containingType];
        }

        var parameter = owner.AddParameter(parameterSymbol.Name, type);
        parameter.DefaultValue = parameterSymbol.DefaultValue?.ToMetadata();
        AddCustomAttributes(parameterSymbol, parameter);
    }

    private void AddCustomAttributes(Symbol symbol, IAttributeHolderDefinitionBuilder owner)
    {
        var attributes = symbol.GetAttributes();
        if (attributes.IsEmpty)
            return;

        foreach (var attribute in attributes)
        {
            var type = GetOrAddNamedType(attribute.Attribute);
            var attributeBuilder = owner.AddCustomAttribute(type);
            foreach (var (_, value) in attribute.Arguments)
            {
                attributeBuilder.Arguments.Add(value);
            }
        }
    }
}
