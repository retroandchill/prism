// @file LiteralMapper.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.BoundTree;
using Prism.Syntax;
using Riok.Mapperly.Abstractions;

namespace Prism.Compiler.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class LiteralMapper
{
    public static partial RawIntegerLiteralData ToRaw(this IntegerLiteralData data);

    public static partial RawFloatLiteralData ToRaw(this FloatLiteralData data);
}
