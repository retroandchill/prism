// @file SyntaxModel.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
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
        var nodeCount = spec.Modules.AsValueEnumerable().Select(m => m.Nodes.Length).Sum();
        _syntaxKinds.EnsureCapacity(spec.Trivia.Length + spec.Tokens.Length + nodeCount);
        // Modules + 3 Token kind groups + trivia
        _syntaxKindGroups.EnsureCapacity(spec.Modules.Length + 4);
        _trivia.EnsureCapacity(spec.Trivia.Length);
        _tokens.EnsureCapacity(spec.Tokens.Length);
        _modules.EnsureCapacity(spec.Modules.Length);
        _nodes.EnsureCapacity(nodeCount);
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
            kinds[i] = token.Kind;

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
        var nextStart = SyntaxNodeStart;
        foreach (var module in spec.Modules)
        {
            var resolvedModule = new SyntaxModule { Name = module.Name };
            _modules.Add(module.Name, resolvedModule);
            if (module.Nodes.IsEmpty)
                continue;

            resolvedModule.EnsureCapacity(module.Nodes.Length);
            var nextValue = nextStart;
            var kinds = new SyntaxKind[module.Nodes.Length];
            foreach (var (i, definition) in module.Nodes.AsValueEnumerable().Index())
            {
                var node = new SyntaxNode(resolvedModule, definition.Name);
                _nodeDefinitions.Add(node, definition);
                resolvedModule.AddNode(node);
                node.Kind = new SyntaxKind(node.Name, nextValue++, node);
                _syntaxKinds.Add(node.Kind.Name, node.Kind);
                kinds[i] = node.Kind;
                _nodes.Add(definition.Name, node);
            }

            var group = new SyntaxGroup(
                module.Name,
                SyntaxGroupKind.Node,
                ImmutableCollectionsMarshal.AsImmutableArray(kinds)
            );
            _syntaxKindGroups.Add(group);
            nextStart += SyntaxNodeStep;
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
            propertiesDict.EnsureCapacity(definition.Properties.Length);
            foreach (var child in definition.Properties)
            {
                AddProperty(child, node, propertiesDict);
            }
            _nodeProperties.Add(node, propertiesDict);
        }
    }

    private void AddProperty(
        PropertyDefinition child,
        SyntaxNode node,
        Dictionary<string, SyntaxProperty> propertiesDict
    )
    {
        switch (child)
        {
            case PropertyItemDefinition item:
            {
                var prop = new SyntaxProperty(node, item.Name, ResolveType(item.Type), item.Shape);
                propertiesDict.Add(item.Name, prop);
                node.AddProperty(prop);
                break;
            }
            case PropertyChoiceDefinition choice:
                foreach (var option in choice.Choices)
                {
                    AddProperty(option, node, propertiesDict);
                }
                break;
            case PropertySequenceDefinition sequence:
                foreach (var element in sequence.Elements)
                {
                    AddProperty(element, node, propertiesDict);
                }
                break;
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
            var definition = _nodeDefinitions[node];
            var propertyLookup = _nodeProperties[node];
            var productions = ExpandMany(
                definition.Properties,
                propertyLookup,
                [new SyntaxProduction(node)]
            );
            node.AddProductions(productions);

            var productionCounts = new Dictionary<SyntaxProperty, int>();
            productions.EnsureCapacity(node.Properties.Count);
            foreach (var property in node.Properties)
            {
                productionCounts[property] = 0;
            }
            foreach (var production in node.Productions)
            {
                foreach (var (property, _) in production.Arguments)
                {
                    productionCounts[property]++;
                }
            }

            foreach (var (property, count) in productionCounts)
            {
                if (count != node.Productions.Count)
                {
                    property.Shape = PropertyShape.Optional;
                }
            }
        }
    }

    private void Expand(
        PropertyDefinition property,
        Dictionary<string, SyntaxProperty> propertyLookup,
        SyntaxProduction production,
        List<SyntaxProduction> next
    )
    {
        switch (property)
        {
            case PropertyItemDefinition item:
            {
                ExpandFromName(item.Name, propertyLookup, production, next);
                break;
            }
            case PropertyReferenceDefinition reference:
                ExpandFromName(reference.Name, propertyLookup, production, next);
                break;
            case PropertySequenceDefinition sequence:
            {
                var branchProductions = ExpandMany(sequence.Elements, propertyLookup, [production]);
                next.AddRange(branchProductions);
                break;
            }
            case PropertyChoiceDefinition choice:
            {
                foreach (var branch in choice.Choices)
                {
                    Expand(branch, propertyLookup, production.Clone(), next);
                }

                break;
            }
            default:
                throw new InvalidOperationException(
                    $"Unexpected property definition type: {property.GetType().FullName}"
                );
        }
    }

    private List<SyntaxProduction> ExpandMany(
        ImmutableArray<PropertyDefinition> properties,
        Dictionary<string, SyntaxProperty> propertyLookup,
        List<SyntaxProduction> productions
    )
    {
        foreach (var property in properties)
        {
            var next = new List<SyntaxProduction>();

            foreach (var production in productions)
            {
                Expand(property, propertyLookup, production, next);
            }

            productions = next;
        }

        return productions;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ExpandFromName(
        string name,
        Dictionary<string, SyntaxProperty> propertyLookup,
        SyntaxProduction production,
        List<SyntaxProduction> next
    )
    {
        var property = propertyLookup[name];
        var argument = new SyntaxProductionArgument(
            property,
            property.Shape == PropertyShape.Optional
        );
        production.Add(argument);
        next.Add(production);
    }

    private void ResolveOverrides()
    {
        foreach (var node in _nodes.Values)
        {
            if (node.Base is null)
                continue;
            var parentChildren = CollectParentProperties(node);

            foreach (var child in node.Properties)
            {
                if (parentChildren.TryGetValue(child.Name, out var parentProperty))
                    child.OverrideOf = parentProperty;
            }
        }
    }

    private static Dictionary<string, SyntaxProperty> CollectParentProperties(SyntaxNode node)
    {
        var result = new Dictionary<string, SyntaxProperty>();
        var @base = node.Base;
        while (@base is not null)
        {
            foreach (var property in @base.Properties)
            {
                result[property.Name] = property;
            }
            @base = @base.Base;
        }
        return result;
    }

    private void ResolveDependencies()
    {
        foreach (var module in _modules.Values)
        {
            var seen = new HashSet<string>();
            foreach (var node in module.Nodes)
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
}
