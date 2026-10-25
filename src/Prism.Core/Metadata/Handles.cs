// @file Handles.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;

namespace Prism.Core.Metadata;

public interface ITableRowHandle<out THandle>
    where THandle : struct, ITableRowHandle<THandle>
{
    internal static abstract THandle Create(int id);
}

[Archivable]
[Handle]
public readonly partial record struct StringHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial StringHandle(int id);
}

[Archivable]
[Handle]
public readonly partial record struct BlobHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial BlobHandle(int id);
}

[Archivable]
[CompositeHandle(typeof(StringHandle), typeof(BlobHandle))]
public readonly partial record struct DataHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial DataHandle(MetadataToken token);
}

[Archivable]
[Handle]
public readonly partial record struct AssemblyDefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial AssemblyDefHandle(int id);
}

[Archivable]
[Handle]
public readonly partial record struct AssemblyRefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial AssemblyRefHandle(int id);
}

[Archivable]
[CompositeHandle(typeof(AssemblyDefHandle), typeof(AssemblyRefHandle))]
public readonly partial record struct AssemblyHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial AssemblyHandle(MetadataToken token);
}

[Archivable]
[Handle]
public readonly partial record struct NamespaceDefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial NamespaceDefHandle(int id);
}

[Archivable]
[Handle]
public readonly partial record struct NamespaceRefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial NamespaceRefHandle(int id);
}

[Archivable]
[Handle]
public readonly partial record struct TypeDefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial TypeDefHandle(int id);
}

[Archivable]
[Handle]
public readonly partial record struct TypeRefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial TypeRefHandle(int id);
}

[Archivable]
[Handle]
public readonly partial record struct TypeSpecHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial TypeSpecHandle(int id);
}

[Archivable]
[CompositeHandle(typeof(TypeDefHandle), typeof(TypeRefHandle), typeof(TypeSpecHandle))]
public readonly partial record struct TypeHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial TypeHandle(MetadataToken token);
}

[Archivable]
[CompositeHandle(typeof(TypeDefHandle), typeof(TypeRefHandle))]
public readonly partial record struct NamedTypeHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial NamedTypeHandle(MetadataToken token);
}

[Archivable]
[CompositeHandle(typeof(TypeDefHandle), typeof(NamespaceDefHandle))]
public readonly partial record struct MemberContainerDefHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial MemberContainerDefHandle(MetadataToken token);
}

[Archivable]
[CompositeHandle(typeof(TypeRefHandle), typeof(NamespaceRefHandle), typeof(AssemblyRefHandle))]
public readonly partial record struct MemberContainerRefHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial MemberContainerRefHandle(MetadataToken token);
}

[Archivable]
[Handle]
public readonly partial record struct VariableHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial VariableHandle(int id);
}

[Archivable]
[Handle]
public readonly partial record struct FunctionHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial FunctionHandle(int id);
}

[Archivable]
[Handle]
public readonly partial record struct ParameterHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial ParameterHandle(int id);
}

[Archivable]
[CompositeHandle(typeof(FunctionHandle), typeof(TypeDefHandle))]
public readonly partial record struct FunctionOrAttributeHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial FunctionOrAttributeHandle(MetadataToken token);
}

[Archivable]
[Handle]
public readonly partial record struct ConstantHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial ConstantHandle(int id);
}
