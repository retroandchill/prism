// @file MirPlace.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Symbols;

namespace Prism.Core.Mir;

internal abstract record MirPlace(TypeSymbol Type);

internal sealed record MirLocalPlace(MirLocalId LocalId, TypeSymbol Type) : MirPlace(Type)
{
    public MirLocalPlace(MirLocal local)
        : this(local.Id, local.Type) { }
}

internal sealed record MirGlobalPlace(VariableSymbol Variable)
    : MirPlace(Variable.EnsureGlobal().Type);

internal sealed record MirDerefPlace(MirValue Pointer, TypeSymbol Type) : MirPlace(Type);

internal sealed record MirIndexPlace(MirPlace Base, MirValue Index, TypeSymbol Type)
    : MirPlace(Type);
