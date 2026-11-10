// @file SymbolToMetadataMapper.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Compiler.Symbols;
using Prism.Metadata;
using Riok.Mapperly.Abstractions;

namespace Prism.Compiler.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class SymbolToMetadataMapper
{
    [MapDerivedType<ConstantParameterDefault, ConstantParameterDefaultDefinition>]
    public static partial ParameterDefaultDefinition ToMetadata(this ParameterDefault value);

    private static partial ConstantParameterDefaultDefinition ToMetadata(
        this ConstantParameterDefault value
    );

    [MapDerivedType<ConstantParameterDefaultDefinition, ConstantParameterDefault>]
    public static partial ParameterDefault ToSymbol(this ParameterDefaultDefinition value);

    [MapperIgnoreTarget(nameof(ConstantParameterDefault.Syntax))]
    private static partial ConstantParameterDefault ToSymbol(
        this ConstantParameterDefaultDefinition value
    );
}
