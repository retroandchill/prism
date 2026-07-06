// @file Compiler.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Ast;
using Prism.Core.Parse;
using Prism.Core.Semantic;

namespace Prism.Core;

public sealed class CompilerExecutor(TargetPlatform? targetPlatform = null)
{
    private readonly SemanticModel _semanticModel = new();
    private readonly TargetPlatform _targetPlatform = targetPlatform ?? TargetPlatform.Default;

    public async Task CompileAsync(
        IEnumerable<FileInfo> files,
        CancellationToken cancellationToken = default
    )
    {
        var compileResults = await Task.WhenAll(
            files.Select(file => ParseFileAsync(file, cancellationToken))
        );
    }

    private async Task<SourceUnit> ParseFileAsync(
        FileInfo file,
        CancellationToken cancellationToken = default
    )
    {
        string fileText;
        using (var fileStream = file.OpenText())
        {
            fileText = await fileStream.ReadToEndAsync(cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var document = new SourceDocument(fileText);
        var parser = new Parser(document);

        cancellationToken.ThrowIfCancellationRequested();
        var unit = parser.ParseCompilationUnit();

        cancellationToken.ThrowIfCancellationRequested();
        _semanticModel.AddCompilationUnit(unit.Syntax);

        return unit;
    }
}
