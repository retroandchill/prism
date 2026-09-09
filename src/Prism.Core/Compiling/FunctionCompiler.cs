// @file FunctionCompiler.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Prism.Core.Binding;
using Prism.Core.Codegen;
using Prism.Core.Diagnostics;
using Prism.Core.Symbols;
using Prism.Core.Symbols.Synthesized;
using ZLinq;

namespace Prism.Core.Compiling;

internal sealed class FunctionCompiler
{
    private readonly Compilation _compilation;
    private readonly ICodeEmitter? _emitter;
    private readonly BindingContext _context;
    private readonly CancellationToken _cancellationToken;

    private FunctionCompiler(
        Compilation compilation,
        ICodeEmitter? emitter,
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
        ICodeEmitter? emitter,
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

        Debug.Assert(variableSymbol.IsGlobal);
        var initializer = _compilation.GetBoundInitializer(variableSymbol);
        _context.ReportDiagnostics(initializer.Diagnostics);
        if (initializer.HasErrors)
            return;

        _emitter?.AddGlobalVariable(initializer, _context);
    }

    private void CompileFunction(FunctionSymbol functionSymbol)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var body = _compilation.GetBoundBody(functionSymbol);
        _context.ReportDiagnostics(body.Diagnostics);
        if (body.HasErrors)
            return;

        if (functionSymbol is SynthesizedGlobalConstructor globalCtor)
        {
            using var globals = _compilation
                .GetGlobalVariables()
                .AsValueEnumerable()
                .Select(_compilation.GetBoundInitializer)
                .ToArrayPool();
            if (_emitter is null || !globalCtor.ShouldEmit(globals.Span))
                return;

            _emitter.AddFunction(body, _context);
            _emitter.RegisterGlobalConstructor(functionSymbol);
        }
        else
        {
            _emitter?.AddFunction(body, _context);
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
