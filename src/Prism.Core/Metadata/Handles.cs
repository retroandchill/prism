// @file Handles.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;

namespace Prism.Core.Metadata;

[Archivable]
public readonly partial record struct StringHandle(int Offset);
