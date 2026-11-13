// @file ParameterSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Cysharp.Text;

namespace Prism.Semantic;

public sealed class ParameterSymbol : ValueSymbol
{
    internal ParameterSymbol(IParameterSymbolData data)
        : base(data) { }

    internal new IParameterSymbolData Data => Unsafe.As<IParameterSymbolData>(base.Data);

    public FunctionSymbol? ContainingFunction =>
        Data is IFunctionOwnedSymbolData functionOwned ? functionOwned.ContainingFunction : null;

    [MemberNotNullWhen(true, nameof(DefaultValue))]
    public bool HasDefaultValue => DefaultValue is not null;

    public ParameterDefault? DefaultValue => Data.DefaultValue;

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        builder.Append(Name);
        builder.Append(": ");
        Type.WriteDisplayString(ref builder);
    }
}
