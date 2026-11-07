// @file MemberContainerReference.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Prism.Core.Utils;

namespace Prism.Core.Metadata;

internal closed class MemberContainerReferenceBase(string name, MetadataReader? reader = null)
{
    protected MetadataReader? Reader { get; } = reader;

    public string Name { get; } = name;

    public MemberContainerReference Parent => GetParent();

    protected abstract MemberContainerReference GetParent();
}

internal union NamespaceOrAssemblyReference(AssemblyReference, NamespaceReference)
{
    public readonly NamespaceOrAssemblyRefHandle Handle
    {
        get
        {
            return this switch
            {
                AssemblyReference asm => asm.Handle,
                NamespaceReference type => type.Handle,
            };
        }
    }

    public static implicit operator MemberContainerReference(NamespaceOrAssemblyReference reference)
    {
        return reference switch
        {
            AssemblyReference asm => asm,
            NamespaceReference ns => ns,
        };
    }
}

internal union MemberContainerReference(AssemblyReference, NamespaceReference, TypeReference)
{
    public readonly MemberContainerRefHandle Handle
    {
        get
        {
            return this switch
            {
                AssemblyReference asm => asm.Handle,
                NamespaceReference type => type.Handle,
                TypeReference ns => ns.Handle,
            };
        }
    }
}

internal sealed class NamespaceReference : MemberContainerReferenceBase
{
    public NamespaceReference(
        NamespaceRefHandle handle,
        string name,
        NamespaceOrAssemblyRefHandle parent,
        MetadataReader reader
    )
        : base(name, reader)
    {
        ParentHandle = parent;
        Handle = handle;
    }

    public NamespaceReference(
        NamespaceRefHandle handle,
        string name,
        NamespaceOrAssemblyReference parent
    )
        : base(name)
    {
        Handle = handle;
    }

    public NamespaceRefHandle Handle { get; }

    public NamespaceOrAssemblyRefHandle ParentHandle { get; private init; }

    public new NamespaceOrAssemblyReference Parent
    {
        get
        {
            if (field.Value is not null)
                return field;

            Debug.Assert(Reader is not null);
            var reference = Reader.GetNamespaceOrAssemblyReference(ParentHandle);
            UnionInterlocked.Initialize(ref field, reference);
            return field;
        }
        private init
        {
            field = value;
            ParentHandle = value.Handle;
        }
    }

    protected override MemberContainerReference GetParent() => Parent;
}

internal sealed class TypeReference : MemberContainerReferenceBase
{
    public TypeReference(
        TypeRefHandle handle,
        string name,
        MemberContainerRefHandle parentHandle,
        MetadataReader reader
    )
        : base(name, reader)
    {
        Handle = handle;
        ParentHandle = parentHandle;
    }

    public TypeReference(TypeRefHandle handle, string name, MemberContainerReference parent)
        : base(name)
    {
        Handle = handle;
        Parent = parent;
    }

    public TypeRefHandle Handle { get; }

    public MemberContainerRefHandle ParentHandle { get; private init; }

    public new MemberContainerReference Parent
    {
        get
        {
            if (field.Value is not null)
                return field;

            Debug.Assert(Reader is not null);
            var reference = Reader.GetMemberContainerRef(ParentHandle);
            UnionInterlocked.Initialize(ref field, reference);
            return field;
        }
        private init
        {
            field = value;
            ParentHandle = value.Handle;
        }
    }

    protected override MemberContainerReference GetParent()
    {
        return Parent;
    }
}
