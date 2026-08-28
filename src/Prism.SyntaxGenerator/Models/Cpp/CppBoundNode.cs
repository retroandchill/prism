// @file CppBoundNode.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.Cpp;

public sealed class CppBoundNode
{
    public string Name { get; }

    public string ClassName { get; }

    public string LowerName { get; }

    public bool HasModule { get; }

    public ImmutableArray<CppBoundNode> LeafNodes { get; }

    internal CppBoundNode(
        string name,
        string className,
        bool hasModule,
        string lowerName,
        ImmutableArray<CppBoundNode> leafNodes
    )
    {
        Name = name;
        ClassName = className;
        LowerName = lowerName;
        HasModule = hasModule;
        LeafNodes = leafNodes;
    }
}
