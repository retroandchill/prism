// @file SynthesizedDefaultConstructor.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Compiler.Abi;
using Prism.Compiler.Semantic;
using Prism.Compiler.Symbols.Source;
using Prism.Diagnostics;
using Prism.Metadata;
using Prism.Semantic;
using Prism.Syntax;

namespace Prism.Compiler.Symbols.Synthesized;

internal sealed class SynthesizedDefaultConstructor : SynthesizedFunction
{
    public SynthesizedDefaultConstructor(Compilation compilation, NamedTypeSymbol containingType)
        : base(compilation)
    {
        ContainingType = containingType;
    }

    public override string Name => CommonNames.Ctor;
    public override Symbol ContainingSymbol => ContainingType;
    public override TypeSymbol ContainingType { get; }
    public override MemberContainerSymbol ContainingMemberContainer => ContainingType;
    public override TypeSymbol ReturnType => DeclaringCompilation.GetSpecialType(SpecialType.Void);

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
    public override AbiKind AbiKind => AbiKind.Internal;
    public override bool IsExtern => false;
}
