// @file SourceLocationData.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;
using Prism.Text;

namespace Prism.Metadata.BoundTree.Serialized;

[Archivable]
internal readonly partial record struct SyntaxLocationData(StringHandle File, TextSpan SourceSpan);
