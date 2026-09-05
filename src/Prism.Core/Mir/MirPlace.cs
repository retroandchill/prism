// @file MirPlace.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Mir;

internal abstract record MirPlace(MirType Type);

internal sealed record MirLocalPlace(MirLocalId LocalId, MirType Type) : MirPlace(Type)
{
    public MirLocalPlace(MirLocal local)
        : this(local.Id, local.Type) { }
}

internal sealed record MirGlobalPlace(MirGlobalId GlobalId, MirType Type) : MirPlace(Type);

internal sealed record MirDerefPlace(MirValue Pointer, MirType Type) : MirPlace(Type);

internal sealed record MirFieldPlace(MirPlace Base, int FieldIndex, MirType Type) : MirPlace(Type);

internal sealed record MirIndexPlace(MirPlace Base, MirValue Index, MirType Type) : MirPlace(Type);
