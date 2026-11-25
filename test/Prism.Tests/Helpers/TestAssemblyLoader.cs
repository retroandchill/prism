using Prism.Compiler.Symbols;
using Prism.Configuration;
using Prism.Metadata;
using Prism.Metadata.Builders;

namespace Prism.Tests.Helpers;

public sealed class TestAssemblyLoader : IAssemblyLoader
{
    private static readonly AssemblyDefinition StandardLibraryStub = CreateStandardLibraryStub();

    public AssemblyDefinition? LoadAssembly(
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        return filePath == "prism-std" ? StandardLibraryStub : null;
    }

    private static AssemblyDefinition CreateStandardLibraryStub()
    {
        var builder = new AssemblyDefinitionBuilder(
            new AssemblyIdentity("prism-std"),
            TargetInfo.CurrentPlatform
        );

        var stdNamespace = builder.GlobalNamespace.AddNestedNamespace("std");
        foreach (var type in SpecialTypes.GetValues())
        {
            if (type == SpecialType.None)
                continue;

            AddSpecialType(stdNamespace, type);
        }
        return builder.Build();
    }

    private static void AddSpecialType(
        NamespaceDefinitionBuilder stdNamespace,
        SpecialType specialType
    )
    {
        var type = stdNamespace.AddType(specialType.BuiltInTypeName);
        type.SpecialType = specialType;
        switch (specialType)
        {
            case SpecialType.None:
                break;
            case SpecialType.Void:
                break;
            case SpecialType.Bool:
                break;
            case SpecialType.I8:
                break;
            case SpecialType.I16:
                break;
            case SpecialType.I32:
                break;
            case SpecialType.I64:
                break;
            case SpecialType.I128:
                break;
            case SpecialType.ISize:
                break;
            case SpecialType.U8:
                break;
            case SpecialType.U16:
                break;
            case SpecialType.U32:
                break;
            case SpecialType.U64:
                break;
            case SpecialType.U128:
                break;
            case SpecialType.USize:
                break;
            case SpecialType.F32:
                break;
            case SpecialType.F64:
                break;
            case SpecialType.Char:
                break;
            case SpecialType.Char16:
                break;
            case SpecialType.Rune:
                break;
            case SpecialType.Str:
                break;
            case SpecialType.Ordering:
                type.Kind = NamedTypeKind.Enum;
                type.BaseType = stdNamespace.Types.Single(t => t.SpecialType == SpecialType.I8);
                type.AddEnumOrdinal("Less", GetI8Bits(-1));
                type.AddEnumOrdinal("Equal", GetI8Bits(0));
                type.AddEnumOrdinal("Greater", GetI8Bits(1));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(specialType), specialType, null);
        }

        static ulong GetI8Bits(sbyte value)
        {
            return unchecked((byte)value);
        }
    }
}
