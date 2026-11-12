// @file VariableSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Prism.Constants;

namespace Prism.Semantic;

public sealed class VariableSymbol : ValueSymbol
{
    internal VariableSymbol(IVariableSymbolData data)
        : base(data) { }

    internal new IVariableSymbolData Data => Unsafe.As<IVariableSymbolData>(base.Data);

    public VariableKind Kind => Data.Kind;

    public bool IsStaticStorage => Kind is VariableKind.Global or VariableKind.Class;

    public bool IsInstance => Kind is VariableKind.Instance;

    public bool IsLocal => Kind == VariableKind.Local;

    public bool HasInitializer => Data.HasInitializer;

    [MemberNotNullWhen(true, nameof(ConstantValue))]
    public bool IsConst => ConstantValue is not null;

    public ConstantValue? ConstantValue => Data.ConstantValue;
}
