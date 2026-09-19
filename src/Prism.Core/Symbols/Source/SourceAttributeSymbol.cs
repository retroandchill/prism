// @file SourceAttributeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Binding;
using Prism.Core.Configuration;
using Prism.Core.Syntax;
using Prism.Core.Utils;

namespace Prism.Core.Symbols.Source;

internal sealed class SourceAttributeSymbol : SourceNamedTypeSymbol
{
    private readonly AttributeDeclarationSyntax _syntax;

    internal SourceAttributeSymbol(
        string name,
        Symbol containingSymbol,
        AttributeDeclarationSyntax syntax
    )
        : base(name, containingSymbol, NamedTypeKind.Attribute)
    {
        _syntax = syntax;
    }

    protected override TypeDeclarationSyntax Syntax => _syntax;

    protected override ImmutableDictionary<string, ImmutableArray<Symbol>> MakeNameToMembersMap(
        BindingContext context
    )
    {
        if (_syntax.Parameters is null)
            return ImmutableDictionary<string, ImmutableArray<Symbol>>.Empty;

        var builder = new Dictionary<string, ImmutableArray<Symbol>.Builder>(
            _syntax.Parameters.Parameters.Count
        );
        foreach (var parameter in _syntax.Parameters.Parameters)
        {
            var name = parameter.Name.IdentifierName;
            var subBuilder = builder.GetOrAdd(name, () => ImmutableArray.CreateBuilder<Symbol>(1));
            // TODO: Add the parameters
        }

        return builder.ToImmutableDictionary(
            pair => pair.Key,
            pair => pair.Value.DrainToImmutable()
        );
    }

    public override SizeAndAlignment GetSizeAndAlignment(CompilationSettings settings)
    {
        throw new NotSupportedException("Attributes are compile-time only constructs");
    }
}
