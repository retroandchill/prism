// @file SourceConstructorSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Binding;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using Prism.Core.Utils;

namespace Prism.Core.Symbols.Source;

internal sealed class SourceConstructorSymbol : SourceFunctionSymbolBase
{
    public ConstructorDeclarationSyntax Syntax { get; }

    public SourceConstructorSymbol(
        NamedTypeSymbol containingSymbol,
        ConstructorDeclarationSyntax syntax
    )
        : base(CommonNames.Ctor, containingSymbol, syntax)
    {
        Syntax = syntax;
        ContainingType = containingSymbol;
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        compilation.CacheSymbol(Syntax, this);
    }

    public override FunctionKind Kind => FunctionKind.Constructor;

    public override string MetadataName
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, ProduceMetadataName(Syntax.SyntaxTree), null);
            return field;
        }
    }

    public override TypeSymbol ContainingType { get; }

    public override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedCompareExchange(
                ref field,
                [Syntax.Keyword.Location],
                default
            );
            return field;
        }
    }

    protected override DeclarationSyntax AbstractSyntax => Syntax;

    protected override TypeSymbol ComputeReturnType(BindingContext context)
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        return compilation.GetSpecialType(SpecialType.Void);
    }

    protected override TypeSymbol ComputeReceiverType(TypeSymbol containerType)
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        return compilation.CreateReferenceTypeSymbol(ContainingType, true);
    }

    protected override ImmutableArray<ParameterSymbol> ComputeParameters(BindingContext context)
    {
        var source = Syntax.Parameters.Parameters;
        return CreateParametersFromSource(source, context);
    }

    protected override void FunctionChecks(BindingContext context)
    {
        // No checks needed currently
    }
}
