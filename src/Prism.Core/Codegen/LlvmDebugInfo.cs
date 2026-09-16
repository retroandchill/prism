// @file LlvmDebugInfo.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LLVMSharp.Interop;
using Prism.Core.Compiling;
using Prism.Core.Syntax;
using Prism.Core.Utils;

namespace Prism.Core.Codegen;

internal sealed class LlvmDebugInfo(Compilation compilation, LLVMModuleRef module) : IDisposable
{
    private const LLVMDWARFSourceLanguage SourceLanguage =
        LLVMDWARFSourceLanguage.LLVMDWARFSourceLanguageC;

    private LLVMDIBuilderRef _builder = module.CreateDIBuilder();
    private readonly Dictionary<SyntaxTree, LLVMMetadataRef> _files = new(
        ReferenceEqualityComparer.Instance
    );
    private LLVMMetadataRef _compileUnit;

    private LLVMMetadataRef GetFile(SyntaxTree tree)
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
            compilation.Settings.IsOptimized ? 1 : 0,
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
