using Riok.Mapperly.Abstractions;

namespace Prism.Mir.Analysis;

internal sealed partial class LocalUsageInfoBuilder
{
    public int ReadCount { get; set; }

    public int WriteCount { get; set; }

    public bool IsAddressed { get; set; }

    public ProjectionUsage Projection { get; set; }

    public LocalUsageInfo Build() => Mapper.Map(this);

    [Mapper]
    private static partial class Mapper
    {
        public static partial LocalUsageInfo Map(LocalUsageInfoBuilder builder);
    }
}
