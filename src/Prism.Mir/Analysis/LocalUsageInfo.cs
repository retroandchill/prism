using System.Collections.Immutable;

namespace Prism.Mir.Analysis;

[Flags]
public enum ProjectionUsage : byte
{
    Read = 1 << 0,
    Write = 1 << 1,

    ReadWrite = Read | Write,
}

public sealed class LocalUsageInfo
{
    public int ReadCount { get; }

    public int WriteCount { get; }

    public bool IsAddressed { get; }

    public ProjectionUsage Projection { get; }

    public ImmutableArray<AssignmentInstruction> Assignments { get; }

    internal LocalUsageInfo(
        int readCount,
        int writeCount,
        bool isAddressed,
        ProjectionUsage projection,
        ImmutableArray<AssignmentInstruction> assignments
    )
    {
        ReadCount = readCount;
        WriteCount = writeCount;
        IsAddressed = isAddressed;
        Projection = projection;
        Assignments = assignments;
    }
}
