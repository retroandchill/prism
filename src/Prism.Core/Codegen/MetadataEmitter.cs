// @file MetadataEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Prism.Core.Compiling;
using Prism.Core.Metadata;
using Prism.Core.Semantic;
using Prism.Core.Symbols;

namespace Prism.Core.Codegen;

internal sealed class MetadataEmitter : IDisposable
{
    private readonly Compilation _compilation;
    private readonly MetadataBuilder _builder;

    private readonly Dictionary<AssemblySymbol, AssemblyRefHandle> _assemblyRefs = new(
        ReferenceEqualityComparer.Instance
    );

    private readonly Dictionary<NamespaceSymbol, NamespaceDefHandle> _namespaceDefs = new(
        ReferenceEqualityComparer.Instance
    );
    private readonly Dictionary<NamespaceSymbol, NamespaceRefHandle> _namespaceRefs = new(
        ReferenceEqualityComparer.Instance
    );
    private readonly Dictionary<TypeSymbol, TypeHandle> _types = new(
        ReferenceEqualityComparer.Instance
    );

    private readonly Dictionary<FunctionSymbol, FunctionHandle> _functions = new(
        ReferenceEqualityComparer.Instance
    );

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

        _builder = new MetadataBuilder(assemblyIdentity, targetInfo);
    }

    public async ValueTask EmitAsync(string fileName, CancellationToken cancellationToken)
    {
        AddSymbols(cancellationToken);

        await using var file = File.Create(fileName);
        await _builder.WriteAsync(file, cancellationToken);
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

    private AssemblyRefHandle GetOrAddAssemblyRef(AssemblySymbol assemblySymbol)
    {
        Debug.Assert(assemblySymbol != _compilation.Assembly);
        if (_assemblyRefs.TryGetValue(assemblySymbol, out var handle))
        {
            return handle;
        }

        handle = _builder.AddReferencedAssembly(assemblySymbol.Identity);
        _assemblyRefs[assemblySymbol] = handle;
        return handle;
    }

    private void AddNamespaceDef(NamespaceSymbol namespaceSymbol)
    {
        var name = _builder.GetOrAddString(namespaceSymbol.Name);
        var parent = NamespaceDefHandle.None;
        if (namespaceSymbol.ContainingNamespace is { IsGlobal: false } containing)
        {
            parent = _namespaceDefs[containing];
        }

        var handle = _builder.AddNamespaceDef(new NamespaceDefRow(name, parent));
        _namespaceDefs[namespaceSymbol] = handle;
    }

    private NamespaceRefHandle GetOrAddNamespaceRef(NamespaceSymbol namespaceSymbol)
    {
        var compilationNamespace = _compilation.GetCompilationNamespace(namespaceSymbol);
        Debug.Assert(compilationNamespace is not null);
        if (_namespaceRefs.TryGetValue(compilationNamespace, out var handle))
        {
            return handle;
        }

        var parent = NamespaceRefHandle.None;
        if (compilationNamespace.ContainingNamespace is { IsGlobal: false } containing)
        {
            parent = GetOrAddNamespaceRef(containing);
        }

        var name = _builder.GetOrAddString(compilationNamespace.Name);
        handle = _builder.AddNamespaceRef(new NamespaceRefRow(name, parent));
        _namespaceRefs[compilationNamespace] = handle;
        return handle;
    }

    private TypeDefHandle AddTypeDef(NamedTypeSymbol namedTypeSymbol)
    {
        if (_types.TryGetValue(namedTypeSymbol, out var typeHandle))
        {
            return (TypeDefHandle)typeHandle;
        }

        var parent = GetContainerDefHandle(namedTypeSymbol);
        var name = _builder.GetOrAddString(namedTypeSymbol.Name);
        var attributes = NamedTypeAttributes.Create(
            namedTypeSymbol.Kind,
            namedTypeSymbol.DeclaredVisibility
        );
        var row = new TypeDefinitionRow(attributes, name, parent);
        var handle = _builder.AddTypeDef(row);
        _types.Add(namedTypeSymbol, handle);
        return handle;
    }

    private MemberContainerDefHandle GetContainerDefHandle(Symbol symbol)
    {
        if (symbol.ContainingType is { } containingType)
        {
            return (TypeDefHandle)_types[containingType];
        }

        if (symbol.ContainingNamespace is { IsGlobal: false } containing)
        {
            return _namespaceDefs[containing];
        }

        return MemberContainerDefHandle.None;
    }

    private TypeHandle GetOrAddType(TypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
            return TypeHandle.None;

        if (_types.TryGetValue(typeSymbol, out var handle))
            return handle;

        return typeSymbol switch
        {
            NamedTypeSymbol namedTypeSymbol => AddNamedType(namedTypeSymbol),
            ArrayTypeSymbol arrayTypeSymbol => AddArrayType(arrayTypeSymbol),
            NullableTypeSymbol nullableTypeSymbol => AddNullableType(nullableTypeSymbol),
            ReferenceTypeSymbol referenceTypeSymbol => AddReferenceType(referenceTypeSymbol),
        };
    }

    private TypeHandle AddNamedType(NamedTypeSymbol namedTypeSymbol)
    {
        var assembly = namedTypeSymbol.ContainingAssembly;
        Debug.Assert(assembly is not null);
        if (assembly == _compilation.Assembly)
            return AddTypeDef(namedTypeSymbol);

        var container = GetContainerRefHandle(namedTypeSymbol);
        var name = _builder.GetOrAddString(namedTypeSymbol.Name);
        return _builder.AddTypeRef(new TypeReferenceRow(name, container));
    }

    private TypeHandle AddArrayType(ArrayTypeSymbol arrayTypeSymbol)
    {
        var containedType = GetOrAddType(arrayTypeSymbol.ElementType);
        return _builder.AddTypeSpec(new ArrayTypeRow(containedType, arrayTypeSymbol.Size));
    }

    private TypeHandle AddNullableType(NullableTypeSymbol nullableTypeSymbol)
    {
        var containedType = GetOrAddType(nullableTypeSymbol.ElementType);
        return _builder.AddTypeSpec(new NullableTypeRow(containedType));
    }

    private TypeHandle AddReferenceType(ReferenceTypeSymbol referenceTypeSymbol)
    {
        var containedType = GetOrAddType(referenceTypeSymbol.ReferencedType);
        return _builder.AddTypeSpec(
            new ReferenceTypeRow(containedType, referenceTypeSymbol.IsMutable)
        );
    }

    private MemberContainerRefHandle GetContainerRefHandle(Symbol symbol)
    {
        if (symbol.ContainingType is { } containingType)
        {
            return (TypeRefHandle)GetOrAddType(containingType);
        }

        if (symbol.ContainingNamespace is { IsGlobal: false } containing)
        {
            return GetOrAddNamespaceRef(containing);
        }

        if (symbol.ContainingAssembly is { } containingAssembly)
        {
            return GetOrAddAssemblyRef(containingAssembly);
        }

        return MemberContainerRefHandle.None;
    }

    private void AddVariable(VariableSymbol variableSymbol)
    {
        var attributes = VariableAttributes.Create(
            variableSymbol.Kind,
            variableSymbol.DeclaredVisibility
        );
        var name = _builder.GetOrAddString(variableSymbol.MetadataName);
        var type = GetOrAddType(variableSymbol.Type);
        var container = GetContainerDefHandle(variableSymbol);
        ConstantHandle constant;
        if (variableSymbol.ConstantValue is { } constantValue)
        {
            constant = _builder.GetOrAddConstant(in constantValue);
        }
        else
        {
            constant = ConstantHandle.None;
        }
        _builder.AddVariable(new VariableRow(attributes, name, type, container, constant));
    }

    private void AddFunction(FunctionSymbol functionSymbol)
    {
        var attributes = FunctionAttributes.Create(
            functionSymbol.DeclaredVisibility,
            functionSymbol.IsExtern,
            functionSymbol.AbiKind
        );
        var name = _builder.GetOrAddString(functionSymbol.MetadataName);
        var returnType = GetOrAddType(functionSymbol.ReturnType);
        var receiverType = GetOrAddType(functionSymbol.ReceiverType);
        var container = GetContainerDefHandle(functionSymbol);
        var handle = _builder.AddFunction(
            new FunctionRow(attributes, name, returnType, receiverType, container)
        );
        _functions.Add(functionSymbol, handle);
    }

    private void AddParameter(ParameterSymbol parameterSymbol)
    {
        var attributes = ParameterAttributes.Create();
        var name = _builder.GetOrAddString(parameterSymbol.MetadataName);
        var type = GetOrAddType(parameterSymbol.Type);
        FunctionOrAttributeHandle owner;
        if (parameterSymbol.ContainingFunction is { } func)
        {
            owner = _functions[func];
        }
        else
        {
            var containingType = parameterSymbol.ContainingType;
            Debug.Assert(containingType is not null);
            owner = (TypeDefHandle)GetOrAddType(containingType);
        }

        var defaultValue = parameterSymbol.DefaultValue switch
        {
            null => ParameterDefaultHandle.None,
            ConstantParameterDefault(var value, _) => _builder.GetOrAddConstant(in value),
        };

        _builder.AddParameter(new ParameterRow(attributes, name, type, owner, defaultValue));
    }

    public void Dispose()
    {
        _builder.Dispose();
    }
}
