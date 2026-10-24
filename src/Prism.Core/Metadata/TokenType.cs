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
    StringTable,
    BinaryBlobs,
    AssemblyReferences,
    Namespaces,
}

internal static partial class Sections
{
    public const uint Assembly = 0;
    public const uint AssemblyRef = 1;
    public const uint Namespace = 2;
    public const uint TypeRef = 3;
    public const uint TypeDef = 4;
    public const uint Variable = 5;
    public const uint Function = 6;
    public const uint Parameter = 7;

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
    Assembly = Sections.Assembly << Sections.RowIdBitCount,
    AssemblyRef = Sections.AssemblyRef << Sections.RowIdBitCount,
    Namespace = Sections.Namespace << Sections.RowIdBitCount,
    TypeRef = Sections.TypeRef << Sections.RowIdBitCount,
    TypeDef = Sections.TypeDef << Sections.RowIdBitCount,
    Variable = Sections.Variable << Sections.RowIdBitCount,
    Function = Sections.Function << Sections.RowIdBitCount,
    Parameter = Sections.Parameter << Sections.RowIdBitCount,
}
