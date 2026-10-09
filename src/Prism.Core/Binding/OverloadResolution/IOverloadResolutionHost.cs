using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Semantic;
using Prism.Core.Symbols;

namespace Prism.Core.Binding.OverloadResolution;

internal interface IOverloadResolutionHost
{
    Compilation Compilation { get; }

    BoundExpression? TryGetImplicitReceiver();

    Conversion ClassifyConversion(TypeSymbol source, TypeSymbol target);

    BoundExpression AddConversion(
        BoundExpression expression,
        TypeSymbol targetType,
        Conversion conversion,
        BindingContext context
    );

    BoundExpression ApplySpeculativeBinding(
        BoundExpression expression,
        TypeSymbol? targetType,
        BindingContext context
    );
}
