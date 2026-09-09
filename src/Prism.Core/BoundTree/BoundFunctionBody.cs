using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Prism.Core.Diagnostics;
using Prism.Core.FlowAnalysis;
using Prism.Core.Symbols;
using Prism.Core.Utils;

namespace Prism.Core.BoundTree;

internal sealed class BoundFunctionBody
{
    public BoundFunctionBody(
        FunctionSymbol function,
        BoundStatement body,
        FunctionBodyAnalysis analysis,
        ImmutableArray<Diagnostic> diagnostics
    )
    {
        Function = function;
        Body = body;
        Analysis = analysis;
        HasBody = true;
        Diagnostics = diagnostics;
    }

    public BoundFunctionBody(FunctionSymbol function, ImmutableArray<Diagnostic> diagnostics)
    {
        Function = function;
        Body = null;
        Analysis = null;
        HasBody = false;
        Diagnostics = diagnostics;
    }

    public FunctionSymbol Function { get; }

    [MemberNotNullWhen(true, nameof(Body), nameof(Analysis))]
    public bool HasBody { get; }

    public BoundStatement? Body { get; }

    public FunctionBodyAnalysis? Analysis { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    private ThreeState _hasErrors;
    public bool HasErrors
    {
        get
        {
            if (_hasErrors.HasValue)
                return _hasErrors.Value;

            var hasErrors =
                (Body is not null && Body.HasErrors)
                || Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
            _hasErrors = hasErrors.ToThreeState();
            return hasErrors;
        }
    }
}
