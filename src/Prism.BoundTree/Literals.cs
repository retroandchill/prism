// @file Literals.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using Prism.Utils;

namespace Prism.BoundTree;

public readonly record struct RawIntegerLiteralData(
    BigInteger Value,
    IntegerBase Base = IntegerBase.Decimal,
    IntegerSuffix Suffix = IntegerSuffix.None
);

public readonly record struct RawFloatLiteralData(
    BigInteger Significand,
    int Exponent10 = 0,
    FloatSuffix Suffix = FloatSuffix.None
);
