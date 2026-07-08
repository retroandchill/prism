// @file Compilation.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Semantic;

public sealed class Compilation
{
    public SemanticModel SemanticModel { get; }
    public TargetPlatform TargetPlatform { get; }

    public Compilation(TargetPlatform targetPlatform)
    {
        TargetPlatform = targetPlatform;
        SemanticModel = new SemanticModel(this);
    }
}
