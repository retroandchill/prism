using System.Collections.Immutable;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Syntax;
using Prism.Core.Text;

namespace Prism.Core.Symbols.Source;

internal abstract class SourceVariableSymbol : VariableSymbol
{
    private SymbolCompletionState _completionState;
    protected VariableDeclarationSyntax Syntax { get; }

    protected SourceVariableSymbol(
        string name,
        Symbol? containingSymbol,
        VariableDeclarationSyntax syntax
    )
        : base(name, containingSymbol)
    {
        Syntax = syntax;
    }

    public sealed override ImmutableArray<Location> Locations { get; }

    public sealed override TypeSymbol Type { get; }

    protected abstract TypeSymbol ComputeType(DiagnosticBag diagnostics);

    public sealed override bool IsMutable { get; }

    public sealed override bool HasInitializer { get; }

    public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

    public sealed override ConstantValue? ConstantValue { get; }

    protected abstract ConstantValue? ComputeConstantValue(DiagnosticBag diagnostics);

    public sealed override bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithin)
    {
        throw new NotImplementedException();
    }

    internal sealed override bool NeedsCompletion => false;

    internal sealed override void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    )
    {
        base.ForceComplete(location, filter, cancellationToken);
    }

    internal sealed override bool IsComplete(CompletionPart part)
    {
        return _completionState.IsComplete(part);
    }
}

internal sealed class SourceGlobalVariableSymbol : SourceVariableSymbol
{
    public SourceGlobalVariableSymbol(
        string name,
        Symbol? containingSymbol,
        VariableDeclarationSyntax syntax
    )
        : base(name, containingSymbol, syntax) { }

    public override bool IsGlobal => true;

    protected override TypeSymbol ComputeType(DiagnosticBag diagnostics)
    {
        throw new NotImplementedException();
    }

    protected override ConstantValue? ComputeConstantValue(DiagnosticBag diagnostics)
    {
        throw new NotImplementedException();
    }
}
