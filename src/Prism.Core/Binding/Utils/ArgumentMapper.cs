using System.Collections.Immutable;
using Prism.Core.Binding.OverloadResolution;
using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Symbols;
using Prism.Core.Syntax;
using ZLinq;

namespace Prism.Core.Binding.Utils;

internal static class ArgumentMapper
{
    public static BoundExpression[]? TryMapArgumentsToParameters(
        Compilation compilation,
        ReadOnlySpan<ParameterSymbol> parameters,
        ImmutableArray<CallArgument> callArguments,
        SyntaxNode fallbackSyntax
    )
    {
        var parameterCount = parameters.Length;
        var mapped = new BoundExpression?[parameterCount];

        foreach (var (i, arg) in callArguments.AsValueEnumerable().Index())
        {
            if (arg.Name is null)
            {
                mapped[i] = arg.Expression;
            }
            else
            {
                var index = FindParameterIndexByName(parameters, arg.Name);
                if (index < 0 || mapped[index] is not null)
                    return null;

                mapped[index] = arg.Expression;
            }
        }

        for (var i = 0; i < parameterCount; i++)
        {
            if (mapped[i] is not null)
                continue;

            var defaultValue = GetDefaultValue(compilation, parameters[i], fallbackSyntax);
            if (defaultValue is null)
                return null;

            mapped[i] = defaultValue;
        }

        return mapped!;
    }

    private static int FindParameterIndexByName(
        ReadOnlySpan<ParameterSymbol> parameters,
        string name
    )
    {
        foreach (var (i, parameter) in parameters.AsValueEnumerable().Index())
        {
            if (parameter.Name == name)
                return i;
        }

        return -1;
    }

    private static BoundExpression? GetDefaultValue(
        Compilation compilation,
        ParameterSymbol parameter,
        SyntaxNode fallbackSyntax
    )
    {
        return parameter.DefaultValue switch
        {
            ConstantParameterDefault(var constant, var syntax) => new BoundLiteral(
                compilation,
                syntax ?? fallbackSyntax,
                parameter.Type,
                constant
            ),
            null => null,
        };
    }
}
