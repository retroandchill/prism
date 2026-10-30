// @file HandleAttribute.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;

namespace Prism.Core.Metadata;

[AttributeUsage(AttributeTargets.Struct)]
internal sealed class HandleAttribute : Attribute
{
    public bool AllowZero { get; init; }
}

[AttributeUsage(AttributeTargets.Struct)]
internal sealed class CompositeHandleAttribute(params Type[] types) : Attribute;
