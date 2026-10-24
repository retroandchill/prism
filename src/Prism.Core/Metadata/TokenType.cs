// @file TokenType.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using NetEscapades.EnumGenerators;

namespace Prism.Core.Metadata;

[EnumExtensions(ExtensionClassName = nameof(Sections))]
internal enum Section : byte
{
    StringTable,
    AssemblyReferences,
    Namespaces,
    SymbolTable,
}

internal static partial class Sections
{
    public const uint Assembly = 0;
    public const uint AssemblyRef = 1;
    public const uint TypeRef = 2;
    public const uint TypeDef = 3;
    public const uint Variable = 4;
    public const uint Function = 5;
    public const uint Parameter = 6;

    public const int RowIdBitCount = 24;
}

public enum TokenType : uint
{
    // ReSharper disable once ShiftExpressionZeroLeftOperand
    Assembly = Sections.Assembly << Sections.RowIdBitCount,
    AssemblyRef = Sections.AssemblyRef << Sections.RowIdBitCount,
    TypeRef = Sections.TypeRef << Sections.RowIdBitCount,
    TypeDef = Sections.TypeDef << Sections.RowIdBitCount,
    Variable = Sections.Variable << Sections.RowIdBitCount,
    Function = Sections.Function << Sections.RowIdBitCount,
    Parameter = Sections.Parameter << Sections.RowIdBitCount,
}
