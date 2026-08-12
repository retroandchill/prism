// @file IGreenNodeWrapper.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Syntax.Green;

internal interface IGreenNodeWrapper
{
    GreenNode Node { get; }
}

internal interface INullableGreenNodeWrapper
{
    GreenNode? Node { get; }
}
