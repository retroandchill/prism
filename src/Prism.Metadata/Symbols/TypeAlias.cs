using System.Diagnostics;
using Prism.Metadata.Builders;
using Prism.Metadata.Serialization;
using Prism.Utils;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Metadata.Symbols;

public sealed partial class TypeAlias
{
    private readonly MetadataReader? _reader;
    private readonly TypeHandle _aliasedType;

    internal TypeAlias(
        TypeAliasHandle handle,
        TypeAliasAttributes attributes,
        string name,
        TypeHandle aliasedType,
        MemberContainerDefHandle parent,
        MetadataReader reader
    )
    {
        Handle = handle;
        Attributes = attributes;
        Name = name;
        _aliasedType = aliasedType;
        ParentHandle = parent;
        _reader = reader;
    }

    [MapperConstructor]
    private TypeAlias(TypeAliasHandle handle, TypeAliasAttributes attributes, string name)
    {
        Handle = handle;
        Attributes = attributes;
        Name = name;
    }

    public TypeAliasHandle Handle { get; }

    internal TypeAliasAttributes Attributes { get; }

    public string Name { get; }

    public MemberContainerDefHandle ParentHandle { get; private set; }

    public Symbols.MemberContainerDefinition Parent
    {
        get
        {
            if (field is not null)
                return field;

            Debug.Assert(_reader is not null);
            Interlocked.CompareExchange(
                ref field,
                _reader.GetMemberContainerDef(ParentHandle),
                null
            );
            return field;
        }
        private set
        {
            field = value;
            ParentHandle = value.Handle;
        }
    }

    public TypeDescriptor AliasedType
    {
        get { return LazyComputation.LazyComputeUnion(ref field, this, static @this =>
            {
                Debug.Assert(@this._reader is not null);
                return @this._reader.GetTypeDescriptor(@this._aliasedType);
            }); }
        private set;
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, UseReferenceHandling = true)]
    [UseStaticMapper(typeof(Symbols.MemberContainerDefinition.Mapper))]
    [UseStaticMapper(typeof(Builders.DescriptorMapper))]
    internal static partial class Mapper
    {
        [MapperIgnoreTarget(nameof(ParentHandle))]
        [MapProperty(nameof(TypeAliasBuilder.Container), nameof(Parent))]
        public static partial TypeAlias ToDefinition(
            TypeAliasBuilder builder,
            [ReferenceHandler] IReferenceHandler handler
        );
    }
}
