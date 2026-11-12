// @file TypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Prism.Semantic;

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
}
