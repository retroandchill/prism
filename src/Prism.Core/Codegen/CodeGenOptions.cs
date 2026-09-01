// @file CodeGenOptions.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Codegen;

public sealed record CodeGenOptions
{
    public required string OutputDirectory { get; init; }
}
