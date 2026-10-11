// @file SynthesizedDefaultConstructor.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Abi;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using Prism.Core.Utils;

namespace Prism.Core.Symbols.Synthesized;

internal sealed class SynthesizedDefaultConstructor(NamedTypeSymbol containingType)
    : FunctionSymbol(CommonNames.Ctor, containingType)
{
    public override TypeSymbol ContainingType { get; } = containingType;
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

    public override TypeSymbol ReceiverType
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, ComputeReceiverType(), null);
            return field;
        }
    }

    private ReferenceTypeSymbol ComputeReceiverType()
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        return compilation.CreateReferenceTypeSymbol(ContainingType, true);
    }

    public override ImmutableArray<ParameterSymbol> Parameters => [];
    public override FunctionKind Kind => FunctionKind.Constructor;
    internal override AbiKind AbiKind => AbiKind.Internal;
    public override bool IsExtern => false;
}
