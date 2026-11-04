using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MagicArchive;
using Prism.Core.Utils;

namespace Prism.Core.Metadata;

internal sealed class AssemblyDefinition
{
    public AssemblyIdentity Identity { get; }
    public TargetInfo TargetInfo { get; }

    private readonly MetadataReader _metadataReader;

    private AssemblyDefinition(MetadataHeader header, ReadOnlyMemory<byte> body)
    {
        _metadataReader = new MetadataReader(body);
        Identity = _metadataReader.MapAssemblyIdentity(header.AssemblyIdentity);
        TargetInfo = header.TargetInfo;
    }

    public static AssemblyDefinition Load(string filepath)
    {
        byte[] buffer;
        using (var stream = File.OpenRead(filepath))
        {
            Debug.Assert(stream.CanSeek);
            buffer = new byte[stream.Length];
            stream.ReadExactly(buffer);
        }

        using var readerState = ArchiveReaderStatePool.Rent(ArchiveSerializerOptions.LittleEndian);
        var reader = new ArchiveReader(buffer, readerState);

        var header = reader.ReadArchivable<MetadataHeader>();
        var remaining = buffer.AsMemory(reader.ConsumedBytes);
        return new AssemblyDefinition(header, remaining);
    }

    public ImmutableArray<AssemblyReference> ReferencedAssemblies
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var referenced = new AssemblyReference[_metadataReader.ReferencedAssembliesCount];
            for (var i = 0; i < referenced.Length; i++)
            {
                referenced[i] = _metadataReader.GetAssemblyRef(AssemblyRefHandle.FromRowId(i + 1));
            }

            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                ImmutableCollectionsMarshal.AsImmutableArray(referenced)
            );

            return field;
        }
    }

    public NamespaceDefinition GlobalNamespace
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                new NamespaceDefinition(
                    NamespaceDefHandle.None,
                    CommonNames.Global,
                    _metadataReader
                ),
                null
            );
            return field;
        }
    }
}
