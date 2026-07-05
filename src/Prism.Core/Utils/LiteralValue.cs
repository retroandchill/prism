// @file LiteralValue.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Prism.Core.Utils;

[Union]
public readonly struct LiteralValue : IUnion
{
    private enum Type
    {
        Bool,
        Integer,
        Float,
        String,
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly struct UnmanagedUnion
    {
        [field: FieldOffset(0)]
        public bool BoolValue { get; init; }

        [field: FieldOffset(0)]
        public UInt128 IntegerValue { get; init; }

        [field: FieldOffset(0)]
        public double FloatValue { get; init; }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Type _type;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly UnmanagedUnion _unmanagedValues;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string? _stringValue;

    public LiteralValue(bool value)
    {
        _type = Type.Bool;
        _unmanagedValues = new UnmanagedUnion { BoolValue = value };
    }

    public LiteralValue(UInt128 value)
    {
        _type = Type.Integer;
        _unmanagedValues = new UnmanagedUnion { IntegerValue = value };
    }

    public LiteralValue(double value)
    {
        _type = Type.Integer;
        _unmanagedValues = new UnmanagedUnion { FloatValue = value };
    }

    public LiteralValue(string value)
    {
        _type = Type.Integer;
        _stringValue = value;
    }

    public object? Value
    {
        get
        {
            return _type switch
            {
                Type.Bool => _unmanagedValues.BoolValue,
                Type.Integer => _unmanagedValues.IntegerValue,
                Type.Float => _unmanagedValues.FloatValue,
                Type.String => _stringValue,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public bool TryGetValue(out bool value)
    {
        if (_type == Type.Bool)
        {
            value = _unmanagedValues.BoolValue;
            return true;
        }

        value = false;
        return false;
    }

    public bool TryGetValue(out UInt128 value)
    {
        if (_type == Type.Integer)
        {
            value = _unmanagedValues.IntegerValue;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out double value)
    {
        if (_type == Type.Float)
        {
            value = _unmanagedValues.FloatValue;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out string? value)
    {
        if (_type == Type.String)
        {
            value = _stringValue;
            return true;
        }

        value = null;
        return false;
    }
}
