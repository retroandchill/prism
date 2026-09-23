// @file SourceAttributeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Binding;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using Prism.Core.Utils;

namespace Prism.Core.Symbols.Source;

internal sealed class SourceAttributeSymbol : SourceNamedTypeSymbol
{
    private readonly AttributeDeclarationSyntax _syntax;

    internal SourceAttributeSymbol(MergedTypeDeclaration declaration, Symbol containingSymbol)
        : base(declaration, containingSymbol)
    {
        Debug.Assert(declaration.Kind == DeclarationKind.Attribute);

        var references = DeclaringSyntaxReferences;
        Debug.Assert(references.Length == 1);
        _syntax = (AttributeDeclarationSyntax)references[0].Syntax;
    }

    protected override TypeDeclarationSyntax Syntax => _syntax;

    protected override ImmutableDictionary<string, ImmutableArray<Symbol>> MakeNameToMembersMapCore(
        BindingContext context
    )
    {
        if (_syntax.Parameters is null)
            return ImmutableDictionary<string, ImmutableArray<Symbol>>.Empty;

        var builder = new Dictionary<string, ImmutableArray<Symbol>.Builder>(
            _syntax.Parameters.Parameters.Count
        );
        var seenDefaultValue = false;
        foreach (var parameter in _syntax.Parameters.Parameters)
        {
            var name = parameter.Name.IdentifierName;
            var subBuilder = builder.GetOrAdd(name, () => ImmutableArray.CreateBuilder<Symbol>(1));

            var symbol = new SourceAttributeParameterSymbol(name, this, parameter);
            subBuilder.Add(symbol);

            if (symbol.HasDefaultValue)
            {
                seenDefaultValue = true;
            }
            else if (seenDefaultValue)
            {
                context.ReportDiagnostic(
                    Diagnostic.DefaultValueAfterNonDefaultValue(parameter.Location, name)
                );
            }
        }

        return builder.ToImmutableDictionary(
            pair => pair.Key,
            pair => pair.Value.DrainToImmutable()
        );
    }
}
