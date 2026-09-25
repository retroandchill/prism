using System.Text.Json.Serialization;
using Tomlyn.Serialization;

namespace Prism.Build.Manifest;

[TomlSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    PreferredObjectCreationHandling = JsonObjectCreationHandling.Replace,
    Converters = [typeof(SemVersionTomlConverter), typeof(OutputKindTomlConverter)]
)]
[TomlSerializable(typeof(ProjectManifest))]
internal partial class PrismTomlContext : TomlSerializerContext;
