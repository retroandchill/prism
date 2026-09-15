// @file LlvmDebugInfo.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using LLVMSharp;
using LLVMSharp.Interop;
using Prism.Core.Compiling;
using Prism.Core.Symbols;
using Prism.Core.Syntax;
using Prism.Core.Utils;

namespace Prism.Core.Codegen;

internal sealed class LlvmDebugInfo : IDisposable
{
    private const LLVMDWARFSourceLanguage SourceLanguage =
        LLVMDWARFSourceLanguage.LLVMDWARFSourceLanguageC;

    private readonly Compilation _compilation;
    private readonly LLVMContextRef _context;
    private readonly LLVMModuleRef _module;
    private LLVMDIBuilderRef _builder;
    private readonly Dictionary<SyntaxTree, LLVMMetadataRef> _files = new(
        ReferenceEqualityComparer.Instance
    );
    private LLVMMetadataRef _compileUnit;
    private readonly Dictionary<FunctionSymbol, LLVMMetadataRef> _subprograms = new(
        ReferenceEqualityComparer.Instance
    );

    public LlvmDebugInfo(Compilation compilation, LLVMContextRef context, LLVMModuleRef module)
    {
        _compilation = compilation;
        _context = context;
        _module = module;
        _builder = _module.CreateDIBuilder();
    }

    public LLVMMetadataRef GetFile(SyntaxTree tree)
    {
        return _files.GetOrAdd(tree, static (t, self) => self.CreateFile(t), this);
    }

    private LLVMMetadataRef CreateFile(SyntaxTree tree)
    {
        return _builder.CreateFile(tree.Path, "");
    }

    public LLVMMetadataRef GetCompileUnit(SyntaxTree tree)
    {
        if (_compileUnit != default)
        {
            return _compileUnit;
        }

        _compileUnit = CreateCompileUnit(tree);
        return _compileUnit;
    }

    private LLVMMetadataRef CreateCompileUnit(SyntaxTree tree)
    {
        var file = GetFile(tree);
        return _builder.CreateCompileUnit(
            SourceLanguage,
            file,
            "prism",
            _compilation.Settings.IsOptimized ? 1 : 0,
            "",
            0,
            "",
            LLVMDWARFEmissionKind.LLVMDWARFEmissionFull,
            0,
            0,
            0,
            "",
            ""
        );
    }

    public void Dispose()
    {
        _builder.Dispose();
    }
}
