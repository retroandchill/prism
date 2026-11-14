using Prism.Configuration;

namespace Prism.Build.Manifest;

public sealed record OutputInfo
{
    public required OutputKind Kind { get; init; }
}
