// @file MirLocal.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Mir;

internal readonly record struct MirLocalId(int Value);

internal enum MirLocalKind : byte
{
    Parameter,
    User,
    Temporary,
    Return,
}

internal class MirLocal
{
    public MirLocal(MirLocalId id, string name, MirType type, MirLocalKind kind, bool isMutable)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(type);

        Id = id;
        Name = name;
        Type = type;
        Kind = kind;
        IsMutable = isMutable;
    }

    public MirLocalId Id { get; }

    public string Name { get; }

    public MirType Type { get; }

    public MirLocalKind Kind { get; }

    public bool IsMutable { get; }
}
