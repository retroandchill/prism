// @file MirValue.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Semantic;
using Prism.Core.Symbols;

namespace Prism.Core.Mir;

internal abstract record MirValue(TypeSymbol Type);

internal sealed record MirConstantValue(ConstantValue Constant, TypeSymbol Type) : MirValue(Type);

internal sealed record MirReadValue(MirPlace Place) : MirValue(Place.Type);

internal sealed record MirAddressOfValue(MirPlace Place) : MirValue(Place.Type);

internal sealed record MirNullValue(TypeSymbol Type) : MirValue(Type);
