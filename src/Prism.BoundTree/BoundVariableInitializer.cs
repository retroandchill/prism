using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Prism.Constants;
using Prism.Diagnostics;
using Prism.Semantic.Symbols;
using Prism.Utils;

namespace Prism.BoundTree;

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

    private ThreeState _hasErrors;
    public bool HasErrors
    {
        get
        {
            if (_hasErrors.HasValue)
                return _hasErrors.Value;

            var hasErrors =
                (Initializer is not null && Initializer.HasErrors)
                || Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
            _hasErrors = hasErrors.ToThreeState();
            return hasErrors;
        }
    }

    public ImmutableArray<Diagnostic> Diagnostics { get; } = diagnostics;
}
