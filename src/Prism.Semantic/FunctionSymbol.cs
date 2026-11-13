// @file FunctionSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using ZLinq;

namespace Prism.Semantic;

public sealed class FunctionSymbol : Symbol
{
    internal FunctionSymbol(IFunctionSymbolData data)
        : base(data) { }

    internal new IFunctionSymbolData Data => Unsafe.As<IFunctionSymbolData>(base.Data);

    public FunctionKind Kind => Data.Kind;

    public AbiKind AbiKind => Data.AbiKind;

    public bool IsExtern => Data.IsExtern;

    public TypeSymbol ReturnType => Data.ReturnType;

    public TypeSymbol? ReceiverType => Data.ReceiverType;

    public ImmutableArray<ParameterSymbol> Parameters => Data.Parameters;

    public bool ReturnsVoid => ReturnType.IsVoid;

    internal ImmutableArray<TypeSymbol> ParameterTypes
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. Parameters.Select(p => p.Type)]
            );
            return field;
        }
    }

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        if (
            ContainingSymbol
            is not null
                and not AssemblySymbol
                and NamespaceSymbol { IsGlobal: false }
        )
        {
            ContainingSymbol.WriteDisplayString(ref builder);
            builder.Append("::");
        }

        builder.Append(Name);
        builder.Append('(');
        foreach (var (i, param) in Parameters.AsValueEnumerable().Index())
        {
            if (i > 0)
                builder.Write(", ");

            param.WriteDisplayString(ref builder);
        }
        builder.Append(')');
        if (ReturnsVoid)
            return;

        builder.Write(": ");
        ReturnType.WriteDisplayString(ref builder);
    }
}
