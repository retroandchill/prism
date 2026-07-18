// @file SyntaxModel.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Prism.SyntaxGenerator.Models.Resolved;
using Prism.SyntaxGenerator.Models.Spec;
using ZLinq;

namespace Prism.SyntaxGenerator.Resolution;

public sealed class SyntaxModelBuilder
{
    private const int TriviaStart = 100;
    private const int KeywordsStart = 200;
    private const int PunctuationsStart = 300;
    private const int OtherTokensStart = 400;
    private const int SyntaxNodeStart = 1000;
    private const int SyntaxNodeStep = 1000;

    private readonly OrderedDictionary<string, SyntaxKind> _syntaxKinds = new();
    private readonly List<SyntaxGroup> _syntaxKindGroups = [];
    private readonly OrderedDictionary<string, SyntaxTrivia> _trivia = new();
    private readonly OrderedDictionary<string, SyntaxToken> _tokens = new();
    private readonly OrderedDictionary<string, SyntaxModule> _modules = new();
    private readonly Dictionary<string, SyntaxNode> _nodes = new();
    private readonly Dictionary<SyntaxNode, NodeDefinition> _nodeDefinitions = new();
    private readonly Dictionary<SyntaxNode, Dictionary<string, SyntaxProperty>> _nodeProperties =
        new();
    private readonly Dictionary<SyntaxProduction, ProductionDefinition> _productionDefinitions =
        new();

    public SyntaxModel Build(SyntaxSpecification spec)
    {
        EnsureCapacity(spec);
        LoadTrivia(spec);
        LoadTokens(spec);
        LoadNodes(spec);
        ResolveInheritance();
        ResolveProperties();
        ResolveProductions();
        ResolveOverrides();
        ResolveDependencies();
        ResolveNodeSyntaxKinds();
        return new SyntaxModel(
            [.. _syntaxKinds.Values],
            [.. _syntaxKindGroups],
            [.. _trivia.Values],
            [.. _tokens.Values],
            [.. _modules.Values]
        );
    }

    private void EnsureCapacity(SyntaxSpecification spec)
    {
        _syntaxKinds.EnsureCapacity(
            spec.Trivia.Length
                + spec.Tokens.Length
                + spec.Modules.AsValueEnumerable()
                    .SelectMany(m => m.Nodes)
                    .Select(m => m.Productions.Length + 1)
                    .Sum()
        );
        // Modules + 3 Token kind groups + trivia
        _syntaxKindGroups.EnsureCapacity(spec.Modules.Length + 4);
        _trivia.EnsureCapacity(spec.Trivia.Length);
        _tokens.EnsureCapacity(spec.Tokens.Length);
        _modules.EnsureCapacity(spec.Modules.Length);
        _nodes.EnsureCapacity(spec.Modules.AsValueEnumerable().Select(m => m.Nodes.Length).Sum());
    }

    private void LoadTrivia(SyntaxSpecification spec)
    {
        var kinds = new SyntaxKind[spec.Trivia.Length];
        foreach (var (i, definition) in spec.Trivia.AsValueEnumerable().Index())
        {
            var trivia = new SyntaxTrivia(definition.Name, definition.DisplayName);
            trivia.Kind = new SyntaxKind(definition.Name, TriviaStart + i, trivia);
            kinds[i] = trivia.Kind;
            _syntaxKinds.Add(trivia.Kind.Name, trivia.Kind);
            _trivia.Add(trivia.Kind.Name, trivia);
        }

        _syntaxKindGroups.Add(
            new SyntaxGroup(
                "Trivia",
                SyntaxGroupKind.Trivia,
                ImmutableCollectionsMarshal.AsImmutableArray(kinds)
            )
        );
    }

    private void LoadTokens(SyntaxSpecification spec)
    {
        var kinds = new SyntaxKind[spec.Tokens.Keywords.Length];
        foreach (var (i, definition) in spec.Tokens.Keywords.AsValueEnumerable().Index())
        {
            var token = new SyntaxToken(definition.Name, TokenCategory.Keyword)
            {
                Flags = definition.Contextual ? TokenFlags.Contextual : TokenFlags.None,
                Text = definition.Text,
            };
            token.Kind = new SyntaxKind(token.Name, KeywordsStart + i, token);

            kinds[i] = token.Kind;
            _syntaxKinds.Add(token.Kind.Name, token.Kind);
            _tokens.Add(token.Kind.Name, token);
        }
        _syntaxKindGroups.Add(
            new SyntaxGroup(
                "Keywords",
                SyntaxGroupKind.Token,
                ImmutableCollectionsMarshal.AsImmutableArray(kinds)
            )
        );

        kinds = new SyntaxKind[spec.Tokens.Punctuations.Length];
        foreach (var (i, definition) in spec.Tokens.Punctuations.AsValueEnumerable().Index())
        {
            var token = new SyntaxToken(definition.Name, TokenCategory.Punctuation)
            {
                Text = definition.Text,
            };
            token.Kind = new SyntaxKind(token.Name, PunctuationsStart + i, token);

            kinds[i] = token.Kind;
            _syntaxKinds.Add(token.Kind.Name, token.Kind);
            _tokens.Add(token.Kind.Name, token);
        }
        _syntaxKindGroups.Add(
            new SyntaxGroup(
                "Punctuations",
                SyntaxGroupKind.Token,
                ImmutableCollectionsMarshal.AsImmutableArray(kinds)
            )
        );

        kinds = new SyntaxKind[spec.Tokens.Other.Length];
        foreach (var (i, definition) in spec.Tokens.Other.AsValueEnumerable().Index())
        {
            var token = new SyntaxToken(definition.Name, TokenCategory.Other)
            {
                DisplayName = definition.DisplayName,
            };
            token.Kind = new SyntaxKind(token.Name, OtherTokensStart + i, token);

            _syntaxKinds.Add(token.Kind.Name, token.Kind);
            _tokens.Add(token.Kind.Name, token);
        }
        _syntaxKindGroups.Add(
            new SyntaxGroup(
                "OtherTokens",
                SyntaxGroupKind.Token,
                ImmutableCollectionsMarshal.AsImmutableArray(kinds)
            )
        );
    }

    private void LoadNodes(SyntaxSpecification spec)
    {
        foreach (var module in spec.Modules)
        {
            var resolvedModule = new SyntaxModule { Name = module.Name };
            _modules.Add(module.Name, resolvedModule);
            resolvedModule.EnsureCapacity(module.Nodes.Length);
            foreach (var definition in module.Nodes)
            {
                var node = new SyntaxNode(resolvedModule, definition.Name);
                _nodeDefinitions.Add(node, definition);
                resolvedModule.AddNode(node);
                _nodes.Add(definition.Name, node);
            }
        }
    }

    private void ResolveInheritance()
    {
        foreach (var node in _nodes.Values)
        {
            var definition = _nodeDefinitions[node];
            if (definition.Base is null)
                continue;

            var baseNode = _nodes[definition.Base];

            node.Base = baseNode;
            baseNode.AddDerivedType(node);
        }
    }

    private void ResolveProperties()
    {
        foreach (var node in _nodes.Values)
        {
            var definition = _nodeDefinitions[node];
            var propertiesDict = new Dictionary<string, SyntaxProperty>();
            propertiesDict.EnsureCapacity(definition.Fields.Length);
            foreach (var child in definition.Fields)
            {
                var prop = new SyntaxProperty(child.Name, ResolveType(child.Type), child.Shape);
                propertiesDict.Add(child.Name, prop);
                node.AddProperty(prop);
            }
            _nodeProperties.Add(node, propertiesDict);
        }
    }

    private SyntaxTypeReference ResolveType(string name)
    {
        return name is "Node" or "Token" or "Trivia"
            ? new SyntaxTypeReference(name)
            : new SyntaxTypeReference(name, _nodes[name]);
    }

    private void ResolveProductions()
    {
        foreach (var node in _nodes.Values)
        {
            var propertiesDict = _nodeProperties[node];
            foreach (var production in _nodeDefinitions[node].Productions)
            {
                var constraints = new SyntaxProductionConstraint[production.Constraints.Length];
                foreach (var (i, constraint) in production.Constraints.AsValueEnumerable().Index())
                {
                    var property = propertiesDict[constraint.Property];
                    switch (constraint)
                    {
                        case TokenConstraint tokenConstraint:
                        {
                            var token = _tokens[tokenConstraint.Token];
                            constraints[i] = new SyntaxTokenConstraint(property, token);
                            break;
                        }
                        case PropertyPresenceConstraint presenceConstraint:
                            constraints[i] = new SyntaxPropertyPresenceConstraint(
                                property,
                                presenceConstraint.Required
                            );
                            break;
                    }
                }

                var resolvedProduction = new SyntaxProduction(
                    production.Name,
                    node,
                    ImmutableCollectionsMarshal.AsImmutableArray(constraints)
                );
                node.AddProduction(resolvedProduction);
                _productionDefinitions.Add(resolvedProduction, production);
            }
        }
    }

    private void ResolveOverrides()
    {
        foreach (var node in _nodes.Values)
        {
            if (node.Base is null)
                continue;
            var parentChildren = CollectParentProperties(node);

            foreach (
                var child in node
                    .Properties.AsValueEnumerable()
                    .Where(child => parentChildren.Contains(child.Name))
            )
            {
                child.IsOverride = true;
            }
        }
    }

    private static HashSet<string> CollectParentProperties(SyntaxNode node)
    {
        var result = new HashSet<string>();
        var @base = node.Base;
        while (@base is not null)
        {
            result.UnionWith(@base.Properties.Select(x => x.Name));
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
                    module.AddDependency(node.Base.Module);
                }

                foreach (var child in node.Properties)
                {
                    if (child.Type.Definition is null)
                        continue;

                    if (child.Type.Definition.Module != module)
                    {
                        module.AddDependency(child.Type.Definition.Module);
                        module.AddForwardDeclaration(child.Type.Definition);
                    }
                    else if (!seen.Contains(child.Type.Definition.Name))
                    {
                        module.AddForwardDeclaration(child.Type.Definition);
                    }
                }
            }
        }
    }

    private void ResolveNodeSyntaxKinds()
    {
        foreach (var (i, module) in _modules.Values.AsValueEnumerable().Index())
        {
            var kinds = ImmutableArray.CreateBuilder<SyntaxKind>();
            var nextValue = SyntaxNodeStart + SyntaxNodeStep * i;
            foreach (var node in module.Nodes.AsValueEnumerable().Where(x => !x.IsAbstract))
            {
                var productionUsed = false;
                foreach (var production in node.Productions.AsValueEnumerable())
                {
                    var definition = _productionDefinitions[production];
                    if (definition.SyntaxKind is null)
                        continue;

                    production.Kind = new SyntaxKind(production.Name, nextValue++, production);
                    kinds.Add(production.Kind);
                    _syntaxKinds.Add(production.Kind.Name, production.Kind);
                    productionUsed = true;
                }

                if (productionUsed)
                    continue;
                node.Kind = new SyntaxKind(node.Name, nextValue++, node);
                _syntaxKinds.Add(node.Kind.Name, node.Kind);
            }

            if (kinds.Count > 0)
            {
                _syntaxKindGroups.Add(
                    new SyntaxGroup(module.Name, SyntaxGroupKind.Node, kinds.DrainToImmutable())
                );
            }
        }
    }
}
