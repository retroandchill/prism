using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.CSharp;

public readonly record struct CSharpProductionArgument(CSharpProperty Property, bool IsOptional);

public class CSharpProduction
{
    public CSharpNode Node { get; }

    public ImmutableArray<CSharpProductionArgument> Arguments { get; }

    internal CSharpProduction(CSharpNode node, ImmutableArray<CSharpProductionArgument> arguments)
    {
        Node = node;
        Arguments = arguments;
    }
}
