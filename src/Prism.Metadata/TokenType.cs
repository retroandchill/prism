// @file TokenType.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;
using NetEscapades.EnumGenerators;

namespace Prism.Metadata;

[EnumExtensions(ExtensionClassName = nameof(Sections))]
internal enum Section
{
    StringTable = 0,
    Blobs = 1,
    AssemblyReferences = 10,
    NamespaceDefs = 11,
    NamespaceRefs = 12,
    TypeDefs = 20,
    TypeRefs = 21,
    TypeSpecs = 22,
    TypeAliases = 23,
    Variables = 30,
    Functions = 31,
    Parameters = 32,
    EnumOrdinals = 33,
    CustomAttributes = 50,
    VariableRefs = 80,
    FunctionRefs = 81,

    VariableInitializerContents = 120,

    VariableInitializers = 150,
}

internal static partial class Sections
{
    public const uint AssemblyDef = 0;
    public const uint AssemblyRef = 1;
    public const uint NamespaceDef = 2;
    public const uint NamespaceRef = 3;

    public const uint TypeRef = 12;
    public const uint TypeDef = 13;
    public const uint TypeSpec = 14;
    public const uint TypeAlias = 15;

    public const uint Variable = 24;
    public const uint Function = 25;
    public const uint Parameter = 26;
    public const uint EnumOrdinal = 27;

    public const uint VariableRef = 40;
    public const uint FunctionRef = 41;

    public const uint CustomAttribute = 80;

    public const uint String = 120;
    public const uint Blob = 121;
    public const uint InlineConst = 122;

    public const uint VariableInitializers = 150;

    public const int RowIdBitCount = 24;
}

[Archivable]
internal readonly partial record struct SectionHeader(
    Section Kind,
    int Offset,
    int Size,
    int Count
);

public enum TokenType : uint
{
    // ReSharper disable once ShiftExpressionZeroLeftOperand
    AssemblyDef = Sections.AssemblyDef << Sections.RowIdBitCount,
    AssemblyRef = Sections.AssemblyRef << Sections.RowIdBitCount,
    NamespaceDef = Sections.NamespaceDef << Sections.RowIdBitCount,
    NamespaceRef = Sections.NamespaceRef << Sections.RowIdBitCount,

    TypeRef = Sections.TypeRef << Sections.RowIdBitCount,
    TypeDef = Sections.TypeDef << Sections.RowIdBitCount,
    TypeSpec = Sections.TypeSpec << Sections.RowIdBitCount,
    TypeAlias = Sections.TypeAlias << Sections.RowIdBitCount,

    Variable = Sections.Variable << Sections.RowIdBitCount,
    Function = Sections.Function << Sections.RowIdBitCount,
    Parameter = Sections.Parameter << Sections.RowIdBitCount,
    EnumOrdinal = Sections.EnumOrdinal << Sections.RowIdBitCount,

    VariableRef = Sections.VariableRef << Sections.RowIdBitCount,
    FunctionRef = Sections.FunctionRef << Sections.RowIdBitCount,

    CustomAttribute = Sections.CustomAttribute << Sections.RowIdBitCount,

    String = Sections.String << Sections.RowIdBitCount,
    Blob = Sections.Blob << Sections.RowIdBitCount,
    InlineConst = Sections.InlineConst << Sections.RowIdBitCount,

    VariableInitializer = Sections.VariableInitializers << Sections.RowIdBitCount,
}
