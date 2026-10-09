// @file OverloadSignature.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Core.Symbols;

internal enum DispatchSpecifier
{
    None,
    Mutable,
    Value,
}

internal readonly record struct OverloadSignature(
    ImmutableArray<TypeSymbol> Parameters,
    DispatchSpecifier Specifier
)
{
    public static OverloadSignature Create(FunctionSymbol function)
    {
        return new OverloadSignature(
            function.ParameterTypes,
            function.ReceiverType switch
            {
                ReferenceTypeSymbol { IsMutable: var mutable } => mutable
                    ? DispatchSpecifier.Mutable
                    : DispatchSpecifier.None,
                not null => DispatchSpecifier.Value,
                null => DispatchSpecifier.None,
            }
        );
    }

    public bool Equals(OverloadSignature other)
    {
        return Specifier == other.Specifier && Parameters.SequenceEqual(other.Parameters);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Specifier);

        foreach (var type in Parameters)
        {
            hashCode.Add(type);
        }

        return hashCode.ToHashCode();
    }
}
