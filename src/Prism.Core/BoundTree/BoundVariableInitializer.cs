using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Prism.Core.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Symbols;

namespace Prism.Core.BoundTree;

internal sealed class BoundVariableInitializer(
    VariableSymbol variable,
    BoundExpression? initializer,
    ImmutableArray<Diagnostic> diagnostics
)
{
    public BoundVariableInitializer(VariableSymbol variable, ImmutableArray<Diagnostic> diagnostics)
        : this(variable, null, diagnostics) { }

    public VariableSymbol Variable { get; } = variable;

    public BoundExpression? Initializer { get; } = initializer;

    [MemberNotNullWhen(true, nameof(Initializer))]
    public bool HasInitializer => Initializer is not null;

    public ConstantValue? ConstantValue => Initializer?.ConstantValue;

    public ImmutableArray<Diagnostic> Diagnostics { get; } = diagnostics;
}
