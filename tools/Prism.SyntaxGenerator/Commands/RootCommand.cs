// @file RootCommand.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using DotMake.CommandLine;

namespace Prism.SyntaxGenerator.Commands;

[CliCommand(
    Description = "Generate Prism syntax",
    Children = [typeof(ExportSchemaCommand), typeof(ExportNodesCommand)]
)]
public class RootCommand { }
