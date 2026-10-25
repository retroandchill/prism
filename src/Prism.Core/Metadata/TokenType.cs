// @file TokenType.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;
using NetEscapades.EnumGenerators;

namespace Prism.Core.Metadata;

[EnumExtensions(ExtensionClassName = nameof(Sections))]
internal enum Section
{
    StringTable = 0,
    BinaryBlobs = 1,
    AssemblyReferences = 10,
    NamespaceDefs = 11,
    NamespaceRefs = 12,
    TypeDefs = 20,
    TypeRefs = 21,
    TypeSpecs = 22,
    Variables = 30,
    Constants = 50,
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

    public const uint Variable = 24;
    public const uint Function = 25;
    public const uint Parameter = 26;

    public const uint Constant = 80;

    public const uint String = 120;
    public const uint Blob = 121;

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

    Variable = Sections.Variable << Sections.RowIdBitCount,
    Function = Sections.Function << Sections.RowIdBitCount,
    Parameter = Sections.Parameter << Sections.RowIdBitCount,

    Constant = Sections.Constant << Sections.RowIdBitCount,

    String = Sections.String << Sections.RowIdBitCount,
    Blob = Sections.Blob << Sections.RowIdBitCount,
}
