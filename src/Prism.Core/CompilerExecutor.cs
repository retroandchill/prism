// @file Compiler.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Parse;
using Prism.Core.Semantic;

namespace Prism.Core;

public sealed class CompilerExecutor(TargetPlatform? targetPlatform = null)
{
    private readonly TargetPlatform _targetPlatform = targetPlatform ?? TargetPlatform.Default;

    public async Task<Compilation> CompileAsync(
        IEnumerable<FileInfo> files,
        CancellationToken cancellationToken = default
    )
    {
        var compilation = new Compilation(_targetPlatform);
        var compileResults = await Task.WhenAll(
            files.Select(file => Task.Run(() => ParseFileAsync(file, compilation, cancellationToken), cancellationToken))
        );
        
        await Task.WhenAll(compileResults.Select(result => Task.Run(() => BindSyntaxAsync(result, compilation, cancellationToken), cancellationToken)));
        
        return compilation;
    }

    private static async Task<SourceUnit> ParseFileAsync(
        FileInfo file,
        Compilation compilation,
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
        compilation.SemanticModel.AddCompilationUnit(unit.Syntax);

        return unit;
    }

    private static async Task BindSyntaxAsync(SourceUnit unit, Compilation compilation, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
