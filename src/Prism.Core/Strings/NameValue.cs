// @file NameValue.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Strings;

internal readonly ref struct NameValue(ReadOnlySpan<char> name, NameHash hash)
{
    public ReadOnlySpan<char> Name { get; } = name;
    public NameHash Hash { get; } = hash;

    public NameValue(ReadOnlySpan<char> name)
        : this(name, new NameHash(name)) { }
}
