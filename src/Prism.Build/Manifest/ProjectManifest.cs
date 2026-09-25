using Tomlyn;

namespace Prism.Build.Manifest;

public sealed record ProjectManifest
{
    public required PackageInfo Package { get; init; }

    public required OutputInfo Output { get; init; }

    public static ProjectManifest Read(Stream stream)
    {
        return TomlSerializer.Deserialize<ProjectManifest>(stream, PrismTomlContext.Default)
            ?? throw new InvalidOperationException("Unable to deserialize project manifest");
    }
}
