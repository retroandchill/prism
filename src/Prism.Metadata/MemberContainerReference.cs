// @file MemberContainerReference.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Prism.Utils;

namespace Prism.Metadata;

public closed class MemberContainerReferenceBase
{
    private protected MemberContainerReferenceBase(string name, MetadataReader? reader = null)
    {
        Reader = reader;
        Name = name;
    }

    private protected MetadataReader? Reader { get; }

    public string Name { get; }

    public MemberContainerReference Parent => GetParent();

    protected abstract MemberContainerReference GetParent();
}

public readonly union NamespaceOrAssemblyReference(AssemblyReference, NamespaceReference)
{
    public NamespaceOrAssemblyRefHandle Handle
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

public readonly union MemberContainerReference(AssemblyReference, NamespaceReference, TypeReference)
{
    public MemberContainerRefHandle Handle
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

public sealed class NamespaceReference : MemberContainerReferenceBase
{
    internal NamespaceReference(
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

    internal NamespaceReference(
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
        get { return LazyComputation.LazyComputeUnion(ref field, this, static @this =>
            {
                Debug.Assert(@this.Reader is not null);
                return @this.Reader.GetNamespaceOrAssemblyReference(@this.ParentHandle);
            }); }
        private init
        {
            field = value;
            ParentHandle = value.Handle;
        }
    }

    protected override MemberContainerReference GetParent() => Parent;
}

public sealed class TypeReference : MemberContainerReferenceBase
{
    internal TypeReference(
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

    internal TypeReference(TypeRefHandle handle, string name, MemberContainerReference parent)
        : base(name)
    {
        Handle = handle;
        Parent = parent;
    }

    public TypeRefHandle Handle { get; }

    public MemberContainerRefHandle ParentHandle { get; private init; }

    public new MemberContainerReference Parent
    {
        get { return LazyComputation.LazyComputeUnion(ref field, this, static @this =>
            {
                Debug.Assert(@this.Reader is not null);
                return @this.Reader.GetMemberContainerRef(@this.ParentHandle);
            }); }
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
