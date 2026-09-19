// @file MergedTypeDeclaration.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Symbols;
using ZLinq;

namespace Prism.Core.Declarations;

internal sealed class MergedTypeDeclaration : MergedDeclaration
{
    private MergedTypeDeclaration(ImmutableArray<SingleTypeDeclaration> declarations)
        : base(declarations[0].Name)
    {
        Declarations = declarations;
        Modifiers = declarations.Aggregate(DeclarationModifiers.None, (current, declaration) =>
            current | declaration.Modifiers
        );
    }

    public static MergedTypeDeclaration Create(ImmutableArray<SingleTypeDeclaration> declarations)
    {
        return new MergedTypeDeclaration(declarations);
    }

    public ImmutableArray<SingleTypeDeclaration> Declarations { get; }

    public override DeclarationKind Kind => Declarations[0].Kind;

    public DeclarationModifiers Modifiers { get; }

    protected override ImmutableArray<MergedDeclaration> GetMergedMembers()
    {
        return ImmutableArray<MergedDeclaration>.CastUp(Members);
    }

    public new ImmutableArray<MergedTypeDeclaration> Members
    {
        get
        {
            if (!field.IsDefault)
            {
                return field;
            }

            ImmutableInterlocked.InterlockedCompareExchange(ref field, MakeChildren(), default);
            return field;
        }
    }

    private ImmutableArray<MergedTypeDeclaration> MakeChildren()
    {
        var types = ImmutableArray.CreateBuilder<SingleTypeDeclaration>();
        var allTypesHaveSameIdentity = true;

        foreach (
            var child in Declarations
                .AsValueEnumerable()
                .SelectMany(d => d.Members.AsValueEnumerable())
        )
        {
            if (types.Count > 0 && allTypesHaveSameIdentity && child.Identity != types[0].Identity)
            {
                allTypesHaveSameIdentity = false;
            }

            types.Add(child);
        }

        var children = ImmutableArray.CreateBuilder<MergedTypeDeclaration>();
        AddTypesToChildren(types, allTypesHaveSameIdentity, children);

        return children.DrainToImmutable();
    }

    private static void AddTypesToChildren(
        ImmutableArray<SingleTypeDeclaration>.Builder types,
        bool allTypesHaveSameName,
        ImmutableArray<MergedTypeDeclaration>.Builder children
    )
    {
        if (types.Count == 0)
            return;

        if (allTypesHaveSameName)
        {
            children.Add(Create(types.DrainToImmutable()));
            return;
        }

        foreach (var grouping in types.GroupBy(ns => ns.Name))
        {
            children.Add(Create([.. grouping]));
        }
    }
}
