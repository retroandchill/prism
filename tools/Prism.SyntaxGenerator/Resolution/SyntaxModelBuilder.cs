// @file SyntaxModel.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.SyntaxGenerator.Models;

namespace Prism.SyntaxGenerator.Resolution;

public sealed class SyntaxModelBuilder
{
    private readonly OrderedDictionary<string, ResolvedModule> _modules = new();
    private readonly Dictionary<string, SyntaxNode> _nodes = new();

    public ResolvedSyntaxModel Build(SyntaxSpecification spec)
    {
        LoadNodes(spec);
        ResolveInheritance();
        ResolveChildren();
        ResolveOverrides();
        ResolveDependencies();
        return new ResolvedSyntaxModel(spec.Trivia, spec.Tokens, _modules, _nodes);
    }

    private void LoadNodes(SyntaxSpecification spec)
    {
        foreach (var module in spec.Modules)
        {
            var resolvedModule = new ResolvedModule { Name = module.Name };
            _modules.Add(module.Name, resolvedModule);
            foreach (var definition in module.Nodes)
            {
                var node = new SyntaxNode { Definition = definition, Module = resolvedModule };
                resolvedModule.Nodes.Add(node);
                _nodes.Add(definition.Name, node);
            }
        }
    }

    private void ResolveInheritance()
    {
        foreach (var node in _nodes.Values)
        {
            if (node.Definition.Base is null)
                continue;

            var baseNode = _nodes[node.Definition.Base];

            node.Base = baseNode;
            baseNode.DerivedTypes.Add(node);
        }
    }

    private void ResolveChildren()
    {
        foreach (var node in _nodes.Values)
        {
            foreach (var child in node.Definition.Children)
            {
                node.Children.Add(
                    new SyntaxChild
                    {
                        Name = child.Name,
                        Type = ResolveType(child.Type),
                        Shape = child.Shape,
                    }
                );
            }
        }
    }

    private SyntaxTypeReference ResolveType(string name)
    {
        if (name is "Node" or "Token" or "Trivia")
        {
            return new SyntaxTypeReference { Name = name };
        }

        return new SyntaxTypeReference { Name = name, Definition = _nodes[name] };
    }

    private void ResolveOverrides()
    {
        foreach (var node in _nodes.Values)
        {
            if (node.Base is null)
                continue;
            var parentChildren = CollectParentChildren(node);

            foreach (var child in node.Children.Where(child => parentChildren.Contains(child.Name)))
            {
                child.IsOverride = true;
            }
        }
    }

    private HashSet<string> CollectParentChildren(SyntaxNode node)
    {
        var result = new HashSet<string>();
        var @base = node.Base;
        while (@base is not null)
        {
            result.UnionWith(@base.Children.Select(x => x.Name));
            @base = @base.Base;
        }
        return result;
    }

    private void ResolveDependencies()
    {
        foreach (var module in _modules.Values)
        {
            var seen = new HashSet<string>();
            foreach (var node in _nodes.Values)
            {
                seen.Add(node.Name);
                if (node.Base is not null && node.Base.Module != module)
                {
                    module.Dependencies.Add(node.Base.Module);
                }

                foreach (var child in node.Children)
                {
                    if (child.Type.Definition is null)
                        continue;

                    if (child.Type.Definition.Module != module)
                    {
                        module.Dependencies.Add(child.Type.Definition.Module);
                        module.ForwardDeclarations.Add(child.Type.Definition);
                    }
                    else if (!seen.Contains(child.Type.Definition.Name))
                    {
                        module.ForwardDeclarations.Add(child.Type.Definition);
                    }
                }
            }
        }
    }
}
