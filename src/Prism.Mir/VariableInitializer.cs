// @file VariableInitializer.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Constants;
using Prism.Semantic.Symbols;
using Riok.Mapperly.Abstractions;

namespace Prism.Mir;

public sealed partial class VariableInitializer
{
    public VariableSymbol Symbol { get; }

    public InitializerValue Value { get; private set; }

    private VariableInitializer(VariableSymbol symbol)
    {
        Symbol = symbol;
    }

    public VariableInitializer WithInitializer(InitializerValue value)
    {
        if (Value == value)
            return this;

        return new VariableInitializer(Symbol) { Value = value };
    }

    internal void Validate()
    {
        switch (Value)
        {
            case ConstantValue:
                // Do nothing, a constant value is valid.
                break;
            case ExecutableBody executableBody:
                executableBody.Validate();
                break;
        }
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    [UseStaticMapper(typeof(ExecutableBody.Mapper))]
    internal static partial class Mapper
    {
        [MapPropertyFromSource(nameof(Value), Use = nameof(MapInitializerValue))]
        public static partial VariableInitializer Map(VariableInitializerBuilder builder);

        private static InitializerValue MapInitializerValue(VariableInitializerBuilder builder)
        {
            if (builder.ExecutableBody is not null)
            {
                return ExecutableBody.Mapper.Map(builder.ExecutableBody);
            }

            return builder.ConstantValue;
        }
    }
}
