// @file Handles.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;

namespace Prism.Core.Metadata;

[AttributeUsage(AttributeTargets.Struct)]
internal sealed class HandleAttribute : Attribute;

[Archivable]
public readonly partial record struct StringHandle(int Offset);

[Archivable]
public readonly partial record struct BlobHandle(int Offset);

[Archivable]
[Handle]
public readonly partial record struct AssemblyRefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial AssemblyRefHandle(int id);
}

[Archivable]
[Handle]
public readonly partial record struct NamespaceHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial NamespaceHandle(int id);
}

[Archivable]
[Handle]
public readonly partial record struct TypeDefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial TypeDefHandle(int id);
}
