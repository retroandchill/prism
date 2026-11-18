// @file Operand.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Prism.Constants;
using Prism.Semantic.Symbols;

namespace Prism.Mir;

public closed record Operand
{
    internal abstract TypeSymbol GetTypeSymbol();

    public abstract override string ToString();
}

public sealed record ConstantOperand(ConstantValue Value, TypeSymbol Type) : Operand
{
    public bool IsNull => Value is NullConstant;

    internal override TypeSymbol GetTypeSymbol() => Type;
}

public closed record PlaceOperand(Place Source) : Operand
{
    internal override TypeSymbol GetTypeSymbol() => Source.Type;
}

public sealed record CopyOperand(Place Source) : PlaceOperand(Source)
{
    public override string ToString() => $"Copy({Source})";
}

public sealed record MoveOperand(Place Source) : PlaceOperand(Source)
{
    public override string ToString() => $"Move({Source})";
}

public static class OperandExtensions
{
    extension(Operand operand)
    {
        public TypeSymbol Type => operand.GetTypeSymbol();
    }
}
