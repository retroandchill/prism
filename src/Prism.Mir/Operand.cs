// @file Operand.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Prism.Constants;

namespace Prism.Mir;

public closed record Operand
{
    public abstract override string ToString();
}

public sealed record ConstantOperand : Operand
{
    public static readonly ConstantOperand Null = new(new NullConstant());
    public static readonly ConstantOperand True = new(true);
    public static readonly ConstantOperand False = new(false);
    public static readonly ConstantOperand EmptyString = new("");

    private static readonly ImmutableArray<ConstantOperand> IntegerZeros;
    private static readonly ImmutableArray<ConstantOperand> IntegerOnes;
    private static readonly ImmutableArray<ConstantOperand> FloatZeros;
    private static readonly ImmutableArray<ConstantOperand> FloatOnes;

    private ConstantOperand(in ConstantValue value)
    {
        Value = value;
    }

    public ConstantValue Value { get; init; }

    public bool IsNull => Value is NullConstant;

    public static ConstantOperand Create(in ConstantValue value)
    {
        return value switch
        {
            NullConstant => Null,
            bool b => b ? True : False,
            CharacterConstant => new ConstantOperand(in value),
            IntegerConstant i => i switch
            {
                { IsZero: true } => Zero(i.Kind),
                { IsOne: true } => One(i.Kind),
                _ => new ConstantOperand(in value),
            },
            FloatConstant f => f switch
            {
                { IsZero: true } => Zero(f.Kind),
                { IsOne: true } => One(f.Kind),
                _ => new ConstantOperand(in value),
            },
            string str => str.Length == 0 ? EmptyString : new ConstantOperand(in value),
        };
    }

    public static ConstantOperand Zero(IntegerKind kind)
    {
        return IntegerZeros[kind.ToRaw()];
    }

    public static ConstantOperand Zero(FloatKind kind)
    {
        return FloatZeros[kind.AsUnderlyingType()];
    }

    public static ConstantOperand One(IntegerKind kind)
    {
        return IntegerOnes[kind.ToRaw()];
    }

    public static ConstantOperand One(FloatKind kind)
    {
        return FloatOnes[kind.AsUnderlyingType()];
    }

    public void Deconstruct(out ConstantValue value)
    {
        value = Value;
    }

    public override string ToString() => Value.ToString();

    static ConstantOperand()
    {
        var integerZeros = new ConstantOperand[IntegerKind.MaxValue + 1];
        var integerOnes = new ConstantOperand[IntegerKind.MaxValue + 1];

        for (byte i = 0; i < IntegerKind.MaxValue + 1; i++)
        {
            var kind = IntegerKind.FromRaw(i);
            integerZeros[i] = new ConstantOperand(IntegerConstant.Zero(kind));
            integerOnes[i] = new ConstantOperand(IntegerConstant.One(kind));
        }

        IntegerZeros = ImmutableCollectionsMarshal.AsImmutableArray(integerZeros);
        IntegerOnes = ImmutableCollectionsMarshal.AsImmutableArray(integerOnes);

        FloatZeros =
        [
            new ConstantOperand(new FloatConstant(0.0F)),
            new ConstantOperand(new FloatConstant(0.0D)),
        ];
        FloatOnes =
        [
            new ConstantOperand(new FloatConstant(1.0F)),
            new ConstantOperand(new FloatConstant(1.0D)),
        ];
    }
}

public closed record PlaceOperand(Place Source) : Operand;

public sealed record CopyOperand(Place Source) : PlaceOperand(Source)
{
    public override string ToString() => $"Copy({Source})";
}

public sealed record MoveOperand(Place Source) : PlaceOperand(Source)
{
    public override string ToString() => $"Move({Source})";
}
