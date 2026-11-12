// @file ParameterSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Prism.Semantic;

public sealed class ParameterSymbol : ValueSymbol
{
    internal ParameterSymbol(IParameterSymbolData data)
        : base(data) { }

    internal new IParameterSymbolData Data => Unsafe.As<IParameterSymbolData>(base.Data);

    [MemberNotNullWhen(true, nameof(DefaultValue))]
    public bool HasDefaultValue => DefaultValue is not null;

    public ParameterDefault? DefaultValue => Data.DefaultValue;
}
