// @file ConstantStorage.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using MagicArchive;

namespace Prism.Core.Constants;

internal readonly struct ConstantStorage
{
    [ExtendedLayout(ExtendedLayoutKind.CUnion)]
    internal struct UnmanagedStorage
    {
        public bool BoolValue;

        public UInt128 IntegerValue;

        public float FloatValue;

        public double DoubleValue;

        public Rune CharacterValue;
    }

    private readonly UnmanagedStorage _unmanagedStorage;
    public object ReferenceValue { get; } = null!;

    public ConstantStorage(string value)
    {
        ReferenceValue = value;
    }

    public ConstantStorage(bool value)
    {
        _unmanagedStorage.BoolValue = value;
    }

    public ConstantStorage(Rune value)
    {
        _unmanagedStorage.CharacterValue = value;
    }

    public ConstantStorage(UInt128 value)
    {
        _unmanagedStorage.IntegerValue = value;
    }

    public ConstantStorage(float value)
    {
        _unmanagedStorage.FloatValue = value;
    }

    public ConstantStorage(double value)
    {
        _unmanagedStorage.DoubleValue = value;
    }

    public bool BoolValue => _unmanagedStorage.BoolValue;

    public Rune CharacterValue => _unmanagedStorage.CharacterValue;

    public UInt128 IntegerValue => _unmanagedStorage.IntegerValue;

    public float FloatValue => _unmanagedStorage.FloatValue;

    public double DoubleValue => _unmanagedStorage.DoubleValue;
}
