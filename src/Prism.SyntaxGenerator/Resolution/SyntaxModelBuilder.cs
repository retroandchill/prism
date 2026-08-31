// @file SyntaxModel.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Text;
using Prism.SyntaxGenerator.Models.Resolved;
using Prism.SyntaxGenerator.Models.Spec;
using Prism.SyntaxGenerator.Utilities;
using ZLinq;

namespace Prism.SyntaxGenerator.Resolution;

public sealed class SyntaxModelBuilder
{
    private const int TriviaStart = 100;
    private const int StructuredTriviaStart = 200;
    private const int StructuredTriviaStep = 100;
    private const int KeywordsStart = 1000;
    private const int PunctuationsStart = 1200;
    private const int OtherTokensStart = 1300;
    private const int SyntaxNodeStart = 2000;
    private const int SyntaxNodeStep = 1000;

    private readonly OrderedDictionary<string, SyntaxKind> _syntaxKinds = new();
    private readonly List<SyntaxGroup> _syntaxKindGroups = [];
    private readonly OrderedDictionary<string, SyntaxTrivia> _trivia = new();
    private readonly OrderedDictionary<string, SyntaxToken> _tokens = new();
    private readonly OrderedDictionary<string, SyntaxModule> _modules = new();
    private readonly SyntaxDispatchGroup _topLevelDispatchGroup = new("Node", true);
    private readonly SyntaxDispatchGroup _structuredTriviaDispatchGroup = new("StructuredTrivia");
    private readonly OrderedDictionary<SyntaxNode, SyntaxDispatchGroup> _dispatchGroups = new();
    private readonly Dictionary<string, SyntaxNode> _nodes = new();
    private readonly Dictionary<SyntaxNode, NodeDefinition> _nodeDefinitions = new();
    private readonly Dictionary<SyntaxNode, Dictionary<string, SyntaxProperty>> _nodeProperties =
        new();
    private readonly List<DiagnosticCategory> _diagnostics = [];
    private readonly List<BoundNode> _boundNodes = [];

    public SyntaxModel Build(SyntaxSpecification spec)
    {
        EnsureCapacity(spec);
        LoadTrivia(spec);
        LoadTokens(spec);
        LoadNodes(spec);
        LoadDiagnostics(spec);
        LoadBoundNodes(spec);
        ResolveInheritance();
        ResolveProperties();
        ResolveProductions();
        ResolveOverrides();
        ResolveDependencies();
        ResolveNodeKinds();
        ResolveDispatchGroups();
        return new SyntaxModel(
            [.. _syntaxKinds.Values],
            [.. _syntaxKindGroups.OrderBy(g => g.StartValue)],
            [.. _trivia.Values],
            [.. _tokens.Values],
            [.. _modules.Values],
            [_topLevelDispatchGroup, _structuredTriviaDispatchGroup, .. _dispatchGroups.Values],
            [.. _diagnostics],
            [.. _boundNodes]
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
        _nodes.EnsureCapacity(nodeCount + 2);
        _diagnostics.EnsureCapacity(spec.Diagnostics.Length);
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
        foreach (var module in spec.Modules)
        {
            var resolvedModule = new SyntaxModule(module.Name, module.Kind);
            _modules.Add(module.Name, resolvedModule);
            if (module.Nodes.IsEmpty)
                continue;

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

    private void LoadDiagnostics(SyntaxSpecification spec)
    {
        const int start = 1000;
        const int step = 1000;

        foreach (var (i, category) in spec.Diagnostics.AsValueEnumerable().Index())
        {
            var firstInstance = start + step * i;
            var resolvedCategory = new DiagnosticCategory(
                category.Name,
                firstInstance,
                firstInstance + step - 1
            );
            resolvedCategory.EnsureCapacity(category.Diagnostics.Length);
            foreach (var (j, diagnostic) in category.Diagnostics.AsValueEnumerable().Index())
            {
                var resolvedDiagnostic = new Diagnostic(
                    diagnostic.Name,
                    firstInstance + j,
                    resolvedCategory,
                    diagnostic.Severity,
                    diagnostic.Format,
                    diagnostic.Explanation,
                    diagnostic.HelpLink,
                    diagnostic.Tags
                );
                var arguments = ResolveArguments(diagnostic, resolvedDiagnostic);
                ResolveMessageParts(resolvedDiagnostic, arguments);
                resolvedCategory.AddDiagnostic(resolvedDiagnostic);
            }
            _diagnostics.Add(resolvedCategory);
        }
    }

    private static FrozenDictionary<string, DiagnosticArgument> ResolveArguments(
        DiagnosticDefinition definition,
        Diagnostic diagnostic
    )
    {
        if (definition.Args.Length == 0)
            return FrozenDictionary<string, DiagnosticArgument>.Empty;

        var dictionary = new Dictionary<string, DiagnosticArgument>();
        dictionary.EnsureCapacity(definition.Args.Length);
        diagnostic.EnsureCapacity(definition.Args.Length);
        foreach (var arg in definition.Args)
        {
            var resolvedArgument = new DiagnosticArgument(arg.Name, arg.Type, diagnostic);
            diagnostic.AddArgument(resolvedArgument);
            dictionary.Add(arg.Name, resolvedArgument);
        }
        return dictionary.ToFrozenDictionary();
    }

    private static void ResolveMessageParts(
        Diagnostic diagnostic,
        FrozenDictionary<string, DiagnosticArgument> arguments
    )
    {
        using var currentTextSegment = ZString.CreateStringBuilder();
        var lookup = arguments.GetAlternateLookup<ReadOnlySpan<char>>();

        var cursor = new TextCursor(diagnostic.Format);
        while (!cursor.IsAtEnd)
        {
            if (cursor.TryConsume('{'))
            {
                if (cursor.TryConsume('{'))
                {
                    currentTextSegment.Append('{');
                }
                else
                {
                    if (currentTextSegment.Length > 0)
                    {
                        diagnostic.AddMessagePart(
                            new DiagnosticMessageTextPart(currentTextSegment.ToString())
                        );
                        currentTextSegment.Clear();
                    }

                    var remaining = cursor.Remaining;
                    var start = cursor.Position;
                    while (cursor.Current != '}')
                    {
                        if (cursor.IsAtEnd)
                        {
                            throw new InvalidOperationException(
                                $"Unexpected end of file at position {cursor.Position}"
                            );
                        }

                        cursor.Advance();
                    }

                    var chunk = remaining[..(cursor.Position - start)];
                    if (!lookup.TryGetValue(chunk, out var arg))
                        throw new InvalidOperationException($"Unexpected argument name {chunk}");

                    cursor.Advance();
                    diagnostic.AddMessagePart(new DiagnosticMessageArgumentPart(arg));
                }
            }
            else if (cursor.TryConsume('}'))
            {
                if (cursor.TryConsume('}'))
                {
                    currentTextSegment.Append('}');
                }
                else
                {
                    throw new InvalidOperationException("Unmatched '}'");
                }
            }
            else
            {
                currentTextSegment.Append(cursor.Current);
                cursor.Advance();
            }
        }

        if (currentTextSegment.Length > 0)
        {
            diagnostic.AddMessagePart(new DiagnosticMessageTextPart(currentTextSegment.ToString()));
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

    private void ResolveNodeKinds()
    {
        var nextNodeStart = SyntaxNodeStart;
        var nextTriviaStart = StructuredTriviaStart;
        foreach (var module in _modules.Values)
        {
            var nextValue = module.Kind switch
            {
                ModuleKind.Node => nextNodeStart,
                ModuleKind.StructuredTrivia => nextTriviaStart,
                _ => throw new InvalidOperationException("Invalid module kind"),
            };
            var builder = ImmutableArray.CreateBuilder<SyntaxKind>(module.Nodes.Count);
            foreach (var node in module.Nodes.AsValueEnumerable().Where(n => !n.IsAbstract))
            {
                node.Kind = new SyntaxKind(node.Name, nextValue++, node);
                _syntaxKinds.Add(node.Kind.Name, node.Kind);
                builder.Add(node.Kind);
            }

            var group = new SyntaxGroup(
                module.Name,
                module.Kind switch
                {
                    ModuleKind.Node => SyntaxGroupKind.Node,
                    ModuleKind.StructuredTrivia => SyntaxGroupKind.StructuredTrivia,
                    _ => throw new InvalidOperationException("Invalid module kind"),
                },
                builder.DrainToImmutable()
            );
            _syntaxKindGroups.Add(group);
            switch (module.Kind)
            {
                case ModuleKind.Node:
                    nextNodeStart += SyntaxNodeStep;
                    break;
                case ModuleKind.StructuredTrivia:
                    nextTriviaStart += StructuredTriviaStep;
                    break;
                default:
                    throw new InvalidOperationException("Invalid module kind");
            }
        }
    }

    private void ResolveDispatchGroups()
    {
        foreach (var node in _modules.Values.AsValueEnumerable().SelectMany(x => x.Nodes))
        {
            if (!node.IsAbstract)
            {
                AddToDispatchGroups(node);
            }
            else
            {
                _dispatchGroups.Add(node, new SyntaxDispatchGroup(node.Name));
            }
        }
    }

    private void AddToDispatchGroups(SyntaxNode node)
    {
        _topLevelDispatchGroup.AddNode(node);
        if (node.Module.Kind == ModuleKind.StructuredTrivia)
            _structuredTriviaDispatchGroup.AddNode(node);

        // We're assuming because of C++'s declaration order requirements that parent classes are already defined.
        var baseClass = node.Base;
        while (baseClass is not null)
        {
            _dispatchGroups[baseClass].AddNode(node);
            baseClass = baseClass.Base;
        }
    }

    private void LoadBoundNodes(SyntaxSpecification spec)
    {
        foreach (var definition in spec.BoundNodes)
        {
            LoadBoundNode(definition, []);
        }
    }

    private void LoadBoundNode(BoundNodeDefinition definition, ImmutableArray<BoundNode> parents)
    {
        var boundNode = new BoundNode(definition.Name, definition.HasModule);
        _boundNodes.Add(boundNode);

        foreach (var childDefinition in definition.Children)
        {
            LoadBoundNode(childDefinition, parents.Add(boundNode));
        }

        if (!definition.Children.IsEmpty)
            return;

        foreach (var parent in parents)
        {
            parent.AddLeafNode(boundNode);
        }
    }
}
