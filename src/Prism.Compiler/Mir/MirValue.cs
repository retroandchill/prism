// @file MirValue.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Compiler.Semantic;
using Prism.Compiler.Symbols;
using Prism.Constants;
using ConstantValue = Prism.Constants.ConstantValue;

namespace Prism.Compiler.Mir;

internal readonly record struct MirValueId(int Id);

internal closed record MirValue(TypeSymbol Type);

internal sealed record MirConstantValue(ConstantValue Constant, TypeSymbol Type) : MirValue(Type)
{
    public bool IsNull => Constant is NullConstant;
}

internal sealed record MirSsaValue(MirValueId Id, TypeSymbol Type) : MirValue(Type);

internal sealed record MirAddressOfValue(MirPlace Place, TypeSymbol Type) : MirValue(Type);

internal sealed record MirVoidValue(TypeSymbol Type) : MirValue(Type);
