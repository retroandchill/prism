// @file SyntaxNode.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.SyntaxGenerator.Models.Spec;

namespace Prism.SyntaxGenerator.Models.Resolved;

public sealed class SyntaxNode : ISyntaxVariant
{
    private readonly List<SyntaxNode> _derivedTypes = [];
    private readonly List<SyntaxProperty> _properties = [];
    private readonly List<SyntaxProduction> _productions = [];
    public string Name { get; }

    public bool IsAbstract => DerivedTypes.Count > 0;

    public SyntaxNode? Base { get; internal set; }

    public SyntaxModule Module { get; }

    public SyntaxKind? Kind { get; internal set; }

    public IReadOnlyList<SyntaxNode> DerivedTypes => _derivedTypes;

    public IReadOnlyList<SyntaxProperty> Properties => _properties;

    public IReadOnlyList<SyntaxProduction> Productions => _productions;

    internal SyntaxNode(SyntaxModule module, string name)
    {
        Module = module;
        Name = name;
    }

    internal void AddDerivedType(SyntaxNode node) => _derivedTypes.Add(node);

    internal void AddProperty(SyntaxProperty property) => _properties.Add(property);

    internal void AddProductions(IEnumerable<SyntaxProduction> production) =>
        _productions.AddRange(production);
}
