// @file TokenInfo.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Parse;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;

namespace Prism.SourceGenerator.Tokens;

[AttributeInfoType<KeywordTokenAttribute>]
public readonly record struct KeywordTokenInfo(string Token);

[AttributeInfoType<PunctuationTokenAttribute>]
public readonly record struct PunctuationTokenInfo(string Token);
