// @file MemberContainerDefinitionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;

namespace Prism.Metadata.Builders;

public closed class MemberContainerDefinitionBuilder(
    string name,
    AssemblyDefinitionBuilder assembly
)
{
    private readonly List<TypeDefinitionBuilder> _types = [];
    private readonly List<TypeAliasBuilder> _typeAliases = [];
    private readonly List<VariableDefinitionBuilder> _variables = [];
    private readonly List<FunctionDefinitionBuilder> _functions = [];

    protected AssemblyDefinitionBuilder Assembly { get; } = assembly;

    public MemberContainerDefHandle Handle => GetHandle();

    public string Name { get; } = name;

    protected abstract MemberContainerDefHandle GetHandle();

    public ReadOnlyCollection<TypeDefinitionBuilder> Types => _types.AsReadOnly();
    public ReadOnlyCollection<TypeAliasBuilder> TypeAliases => _typeAliases.AsReadOnly();
    public ReadOnlyCollection<VariableDefinitionBuilder> Variables => _variables.AsReadOnly();
    public ReadOnlyCollection<FunctionDefinitionBuilder> Functions => _functions.AsReadOnly();

    public TypeDefinitionBuilder AddType(string name)
    {
        var type = Assembly.AddTypeDefinition(name, this);
        _types.Add(type);
        return type;
    }

    public TypeAliasBuilder AddTypeAlias(string name)
    {
        var typeAlias = Assembly.AddTypeAlias(name, this);
        _typeAliases.Add(typeAlias);
        return typeAlias;
    }

    public VariableDefinitionBuilder AddVariable(string name, TypeDescriptorBuilder type)
    {
        var variable = Assembly.AddVariable(name, type, this);
        _variables.Add(variable);
        return variable;
    }

    public FunctionDefinitionBuilder AddFunction(string name, TypeDescriptorBuilder returnType)
    {
        var function = Assembly.AddFunction(name, returnType, this);
        _functions.Add(function);
        return function;
    }
}
