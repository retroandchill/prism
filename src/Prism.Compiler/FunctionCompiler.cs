// @file FunctionCompiler.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Compiler.Binding;
using Prism.Compiler.Codegen;
using Prism.Compiler.Symbols;
using Prism.Compiler.Symbols.Synthesized;
using Prism.Diagnostics;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using ZLinq;

namespace Prism.Compiler;

internal sealed class FunctionCompiler
{
    private readonly Compilation _compilation;
    private readonly IMirEmitter? _emitter;
    private readonly BindingContext _context;
    private readonly CancellationToken _cancellationToken;

    private FunctionCompiler(
        Compilation compilation,
        IMirEmitter? emitter,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        _compilation = compilation;
        _emitter = emitter;
        _context = context;
        _cancellationToken = cancellationToken;
    }

    public static void CompileFunctions(
        Compilation compilation,
        IMirEmitter? emitter,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var compiler = new FunctionCompiler(compilation, emitter, context, cancellationToken);
        compiler.CompileContainer(compilation.Assembly.GlobalNamespace);
        compiler.AddEntryPoint();
    }

    private void CompileContainer(MemberContainerSymbol container)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        foreach (var member in container.GetMembers())
        {
            switch (member)
            {
                case VariableSymbol variableSymbol:
                    CompileVariable(variableSymbol);
                    break;
                case FunctionSymbol functionSymbol:
                    CompileFunction(functionSymbol);
                    break;
                case MemberContainerSymbol memberContainerSymbol:
                    CompileContainer(memberContainerSymbol);
                    break;
            }
        }
    }

    private void CompileVariable(VariableSymbol variableSymbol)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var initializer = _compilation.GetBoundInitializer(variableSymbol, _cancellationToken);
        _context.ReportDiagnostics(initializer.Diagnostics);
        if (initializer.Initializer is null)
            return;

        _emitter?.RegisterGlobalVariable(initializer, _cancellationToken);
    }

    private void CompileFunction(FunctionSymbol functionSymbol)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var body = _compilation.GetBoundBody(functionSymbol, _cancellationToken);
        _context.ReportDiagnostics(body.Diagnostics);
        if (body.Body is null)
            return;

        if (functionSymbol.Data is SynthesizedGlobalConstructor globalCtor)
        {
            using var globals = _compilation
                .GetGlobalVariables()
                .AsValueEnumerable()
                .Select(v => _compilation.GetBoundInitializer(v, _cancellationToken))
                .ToArrayPool();
            if (_emitter is null || !globalCtor.ShouldEmit(globals.Span))
                return;

            _emitter.EmitFunctionBody(body, _cancellationToken);
        }
        else
        {
            _emitter?.EmitFunctionBody(body, _cancellationToken);
        }
    }

    private void AddEntryPoint()
    {
        var (entryPoint, entryPointDiagnostics) = _compilation.GetEntryPointAndDiagnostics();
        _context.ReportDiagnostics(entryPointDiagnostics);
        if (entryPoint is null)
            return;
        if (!entryPointDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
        {
            _emitter?.RegisterEntryPoint(entryPoint);
        }
    }
}
