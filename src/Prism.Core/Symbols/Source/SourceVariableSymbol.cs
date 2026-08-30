using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Syntax;
using Prism.Core.Text;
using ZLinq;

namespace Prism.Core.Symbols.Source;

internal abstract class SourceVariableSymbol : VariableSymbol
{
    private sealed record ConstantValueWrapper(ConstantValue? Value);

    private SymbolCompletionState _completionState;
    protected VariableDeclarationSyntax Syntax { get; }

    private ConstantValueWrapper? _constantValue;

    protected SourceVariableSymbol(
        string name,
        Symbol? containingSymbol,
        VariableDeclarationSyntax syntax
    )
        : base(name, containingSymbol)
    {
        Syntax = syntax;
        IsMutable = syntax
            .Modifiers.AsValueEnumerable()
            .Any(t => t.Kind == SyntaxKind.MutableKeyword);
    }

    public sealed override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Interlocked.CompareExchange(ref field, [Syntax.Identifier.Location], default);
            return field;
        }
    }

    public sealed override TypeSymbol Type
    {
        get
        {
            if (field is not null)
                return field;

            var diagnostics = DiagnosticBag.Create();
            if (Interlocked.CompareExchange(ref field, ComputeType(diagnostics), null) is not null)
                return field;

            AddDeclarationDiagnostics(diagnostics);
            _completionState.MarkPartComplete(CompletionPart.Type);
            return field;
        }
    }

    protected abstract TypeSymbol ComputeType(DiagnosticBag diagnostics);

    public sealed override bool IsMutable { get; }

    public sealed override bool HasInitializer => Syntax.Initializer is not null;

    public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Interlocked.CompareExchange(ref field, [new SyntaxReference(Syntax)], default);
            return field;
        }
    }

    public sealed override ConstantValue? ConstantValue
    {
        get
        {
            if (_constantValue is not null)
                return _constantValue.Value;

            var diagnostics = DiagnosticBag.Create();
            if (
                Interlocked.CompareExchange(
                    ref _constantValue,
                    new ConstantValueWrapper(ComputeConstantValue(diagnostics)),
                    null
                )
                is not null
            )
                return _constantValue.Value;

            AddDeclarationDiagnostics(diagnostics);
            _completionState.MarkPartComplete(CompletionPart.ConstantValue);
            return _constantValue.Value;
        }
    }

    protected abstract ConstantValue? ComputeConstantValue(DiagnosticBag diagnostics);

    public sealed override bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithin)
    {
        return IsDefinedInSourceTree(new SyntaxReference(Syntax), tree, definedWithin);
    }

    internal sealed override bool NeedsCompletion => false;

    internal sealed override void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    )
    {
        if (filter?.Invoke(this) == false)
            return;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var incompletePart = _completionState.NextIncompletePart;
            switch (incompletePart)
            {
                case CompletionPart.Type:
                    _ = Type;
                    break;
                case CompletionPart.ConstantValue:
                    _ = ConstantValue;
                    break;
                case CompletionPart.None:
                    return;
                default:
                    _completionState.MarkPartComplete(
                        CompletionPart.All & ~CompletionPart.VariableAll
                    );
                    break;
            }

            _completionState.WaitPartComplete(incompletePart, cancellationToken);
        }
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
        : base(name, containingSymbol, syntax)
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        compilation.CacheSymbol(Syntax, this);
    }

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
