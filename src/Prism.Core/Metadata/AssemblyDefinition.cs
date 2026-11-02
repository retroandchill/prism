using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MagicArchive;

namespace Prism.Core.Metadata;

internal sealed class AssemblyDefinition
{
    public AssemblyIdentity Identity { get; }

    public TargetInfo TargetInfo { get; }

    private readonly MetadataReader _metadataReader;

    private readonly ConcurrentDictionary<AssemblyRefHandle, AssemblyIdentity> _identityCache =
        new();

    private AssemblyDefinition(
        AssemblyIdentity identity,
        TargetInfo targetInfo,
        MetadataReader reader
    )
    {
        Identity = identity;
        TargetInfo = targetInfo;
        _metadataReader = reader;
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

        var (identityData, targetInfo) = reader.ReadArchivable<MetadataHeader>();
        var remaining = buffer.AsMemory(reader.ConsumedBytes);
        var metadataReader = new MetadataReader(remaining);

        var identity = MapAssemblyIdentity(identityData, metadataReader);
        return new AssemblyDefinition(identity, targetInfo, metadataReader);
    }

    private static AssemblyIdentity MapAssemblyIdentity(
        AssemblyIdentityData data,
        MetadataReader metadataReader
    )
    {
        var assemblyName = metadataReader.GetString(data.Name);
        var version = data.Version.ToVersion();
        return new AssemblyIdentity(assemblyName, version);
    }

    private AssemblyIdentity GetAssemblyRef(AssemblyRefHandle handle)
    {
        return _identityCache.GetOrAdd(
            handle,
            static (handle, metadataReader) =>
            {
                ref readonly var data = ref metadataReader.GetAssemblyRef(handle);
                return MapAssemblyIdentity(data, metadataReader);
            },
            _metadataReader
        );
    }

    public ImmutableArray<AssemblyIdentity> ReferencedAssemblies
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var referenced = new AssemblyIdentity[_metadataReader.ReferencedAssembliesCount];
            for (var i = 0; i < referenced.Length; i++)
            {
                referenced[i] = GetAssemblyRef(AssemblyRefHandle.FromRowId(i + 1));
            }

            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                ImmutableCollectionsMarshal.AsImmutableArray(referenced)
            );

            return field;
        }
    }
}
