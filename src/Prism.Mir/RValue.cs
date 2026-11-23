// @file RValue.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
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

    public UseRValue WithOperand(Operand operand)
    {
        return operand == Operand ? this : new UseRValue(operand);
    }
}

public sealed record UnaryRValue(UnaryOperator Operator, Operand Operand) : RValue
{
    public override string ToString() => $"{Operator}({Operand})";

    public UnaryRValue WithOperand(Operand operand)
    {
        return operand == Operand ? this : this with { Operand = operand };
    }
}

public sealed record BinaryRValue(BinaryOperator Operator, Operand Left, Operand Right) : RValue
{
    public override string ToString() => $"{Operator}({Left}, {Right})";

    public BinaryRValue WithOperands(Operand left, Operand right)
    {
        return left == Left && right == Right ? this : this with { Left = left, Right = right };
    }
}

public sealed record IsNullRValue(Operand Operand) : RValue
{
    public override string ToString() => $"IsNull({Operand})";

    public IsNullRValue WithOperand(Operand operand)
    {
        return operand == Operand ? this : new IsNullRValue(operand);
    }
}

public sealed record IsNotNullRValue(Operand Operand) : RValue
{
    public override string ToString() => $"IsNotNull({Operand})";

    public IsNotNullRValue WithOperand(Operand operand)
    {
        return operand == Operand ? this : new IsNotNullRValue(operand);
    }
}

public sealed record SelectRValue(Operand Condition, Operand TrueValue, Operand FalseValue) : RValue
{
    public override string ToString() => $"Select({Condition}, {TrueValue}, {FalseValue})";

    public SelectRValue WithOperands(Operand condition, Operand trueValue, Operand falseValue)
    {
        return condition == Condition && trueValue == TrueValue && falseValue == FalseValue
            ? this
            : new SelectRValue(condition, trueValue, falseValue);
    }
}

public sealed record ConversionRValue(Operand Operand, Conversion Conversion, TypeSymbol TargetType)
    : RValue
{
    public override string ToString() => $"Cast({Operand}, {TargetType})";

    public ConversionRValue WithOperand(Operand operand)
    {
        return operand == Operand ? this : this with { Operand = operand };
    }
}

public sealed record ReferenceRValue(Place Place, bool IsMutable) : RValue
{
    public override string ToString() => $"Ref({(IsMutable ? "mutable" : "")}{Place})";

    public ReferenceRValue WithPlace(Place place)
    {
        return place == Place ? this : this with { Place = place };
    }
}

public sealed record ArrayRValue(ImmutableArray<Operand> Elements) : RValue
{
    public override string ToString() => $"[{string.Join(", ", Elements)}]";

    public ArrayRValue WithElements(ImmutableArray<Operand> elements)
    {
        return elements == Elements ? this : new ArrayRValue(elements);
    }
}

public sealed record DataRValue(Operand Operand) : RValue
{
    public override string ToString() => $"Data({Operand})";

    public DataRValue WithOperand(Operand operand)
    {
        return operand == Operand ? this : new DataRValue(operand);
    }
}

public sealed record LengthRValue(Operand Operand) : RValue
{
    public override string ToString() => $"Length({Operand})";

    public LengthRValue WithOperand(Operand operand)
    {
        return operand == Operand ? this : new LengthRValue(operand);
    }
}
