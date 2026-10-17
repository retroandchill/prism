using System.Buffers;
using System.Collections.Immutable;

namespace Prism.Core.Metadata;

internal sealed class MetadataReader : IDisposable
{
    private readonly ReadOnlySequence<byte> _buffer;
    private SequencePosition _position;

    public MetadataReader(ReadOnlySequence<byte> buffer)
    {
        _buffer = buffer;
        _position = buffer.Start;
    }

    public long Consumed => _buffer.Slice(_buffer.Start, _position).Length;
    public bool End => _position.Equals(_buffer.End);

    public void Seek(SequencePosition position)
    {
        _position = position;
    }

    public ImmutableArray<AssemblyReference> ReadReferencedAssemblies()
    {
        throw new NotImplementedException();
    }

    public Scope EnterScope()
    {
        return new Scope(this);
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }

    internal readonly ref struct Scope(MetadataReader reader) : IDisposable
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}
