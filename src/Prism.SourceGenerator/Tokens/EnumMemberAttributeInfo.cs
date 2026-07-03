// @file EnumMemberAttributeInfo.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;

namespace Prism.SourceGenerator.Tokens;

[AttributeInfoType<EnumMemberAttribute>]
public readonly record struct EnumMemberAttributeInfo
{
    public string? Value { get; init; }
}
