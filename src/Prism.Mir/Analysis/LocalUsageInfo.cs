namespace Prism.Mir.Analysis;

public sealed class LocalUsageInfo
{
    public int ReadCount { get; }

    public int WriteCount { get; }

    public bool IsAddressed { get; }

    public bool IsProjected { get; }

    internal LocalUsageInfo(int readCount, int writeCount, bool isAddressed, bool isProjected)
    {
        ReadCount = readCount;
        WriteCount = writeCount;
        IsAddressed = isAddressed;
        IsProjected = isProjected;
    }
}
