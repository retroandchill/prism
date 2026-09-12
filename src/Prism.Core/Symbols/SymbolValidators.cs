// @file SymbolValidators.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Symbols;

internal static class SymbolValidators
{
    extension(VariableSymbol variable)
    {
        public VariableSymbol EnsureLocal()
        {
            return variable.IsLocal
                ? variable
                : throw new ArgumentException("Variable is not local", nameof(variable));
        }

        public VariableSymbol EnsureGlobal()
        {
            return variable.IsGlobal
                ? variable
                : throw new ArgumentException("Variable is not global", nameof(variable));
        }
    }
}
