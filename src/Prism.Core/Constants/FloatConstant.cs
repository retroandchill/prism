// @file FloatConstant.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using MagicArchive;

namespace Prism.Core.Constants;

internal enum FloatKind : byte
{
    F32,
    F64,
}

[Archivable(GenerateType.Custom)]
[Union]
public readonly partial struct FloatConstant : IUnion
{
    private readonly FloatKind _kind;
    private readonly double _value;

    private FloatConstant(FloatKind kind, double value)
    {
        _kind = kind;
        _value = value;
    }

    public FloatConstant(float value)
        : this(FloatKind.F32, value) { }

    public FloatConstant(double value)
        : this(FloatKind.F64, value) { }

    public object Value
    {
        get
        {
            return _kind switch
            {
                FloatKind.F32 => (float)_value,
                FloatKind.F64 => _value,
                _ => throw new InvalidOperationException("Invalid float kind"),
            };
        }
    }

    public bool TryGetValue(out float value)
    {
        if (_kind == FloatKind.F32)
        {
            value = (float)_value;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out double value)
    {
        if (_kind == FloatKind.F64)
        {
            value = _value;
            return true;
        }

        value = 0;
        return false;
    }

    static void IArchivable<FloatConstant>.Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in FloatConstant value
    )
    {
        switch (value._kind)
        {
            case FloatKind.F32:
                writer.WriteBlittable(value._kind, (float)value._value);
                break;
            case FloatKind.F64:
                writer.WriteBlittable(value._kind, value._value);
                break;
            default:
                throw new ArchiveSerializationException("Invalid float kind");
        }
    }

    static void IArchivable<FloatConstant>.Deserialize(
        ref ArchiveReader reader,
        scoped ref FloatConstant value
    )
    {
        var kind = reader.ReadBlittable<FloatKind>();
        value = kind switch
        {
            FloatKind.F32 => reader.ReadBlittable<float>(),
            FloatKind.F64 => reader.ReadBlittable<double>(),
            _ => throw new ArchiveSerializationException("Invalid float kind"),
        };
    }
}
