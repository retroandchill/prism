// @file SymbolToMetadataMapper.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Metadata;
using Prism.Core.Symbols;
using Riok.Mapperly.Abstractions;

namespace Prism.Core.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class SymbolToMetadataMapper
{
    [MapDerivedType<ConstantParameterDefault, ConstantParameterDefaultDefinition>]
    public static partial ParameterDefaultDefinition ToMetadata(this ParameterDefault value);

    private static partial ConstantParameterDefaultDefinition ToMetadata(
        this ConstantParameterDefault value
    );
}
