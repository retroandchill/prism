// @file TokenType.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Metadata;

public enum TokenType : uint
{
    Assembly = 0x00000000,
    AssemblyRef = 0x01000000,
    TypeRef = 0x02000000,
    TypeDef = 0x03000000,
    Variable = 0x04000000,
    Function = 0x05000000,
    Parameter = 0x06000000,

    String = 0x70000000,
}
