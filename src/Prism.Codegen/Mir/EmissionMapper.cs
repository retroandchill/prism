// @file EmissionMapper.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Mir;
using Riok.Mapperly.Abstractions;

namespace Prism.Codegen.Mir;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
internal static partial class EmissionMapper
{
    [MapperIgnoreSourceValue(BinaryOperation.LogicalAnd)]
    [MapperIgnoreSourceValue(BinaryOperation.LogicalOr)]
    public static partial BinaryOperator ToBinaryOperator(this BinaryOperation operation);
}
