// @file ParameterAttributes.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;

namespace Prism.Core.Metadata;

[Archivable]
internal readonly partial record struct ParameterAttributes
{
    [ArchiveInclude]
    private readonly uint _value;

    private ParameterAttributes(uint value)
    {
        _value = value;
    }

    public static ParameterAttributes Create()
    {
        // Currently there are not major attributes that need to be stored,
        // but for the sake of reserving space for future features, we will store a 0 in this slot.
        return new ParameterAttributes(0);
    }
}
