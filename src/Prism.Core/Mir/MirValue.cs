// @file MirValue.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Semantic;
using Prism.Core.Symbols;

namespace Prism.Core.Mir;

internal readonly record struct MirValueId(int Id);

internal closed record MirValue(TypeSymbol Type);

internal sealed record MirConstantValue(ConstantValue Constant, TypeSymbol Type) : MirValue(Type);

internal sealed record MirNullValue(TypeSymbol Type) : MirValue(Type);

internal sealed record MirSsaValue(MirValueId Id, TypeSymbol Type) : MirValue(Type);

internal sealed record MirAddressOfValue(MirPlace Place, TypeSymbol Type) : MirValue(Type);

internal sealed record MirVoidValue(TypeSymbol Type) : MirValue(Type);
