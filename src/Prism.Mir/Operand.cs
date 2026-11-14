// @file Operand.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Constants;

namespace Prism.Mir;

public closed class Operand
{
    public abstract override string ToString();
}

public sealed record ConstantOperand(ConstantValue Value)
{
    public override string ToString() => Value.ToString();
}

public sealed record CopyOperand(Place Source)
{
    public override string ToString() => $"Copy({Source})";
}

public sealed record MoveOperand(Place Source)
{
    public override string ToString() => $"Move({Source})";
}
