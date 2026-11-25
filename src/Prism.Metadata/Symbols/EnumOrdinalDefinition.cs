// @file EnumOrdinalDefinition.cs
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

public partial class EnumOrdinalDefinition : IAttributeHolderDefinition
{
    public EnumOrdinalHandle Handle { get; }
    public string Name { get; }
    private readonly MetadataReader? _reader;

    internal EnumOrdinalDefinition(
        EnumOrdinalHandle handle,
        string name,
        TypeDefHandle parent,
        ulong bits,
        MetadataReader reader
    )
    {
        Handle = handle;
        ParentHandle = parent;
        _reader = reader;
        Name = name;
        Bits = bits;
    }

    [MapperConstructor]
    internal EnumOrdinalDefinition(EnumOrdinalHandle handle, string name, ulong bits)
    {
        Handle = handle;
        Name = name;
        Bits = bits;
    }

    public ulong Bits { get; }

    public TypeDefHandle ParentHandle { get; private set; }

    public TypeDefinition Parent
    {
        get
        {
            if (field is not null)
                return field;

            Debug.Assert(_reader is not null);
            Interlocked.CompareExchange(ref field, _reader.GetTypeDef(ParentHandle), null);
            return field;
        }
        private set
        {
            field = value;
            ParentHandle = value.Handle;
        }
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, UseReferenceHandling = true)]
    [UseStaticMapper(typeof(TypeDefinition.Mapper))]
    [UseStaticMapper(typeof(Symbols.CustomAttributeDefinition.Mapper))]
    internal static partial class Mapper
    {
        [MapperIgnoreTarget(nameof(ParentHandle))]
        public static partial EnumOrdinalDefinition ToDefinition(
            EnumOrdinalDefinitionBuilder builder,
            [ReferenceHandler] IReferenceHandler handler
        );
    }

    public ImmutableArray<Symbols.CustomAttributeDefinition> CustomAttributes
    {
        get { return LazyComputation.LazyCompute(ref field, this, static @this =>
            {
                Debug.Assert(@this._reader is not null);
                var mappings = @this._reader.CustomAttributesByMember[@this.Handle];
                return [.. mappings.Select(@this._reader.GetCustomAttribute)];
            }); }
        private set;
    }
}
