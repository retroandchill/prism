using Tomlyn;

namespace Prism.Build.Manifest;

public sealed record ProjectManifest
{
    public required PackageInfo Package { get; init; }

    public required OutputInfo Output { get; init; }
}
