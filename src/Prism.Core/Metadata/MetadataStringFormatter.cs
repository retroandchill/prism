using MagicArchive;
using MagicArchive.Formatters;

namespace Prism.Core.Metadata;

internal sealed class MetadataStringFormatter : ArchiveFormatter<string>
{
    public static readonly MetadataStringFormatter Default = new();

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in string? value
    )
    {
        var metadataContext = writer.MetadataState;
        if (metadataContext is null)
        {
            Utf8StringFormatter.Default.Serialize(ref writer, value);
            return;
        }

        if (value is null)
        {
            writer.WriteBlittable(-1);
        }
        else
        {
            writer.WriteBlittable(metadataContext.GetOrAddString(value));
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref string? value)
    {
        var metadataContext = reader.MetadataState;
        if (metadataContext is null)
        {
            Utf8StringFormatter.Default.Deserialize(ref reader, ref value);
            return;
        }

        value = metadataContext.GetString(reader.ReadBlittable<int>());
    }
}

internal sealed class MetadataStringFormatterAttribute
    : ArchiveCustomFormatterAttribute<MetadataStringFormatter, string>
{
    public override MetadataStringFormatter GetFormatter()
    {
        return MetadataStringFormatter.Default;
    }
}
