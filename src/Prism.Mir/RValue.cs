// @file RValue.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Mir;

public closed record RValue;

public sealed record UseRValue(Operand Operand) : RValue;

public sealed record UnaryRValue(UnaryOperator Operator, Operand Operand) : RValue;

public sealed record BinaryRValue(BinaryOperator Operator, Operand Left, Operand Right) : RValue;
