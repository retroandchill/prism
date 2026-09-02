// @file SourceLabelSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Binding;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Source;

internal sealed class SourceLabelSymbol(
    FunctionSymbol containingFunction,
    LabeledStatementSyntax syntax
) : LabelSymbol(syntax.Identifier.IdentifierName, containingFunction)
{
    public override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedInitialize(ref field, [syntax.Location]);
            return field;
        }
    }

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedInitialize(ref field, [new SyntaxReference(syntax)]);
            return field;
        }
    }
}
