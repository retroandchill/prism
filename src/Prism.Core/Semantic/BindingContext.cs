// @file BindingContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Semantic;

public sealed class BindingContext
{
    private readonly HashSet<SymbolDeclaration> _inProgress =
    [
        with(ReferenceEqualityComparer.Instance),
    ];

    public bool EnterInference(SymbolDeclaration decl) => _inProgress.Add(decl);

    public void ExitInference(SymbolDeclaration decl) => _inProgress.Remove(decl);

    public bool IsInferring(SymbolDeclaration decl) => _inProgress.Contains(decl);
}
