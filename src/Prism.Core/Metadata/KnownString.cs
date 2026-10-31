// @file KnownStrings.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Frozen;
using System.Runtime.Serialization;
using NetEscapades.EnumGenerators;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Metadata;

[EnumExtensions(ExtensionClassName = nameof(KnownStrings))]
internal enum KnownString
{
    NullHandle,

    [EnumMember(Value = "")]
    Empty,

    [EnumMember(Value = CommonNames.Ctor)]
    Ctor,

    [EnumMember(Value = CommonNames.Main)]
    Main,
}

internal static partial class KnownStrings
{
    public const int HandleShift = 512;

    private static readonly FrozenDictionary<string, KnownString> KnownHandlesMap;

    extension(KnownString value)
    {
        public StringHandle ToHandle() => StringHandle.FromRowId(value.AsUnderlyingType());

        public static KnownString? TryGet(string str)
        {
            return KnownHandlesMap.TryGetValue(str, out var result) ? result : null;
        }
    }

    static KnownStrings()
    {
        KnownHandlesMap = KnownString
            .GetValues()
            .AsValueEnumerable()
            .Skip(1)
            .ToFrozenDictionary(x => x.ToStringFastWithMetadata());
    }
}
