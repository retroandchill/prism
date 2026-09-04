// @file MirGlobal.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Mir;

internal readonly record struct MirGlobalId(int Value);

internal sealed class MirGlobal
{
    public MirGlobal(
        MirGlobalId id,
        string name,
        MirType type,
        bool isMutable,
        MirGlobalInitializer initializer
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(initializer);

        Id = id;
        Name = name;
        Type = type;
        IsMutable = isMutable;
        Initializer = initializer;
    }

    public MirGlobalId Id { get; }

    public string Name { get; }

    public MirType Type { get; }

    public bool IsMutable { get; }

    public MirGlobalInitializer Initializer { get; }
}
