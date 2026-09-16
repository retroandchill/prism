// @file DwarfTypeEncoding.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Codegen;

internal static class DwarfTypeEncoding
{
    public const uint Address = 0x01;
    public const uint Boolean = 0x02;
    public const uint Float = 0x04;
    public const uint Signed = 0x05;
    public const uint Unsigned = 0x07;
    public const uint UTF = 0x10;
}
