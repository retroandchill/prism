// @file VariableInitializerBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Constants;
using Prism.Semantic.Symbols;
using Riok.Mapperly.Abstractions;

namespace Prism.Mir;

public sealed class VariableInitializerBuilder
{
    public VariableSymbol Symbol { get; }

    private bool _hasValidBody;

    internal ConstantValue ConstantValue { get; set; }

    internal ExecutableBodyBuilder? ExecutableBody { get; private set; }

    public VariableInitializerBuilder(VariableSymbol symbol)
    {
        Symbol = symbol;
    }

    public void WithConstantValue(in ConstantValue constantValue)
    {
        ConstantValue = constantValue;
        ExecutableBody = null;
        _hasValidBody = true;
    }

    public ExecutableBodyBuilder WithExecutableBody()
    {
        ExecutableBody = new ExecutableBodyBuilder();
        ExecutableBody.SetReturnValueLocal(Symbol.Type);

        ConstantValue = default;
        _hasValidBody = true;

        return ExecutableBody;
    }

    public VariableInitializer Build()
    {
        if (!_hasValidBody)
            throw new InvalidOperationException(
                "Cannot build variable initializer without executable body or constant value."
            );

        var initializer = VariableInitializer.Mapper.Map(this);
        initializer.Validate();
        return initializer;
    }
}
