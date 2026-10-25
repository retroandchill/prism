// @file Handles.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
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

[Archivable]
[Handle]
public readonly partial record struct TypeRefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial TypeRefHandle(int id);
}

[Archivable]
[Union]
public readonly partial record struct TypeHandle : IUnion
{
    [ArchiveInclude]
    public MetadataToken Token { get; private init; }

    private TypeHandle(MetadataToken token)
    {
        Debug.Assert(token.Type is TokenType.TypeDef or TokenType.TypeRef);
        Token = token;
    }

    public TypeHandle(TypeRefHandle handle)
    {
        Token = handle;
    }

    public TypeHandle(TypeDefHandle handle)
    {
        Token = handle;
    }

    [ArchiveIgnore]
    public object Value
    {
        get
        {
            return Token.Type switch
            {
                TokenType.TypeRef => TypeRefHandle.FromRowId((int)Token.Id),
                TokenType.TypeDef => TypeDefHandle.FromRowId((int)Token.Id),
                _ => throw new InvalidOperationException($"Unexpected token type {Token.Type}"),
            };
        }
    }

    public bool TryGetValue(out TypeRefHandle handle)
    {
        if (Token.Type == TokenType.TypeRef)
        {
            handle = TypeRefHandle.FromRowId((int)Token.Id);
            return true;
        }

        handle = default;
        return false;
    }

    public bool TryGetValue(out TypeDefHandle handle)
    {
        if (Token.Type == TokenType.TypeDef)
        {
            handle = TypeDefHandle.FromRowId((int)Token.Id);
            return true;
        }

        handle = default;
        return false;
    }
}
