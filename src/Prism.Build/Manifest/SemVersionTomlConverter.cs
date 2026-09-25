using Semver;
using Tomlyn;
using Tomlyn.Serialization;

namespace Prism.Build.Manifest;

internal sealed class SemVersionTomlConverter : TomlConverter<SemVersion>
{
    public override SemVersion Read(TomlReader reader)
    {
        var str = reader.GetString();
        try
        {
            return SemVersion.Parse(str);
        }
        catch (Exception ex)
        {
            throw new TomlException($"Failed to parse SemVersion from string: {str}", ex);
        }
    }

    public override void Write(TomlWriter writer, SemVersion value)
    {
        writer.WriteStringValue(value.ToString());
    }
}
