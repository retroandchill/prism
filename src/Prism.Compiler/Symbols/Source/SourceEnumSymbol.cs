// @file SourceEnumSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Prism.Compiler.Declarations;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Utils;

namespace Prism.Compiler.Symbols.Source;

internal sealed class SourceEnumSymbol : SourceNamedTypeSymbol, IEnumSymbolData
{
    private ThreeState _isDefaultConstructible;

    internal SourceEnumSymbol(
        Compilation compilation,
        MergedTypeDeclaration declaration,
        MemberContainerSymbol containingSymbol
    )
        : base(compilation, declaration, containingSymbol)
    {
        Debug.Assert(declaration.Kind == DeclarationKind.Enum);

        var declarations = declaration.Declarations;
        Debug.Assert(declarations.Length == 1);
    }

    public NamedTypeSymbol EnumUnderlyingType
    {
        get
        {
            return LazyComputation.LazyCompute(ref field, this, static @this =>
                @this.ComputeUnderlyingType()
            );
        }
    }

    private NamedTypeSymbol ComputeUnderlyingType()
    {
        // TODO: Currently we don't actually have a syntax for the underlying type, once we do, we need to parse that

        // Fallback is i32 to line up with what C, C++, and C# use for defaults
        return DeclaringCompilation.GetSpecialType(SpecialType.I32);
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
        return true;
    }
}
