using Semver;

namespace Prism.Build.Manifest;

public sealed record PackageInfo
{
    private static readonly SemVersion DefaultVersion = SemVersion.Parse("0.0.0");

    public required string Name { get; init; }

    public SemVersion Version { get; init; } = DefaultVersion;
}
