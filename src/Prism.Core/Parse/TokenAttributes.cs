// @file TokenAttributes.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Parse;

[AttributeUsage(AttributeTargets.Enum)]
internal sealed class TokenKindAttribute : Attribute;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class KeywordTokenAttribute : Attribute;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class PunctuationTokenAttribute : Attribute;
