// @file SynthesizedGlobalConstructor.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Compiler.Abi;
using Prism.Compiler.BoundTree;
using Prism.Compiler.Semantic;
using Prism.Diagnostics;
using Prism.Metadata;
using Prism.Semantic;
using Prism.Syntax;
using Prism.Utils;
using ZLinq;

namespace Prism.Compiler.Symbols.Synthesized;

internal sealed class SynthesizedGlobalConstructor(
    Compilation compilation,
    NamespaceSymbol containingNamespace
) : SynthesizedFunction(compilation)
{
    private ThreeState _shouldEmit = ThreeState.Unknown;

    public override string Name => CommonNames.GlobalCtor;
    public override Symbol ContainingSymbol => ContainingNamespace;
    public override NamespaceSymbol ContainingNamespace { get; } = containingNamespace;

    public override TypeSymbol? ContainingType => null;

    public override FunctionKind Kind => FunctionKind.GlobalConstructor;

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];
    public override TypeSymbol ReturnType
    {
        get
        {
            Debug.Assert(DeclaringCompilation is not null);
            return DeclaringCompilation.GetSpecialType(SpecialType.Void);
        }
    }

    public override TypeSymbol? ReceiverType => null;

    public override ImmutableArray<ParameterSymbol> Parameters => [];
    public override AbiKind AbiKind => AbiKind.Internal;
    public override bool IsExtern => false;

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
