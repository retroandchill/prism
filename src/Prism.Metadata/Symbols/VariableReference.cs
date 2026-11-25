// @file VariableReference.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Prism.Metadata.Serialization;
using Prism.Utils;

namespace Prism.Metadata.Symbols;

public sealed class VariableReference
{
    private readonly MetadataReader? _reader;

    internal VariableReference(
        VariableRefHandle handle,
        string name,
        MemberContainerRefHandle parent,
        MetadataReader reader
    )
    {
        Handle = handle;
        Name = name;
        ParentHandle = parent;
        _reader = reader;
    }

    internal VariableReference(
        VariableRefHandle handle,
        string name,
        MemberContainerReference parent
    )
    {
        Handle = handle;
        Name = name;
        Parent = parent;
    }

    public VariableRefHandle Handle { get; }

    public string Name { get; }

    public MemberContainerRefHandle ParentHandle { get; private init; }

    public MemberContainerReference Parent
    {
        get { return LazyComputation.LazyComputeUnion(ref field, this, static @this =>
            {
                Debug.Assert(@this._reader is not null);
                return @this._reader.GetMemberContainerRef(@this.ParentHandle);
            }); }
        private init
        {
            field = value;
            ParentHandle = value.Handle;
        }
    }
}
