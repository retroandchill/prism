using Prism.Core.Parse;

namespace Prism.Core;

public abstract record Location;

public sealed record SourceLocation(SourceFile SourceFile, TextSpan Span) : Location;

public sealed record ExternalFileLocation(string FilePath, TextSpan Span) : Location;

public sealed record NoLocation : Location
{
    public static NoLocation Instance { get; } = new();
}
