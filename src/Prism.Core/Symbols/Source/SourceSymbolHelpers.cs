// @file SourceSymbolHelpers.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Binding;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using ZLinq;

namespace Prism.Core.Symbols.Source;

internal static class SourceSymbolHelpers
{
    public static ImmutableArray<AttributeData> GetAttributes(
        Symbol symbol,
        SyntaxList<AttributeListSyntax> attributes,
        Binder binder,
        BindingContext context
    )
    {
        if (attributes.Count == 0)
            return [];

        var builder = ImmutableArray.CreateBuilder<AttributeData>();

        foreach (
            var attribute in attributes
                .AsValueEnumerable()
                .SelectMany(l => l.Attributes.AsValueEnumerable())
        )
        {
            var attributeType = (NamedTypeSymbol)binder.ResolveType(attribute.Type, context);
            if (attributeType.Kind != NamedTypeKind.Attribute)
            {
                context.ReportDiagnostic(
                    Diagnostic.TypeIsNotAttribute(
                        attribute.Type.Location,
                        attributeType.ToDisplayString()
                    )
                );
                continue;
            }

            // TODO: We need to actually parse the arguments
            builder.Add(new AttributeData(new SyntaxReference(attribute), attributeType, []));
        }

        return builder.DrainToImmutable();
    }
}
