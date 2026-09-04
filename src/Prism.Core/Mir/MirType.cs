// @file MirType.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Mir;

internal abstract record MirType;

internal sealed record MirIntType(int Bits, bool Signed) : MirType;

internal sealed record MirFloatType(int Bits) : MirType;

internal sealed record MirBoolType : MirType
{
    private MirBoolType() { }

    public static readonly MirBoolType Instance = new();
}

internal sealed record MirPointerType(MirType Pointee, bool IsMutable) : MirType;

internal sealed record MirSliceType(MirType Element, bool IsMutable) : MirType;

internal sealed record MirArrayType(MirType ElementType, ulong Length) : MirType;

internal sealed record MirStructType(string Name, IReadOnlyList<MirType> Fields) : MirType;

internal sealed record MirVoidType : MirType
{
    private MirVoidType() { }

    public static readonly MirVoidType Instance = new();
}
