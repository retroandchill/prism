// @file MirLocal.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Symbols;

namespace Prism.Core.Mir;

internal readonly record struct MirLocalId(int Value);

internal abstract record MirLocal(MirLocalId Id, string Name, TypeSymbol Type)
{
    public abstract Symbol? Symbol { get; }
}

internal sealed record MirParameter(MirLocalId Id, ParameterSymbol Parameter)
    : MirLocal(Id, Parameter.Name, Parameter.Type)
{
    public override Symbol Symbol => Parameter;
}

internal sealed record MirLocalVariable(MirLocalId Id, VariableSymbol Variable)
    : MirLocal(Id, Variable.EnsureLocal().Name, Variable.Type)
{
    public override Symbol Symbol => Variable;
}

internal sealed record MirTemporary(MirLocalId Id, string Name, TypeSymbol Type)
    : MirLocal(Id, Name, Type)
{
    public override Symbol? Symbol => null;
}
