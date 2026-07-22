using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.Cpp;

public readonly record struct CppProductionArgument(CppProperty Property, bool IsOptional);

public class CppProduction
{
    public CppNode Node { get; }

    public ImmutableArray<CppProductionArgument> Arguments { get; }

    internal CppProduction(CppNode node, ImmutableArray<CppProductionArgument> arguments)
    {
        Node = node;
        Arguments = arguments;
    }
}
