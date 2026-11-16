namespace Prism.Mir.Analysis;

internal sealed class LocalUsageInfoBuilder
{
    public int ReadCount { get; set; }

    public int WriteCount { get; set; }

    public bool IsAddressed { get; set; }

    public bool IsProjected { get; set; }
}
