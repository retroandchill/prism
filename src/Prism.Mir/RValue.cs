// @file RValue.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Semantic;
using Prism.Semantic.Conversions;
using Prism.Semantic.Symbols;

namespace Prism.Mir;

public closed record RValue
{
    public abstract override string ToString();
}

public sealed record UseRValue(Operand Operand) : RValue
{
    public override string ToString() => Operand.ToString();
}

public sealed record UnaryRValue(UnaryOperator Operator, Operand Operand) : RValue
{
    public override string ToString() => $"{Operator}({Operand})";
}

public sealed record BinaryRValue(BinaryOperator Operator, Operand Left, Operand Right) : RValue
{
    public override string ToString() => $"{Operator}({Left}, {Right})";
}

public sealed record SelectRValue(Operand Condition, Operand TrueValue, Operand FalseValue) : RValue
{
    public override string ToString() => $"Select({Condition}, {TrueValue}, {FalseValue})";
}

public sealed record ConversionRValue(Operand Operand, Conversion Conversion, TypeSymbol TargetType)
    : RValue
{
    public override string ToString() => $"Cast({Operand})";
}

public sealed record ReferenceRValue(Operand Operand, bool IsMutable) : RValue
{
    public override string ToString() => $"Ref({(IsMutable ? "mutable" : "")}{Operand})";
}
