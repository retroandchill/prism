// @file OverloadSignature.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Core.Symbols;

internal readonly record struct OverloadSignature(ImmutableArray<TypeSymbol> Parameters)
{
    public static OverloadSignature Create(FunctionSymbol function)
    {
        return new OverloadSignature(function.ParameterTypes);
    }

    public bool Equals(OverloadSignature other)
    {
        return Parameters.SequenceEqual(other.Parameters);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        foreach (var type in Parameters)
        {
            hashCode.Add(type);
        }

        return hashCode.ToHashCode();
    }
}
