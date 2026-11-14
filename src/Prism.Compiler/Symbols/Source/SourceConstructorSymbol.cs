// @file SourceConstructorSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Compiler.Binding;
using Prism.Compiler.Semantic;
using Prism.Diagnostics;
using Prism.Metadata;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using Prism.Syntax;

namespace Prism.Compiler.Symbols.Source;

internal sealed class SourceConstructorSymbol(
    Compilation compilation,
    NamedTypeSymbol containingSymbol,
    ConstructorDeclarationSyntax syntax
) : SourceFunctionSymbolBase(compilation, containingSymbol, syntax)
{
    public ConstructorDeclarationSyntax Syntax { get; } = syntax;

    public override string Name => CommonNames.Ctor;

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

    public override TypeSymbol ContainingType { get; } = containingSymbol;

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
