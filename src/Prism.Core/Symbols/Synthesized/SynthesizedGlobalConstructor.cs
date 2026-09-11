// @file SynthesizedGlobalConstructor.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Abi;
using Prism.Core.BoundTree;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Symbols.Synthesized;

internal sealed class SynthesizedGlobalConstructor(NamespaceSymbol containingNamespace)
    : FunctionSymbol(CommonNames.GlobalCtor, containingNamespace)
{
    private ThreeState _shouldEmit = ThreeState.Unknown;

    public override bool IsImplicitlyDeclared => true;
    public override ImmutableArray<Location> Locations => [];
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];
    public override TypeSymbol ReturnType
    {
        get
        {
            Debug.Assert(DeclaringCompilation is not null);
            return DeclaringCompilation.GetSpecialType(SpecialType.Void);
        }
    }

    public override ImmutableArray<ParameterSymbol> Parameters => [];
    internal override AbiKind AbiKind => AbiKind.Internal;

    internal bool ShouldEmit(ReadOnlySpan<BoundVariableInitializer> variables)
    {
        if (_shouldEmit.HasValue)
            return _shouldEmit.Value;

        var shouldEmit = ComputeShouldEmit(variables);
        _shouldEmit = shouldEmit.ToThreeState();
        return shouldEmit;
    }

    private static bool ComputeShouldEmit(ReadOnlySpan<BoundVariableInitializer> variables)
    {
        return variables.AsValueEnumerable().Any(variable => variable.ConstantValue is null);
    }
}
