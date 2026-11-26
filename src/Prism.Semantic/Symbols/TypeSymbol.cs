// @file TypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Prism.Semantic.Layout;
using Prism.Semantic.Symbols.Data;

namespace Prism.Semantic.Symbols;

public closed class TypeSymbol : MemberContainerSymbol
{
    private protected TypeSymbol(ITypeSymbolData data)
        : base(data) { }

    internal new ITypeSymbolData Data => Unsafe.As<ITypeSymbolData>(base.Data);

    public SpecialType SpecialType => Data.SpecialType;

    public bool IsErrorType => Data.IsErrorType;

    public bool IsVoid => SpecialType == SpecialType.Void;

    public bool IsDynamicallySized => Data.IsDynamicallySized;

    public bool IsDefaultConstructible => Data.IsDefaultConstructible;

    public abstract TypeLayout GetLayout(CancellationToken cancellationToken = default);

    public TypeSymbol UnwrapTypeAlias()
    {
        var typeSymbol = this;
        while (typeSymbol is TypeAliasSymbol typeAliasSymbol)
        {
            typeSymbol = typeAliasSymbol.AliasedType;
        }

        return typeSymbol;
    }

    public sealed override bool Equals(Symbol other, SymbolCompareKind kind)
    {
        return other switch
        {
            TypeAliasSymbol typeAliasSymbol => Equals(typeAliasSymbol.AliasedType, kind),
            TypeSymbol typeSymbol => Equals(typeSymbol, kind),
            _ => false,
        };
    }

    public virtual bool Equals(TypeSymbol other, SymbolCompareKind kind)
    {
        return ReferenceEquals(this, other);
    }
}
