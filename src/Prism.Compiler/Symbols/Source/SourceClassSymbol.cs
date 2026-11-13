// @file SourceAttributeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Compiler.Declarations;
using Prism.Compiler.Symbols.Synthesized;
using Prism.Semantic;
using Prism.Utils;
using ZLinq;

namespace Prism.Compiler.Symbols.Source;

internal sealed class SourceClassSymbol : SourceNamedTypeSymbol
{
    private ThreeState _isDefaultConstructible;

    internal SourceClassSymbol(
        Compilation compilation,
        MergedTypeDeclaration declaration,
        MemberContainerSymbol containingSymbol
    )
        : base(compilation, declaration, containingSymbol)
    {
        Debug.Assert(declaration.Kind == DeclarationKind.Class);

        var declarations = declaration.Declarations;
        Debug.Assert(declarations.Length == 1);
    }

    protected override void AddSynthesizedMembers(
        Dictionary<string, ImmutableArray<Symbol>.Builder> nameToMembersMap
    )
    {
        if (!nameToMembersMap.ContainsKey(CommonNames.Ctor))
        {
            TryAddDefaultConstructor(nameToMembersMap);
        }
    }

    private void TryAddDefaultConstructor(
        Dictionary<string, ImmutableArray<Symbol>.Builder> nameToMembersMap
    )
    {
        if (SpecialType is SpecialType.Void or SpecialType.Str)
            return;

        foreach (
            var field in nameToMembersMap
                .AsValueEnumerable()
                .SelectMany(x => x.Value.AsValueEnumerable())
                .OfType<VariableSymbol>()
                .Where(v => v.IsInstance)
        )
        {
            if (field is { HasInitializer: false, Type.IsDefaultConstructible: false })
                return;
        }

        var builder = ImmutableArray.CreateBuilder<Symbol>();
        builder.Add(
            new SynthesizedDefaultConstructor(DeclaringCompilation, PublicSymbol).PublicSymbol
        );
        nameToMembersMap.Add(CommonNames.Ctor, builder);
    }

    public override bool IsDefaultConstructible
    {
        get
        {
            if (_isDefaultConstructible.HasValue)
                return _isDefaultConstructible.Value;

            var isDefaultConstructible = ComputeIsDefaultConstructible();
            Interlocked.Exchange(
                ref _isDefaultConstructible,
                isDefaultConstructible.ToThreeState()
            );
            return isDefaultConstructible;
        }
    }

    private bool ComputeIsDefaultConstructible()
    {
        return GetMembers(CommonNames.Ctor)
            .OfType<FunctionSymbol>()
            .Any(f => f.Parameters.Length == 0);
    }
}
