// @file InitializerValue.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using Prism.Constants;

namespace Prism.Mir;

public readonly union InitializerValue(ConstantValue, ExecutableBody)
    : IEquatable<InitializerValue>,
        IEqualityOperators<InitializerValue, InitializerValue, bool>
{
    public override bool Equals(object? obj)
    {
        return obj is InitializerValue other && Equals(other);
    }

    public bool Equals(InitializerValue other)
    {
        return Equals(Value, other.Value);
    }

    public override int GetHashCode()
    {
        return Value != null ? Value.GetHashCode() : 0;
    }

    public static bool operator ==(InitializerValue left, InitializerValue right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(InitializerValue left, InitializerValue right)
    {
        return !left.Equals(right);
    }
}
