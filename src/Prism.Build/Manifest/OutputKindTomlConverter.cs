using Prism.Core.Configuration;
using Tomlyn;
using Tomlyn.Serialization;

namespace Prism.Build.Manifest;

internal sealed class OutputKindTomlConverter : TomlConverter<OutputKind>
{
    public override OutputKind Read(TomlReader reader)
    {
        var str = reader.GetString();
        return str switch
        {
            "executable" => OutputKind.Executable,
            "static-library" => OutputKind.StaticLibrary,
            "shared-library" => OutputKind.SharedLibrary,
            _ => throw new TomlException($"Failed to parse OutputKind from string: {str}"),
        };
    }

    public override void Write(TomlWriter writer, OutputKind value)
    {
        switch (value)
        {
            case OutputKind.Executable:
                writer.WriteStringValue("executable");
                break;
            case OutputKind.StaticLibrary:
                writer.WriteStringValue("static-library");
                break;
            case OutputKind.SharedLibrary:
                writer.WriteStringValue("shared-library");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }
}
