using Prism.Compiler.BoundTree;
using Prism.Compiler.Semantic;
using Prism.Compiler.Symbols;
using Prism.Semantic;
using Prism.Semantic.Conversions;
using Prism.Semantic.Symbols;

namespace Prism.Compiler.Binding.OverloadResolution;

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
