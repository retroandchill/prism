// @file FunctionReference.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Metadata.Builders;
using Prism.Metadata.Serialization;
using Prism.Utils;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Metadata.Symbols;

public sealed partial class FunctionReference
{
    private readonly MetadataReader? _reader;
    private readonly BlobHandle _parametersHandle;

    internal FunctionReference(
        FunctionRefHandle handle,
        string name,
        MemberContainerRefHandle parent,
        BlobHandle parameters,
        MetadataReader? reader
    )
    {
        Handle = handle;
        Name = name;
        ParentHandle = parent;
        _parametersHandle = parameters;
        _reader = reader;
    }

    [MapperConstructor]
    internal FunctionReference(
        FunctionRefHandle handle,
        string name,
        MemberContainerReference parent,
        ImmutableArray<TypeDescriptor> parameters
    )
    {
        Handle = handle;
        Name = name;
        Parent = parent;
        Parameters = parameters;
    }

    public FunctionRefHandle Handle { get; }

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

    public ImmutableArray<TypeDescriptor> Parameters
    {
        get { return LazyComputation.LazyCompute(ref field, this, static @this =>
            {
                Debug.Assert(@this._reader is not null);
                return
                [
                    .. @this
                        ._reader.GetBlob<ImmutableArray<TypeHandle>>(@this._parametersHandle)
                        .Select(@this._reader.GetTypeDescriptor),
                ];
            }); }
        private set;
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, UseReferenceHandling = true)]
    [UseStaticMapper(typeof(DescriptorMapper))]
    internal static partial class Mapper
    {
        [MapperIgnoreTarget(nameof(ParentHandle))]
        public static partial FunctionReference ToReference(
            FunctionReferenceBuilder builder,
            [ReferenceHandler] IReferenceHandler handler
        );
    }
}
