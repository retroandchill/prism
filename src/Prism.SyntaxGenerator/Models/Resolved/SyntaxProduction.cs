// @file SyntaxProduction.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.Resolved;

public readonly record struct SyntaxProductionArgument(SyntaxProperty Property, bool IsOptional);

public sealed class SyntaxProduction : ISyntaxVariant
{
    public SyntaxNode Node { get; }

    private readonly List<SyntaxProductionArgument> _arguments;
    public IReadOnlyList<SyntaxProductionArgument> Arguments => _arguments;

    internal SyntaxProduction(SyntaxNode node, List<SyntaxProductionArgument>? arguments = null)
    {
        Node = node;
        _arguments = arguments ?? [];
    }

    internal void Add(SyntaxProductionArgument argument)
    {
        _arguments.Add(argument);
    }

    internal SyntaxProduction Clone()
    {
        return new SyntaxProduction(Node, [.. _arguments]);
    }
}
