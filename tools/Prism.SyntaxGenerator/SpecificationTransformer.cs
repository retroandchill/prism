// @file SpecificationTransformer.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using CaseConverter;
using Prism.SyntaxGenerator.Models;

namespace Prism.SyntaxGenerator;

public static class SpecificationTransformer
{
    private const string GreenFragmentName = "syntax.green";
    private const string RedFragmentName = "syntax";

    private readonly record struct ModuleBuilderContext(
        string ModuleName,
        FrozenDictionary<string, string> ModuleTypeAssociations,
        HashSet<string> ReferencedTypes,
        Dictionary<string, HashSet<string>> DeclaredTypes,
        HashSet<string> ForwardDeclarations,
        HashSet<string> Imports
    );

    public static ImmutableArray<GeneratedModule> Transform(SyntaxSpecification spec)
    {
        var moduleTypeAssociations = spec
            .Modules.SelectMany(
                x => x.Nodes.Select(y => y.Name),
                (def, name) => (Module: def.Name, Type: name)
            )
            .ToFrozenDictionary(x => x.Type, x => x.Module);
        return [.. spec.Modules.Select(x => Transform(x, moduleTypeAssociations))];
    }

    private static GeneratedModule Transform(
        ModuleDefinition module,
        FrozenDictionary<string, string> moduleTypeAssociations
    )
    {
        var context = new ModuleBuilderContext(module.Name, moduleTypeAssociations, [], [], [], []);

        var nodes = module.Nodes.Select(x => Transform(x, in context)).ToImmutableArray();
        var moduleName = module.Name.ToSnakeCase();

        return new GeneratedModule
        {
            Name = moduleName,
            GreenName = $"{GreenFragmentName}.{moduleName}",
            RedName = $"{RedFragmentName}.{moduleName}",
            ForwardDeclarations =
            [
                .. context
                    .ForwardDeclarations.OrderBy(x => x)
                    .Select(x => new Dependency(GetGreenName(x), GetRedName(x))),
            ],
            Imports =
            [
                .. context
                    .Imports.OrderBy(x => x)
                    .Select(x => new Dependency(
                        $"{GreenFragmentName}.{x}",
                        $"{RedFragmentName}.{x}"
                    )),
            ],
            Nodes = nodes,
        };
    }

    private static GeneratedNode Transform(NodeDefinition spec, in ModuleBuilderContext context)
    {
        var children = new GeneratedChild[spec.Children.Length];
        var childNames = spec.Base is not null ? context.DeclaredTypes[spec.Base].ToHashSet() : [];
        context.DeclaredTypes[spec.Name] = childNames;
        for (var i = 0; i < spec.Children.Length; i++)
        {
            var child = spec.Children[i];
            children[i] = Transform(child, childNames, in context);
            childNames.Add(child.Name);
        }

        return new GeneratedNode
        {
            Name = spec.Name,
            GreenName = $"Green{spec.Name}",
            GreenBase = GetGreenName(spec.Base),
            RedName = $"{spec.Name}Syntax",
            RedBase = GetRedName(spec.Base),
            IsAbstract = spec.IsAbstract,
            Children = ImmutableCollectionsMarshal.AsImmutableArray(children),
        };
    }

    private static GeneratedChild Transform(
        ChildDefinition child,
        HashSet<string> existingMembers,
        in ModuleBuilderContext context
    )
    {
        context.ReferencedTypes.Add(child.Type);
        var baseName = child.Name.ToSnakeCase();
        var fieldName = $"{baseName}_";

        var associatedModule = context.ModuleTypeAssociations.GetValueOrDefault(child.Type);
        if (associatedModule is not null)
        {
            if (associatedModule != context.ModuleName)
            {
                context.ForwardDeclarations.Add(child.Type);
                context.Imports.Add(associatedModule);
            }
            else if (!context.DeclaredTypes.ContainsKey(child.Type))
            {
                context.ForwardDeclarations.Add(child.Type);
            }
        }

        var greenType = GetGreenName(child.Type);
        var redType = GetRedName(child.Type);

        string greenGetterType;
        string redGetterType;
        if (child.IsOptional)
        {
            greenGetterType = $"Optional<const {greenType}&>";
            redGetterType = $"Optional<const {redType}&>";
        }
        else
        {
            greenGetterType = $"const {greenType}&";
            redGetterType = $"const {redType}&";
        }

        return new GeneratedChild
        {
            Name = baseName,
            FieldName = fieldName,
            GetterName = baseName,
            GreenGetterType = greenGetterType,
            GreenFieldType = $"GreenPtr<{greenType}>",
            RedGetterType = redGetterType,
            RedFieldType = $"RedPtr<{redType}>",
            IsOptional = child.IsOptional,
            IsOverride = existingMembers.Contains(child.Name),
        };
    }

    private static string GetGreenName(string? baseName)
    {
        return baseName is not null ? $"Green{baseName}" : "GreenNode";
    }

    private static string GetRedName(string? baseName)
    {
        return baseName switch
        {
            "Token" => "SyntaxToken",
            "TokenList" => "SyntaxTokenList",
            "Node" or null => "SyntaxNode",
            _ => $"{baseName}Syntax",
        };
    }
}
