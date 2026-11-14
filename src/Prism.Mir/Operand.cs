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

    public static ConstantOperand Create(in ConstantValue value)
    {
        return value switch
        {
            NullConstant => Null,
            bool b => b ? True : False,
            CharacterConstant => new ConstantOperand(in value),
            IntegerConstant i => i switch
            {
                { IsZero: true } => IntegerZeros[i.Kind.ToRaw()],
                { IsOne: true } => IntegerOnes[i.Kind.ToRaw()],
                _ => new ConstantOperand(in value),
            },
            FloatConstant f => f switch
            {
                { IsZero: true } => IntegerZeros[f.Kind.AsUnderlyingType()],
                { IsOne: true } => IntegerOnes[f.Kind.AsUnderlyingType()],
                _ => new ConstantOperand(in value),
            },
            string str => str.Length == 0 ? EmptyString : new ConstantOperand(in value),
        };
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
            integerZeros[i] = new ConstantOperand(IntegerConstant.From(kind, 0));
            integerOnes[i] = new ConstantOperand(IntegerConstant.From(kind, 1));
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

public sealed record CopyOperand(Place Source) : Operand
{
    public override string ToString() => $"Copy({Source})";
}

public sealed record MoveOperand(Place Source) : Operand
{
    public override string ToString() => $"Move({Source})";
}
