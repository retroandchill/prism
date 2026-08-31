using System.Collections.Immutable;
using Prism.Core.Compiling;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Source;

internal sealed class SourceAssemblySymbol(Compilation compilation)
    : AssemblySymbol(compilation.AssemblyName)
{
    private SymbolCompletionState _completionState = new();

    public override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Interlocked.CompareExchange(
                ref field,
                DeclaringCompilation
                    .MergedRootDeclaration.Declarations.Select(Location (d) => d.Location)
                    .ToImmutableArray(),
                default
            );
            return field;
        }
    }

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override NamespaceSymbol GlobalNamespace
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                new SourceNamespaceSymbol(DeclaringCompilation.MergedRootDeclaration, this, this),
                null
            );
            return field;
        }
    }

    internal override Compilation DeclaringCompilation { get; } = compilation;

    internal override bool NeedsCompletion => true;

    internal override void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    )
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var incompletePart = _completionState.NextIncompletePart;
            switch (incompletePart)
            {
                case CompletionPart.Members:
                    GlobalNamespace.ForceComplete(location, filter, cancellationToken);
                    if (IsComplete(CompletionPart.Members))
                    {
                        _completionState.MarkPartComplete(CompletionPart.Members);
                    }
                    break;
                case CompletionPart.None:
                    return;
                default:
                    // Any other values are for other kinds of symbols
                    _completionState.MarkPartComplete(
                        CompletionPart.All & ~CompletionPart.AssemblyAll
                    );
                    break;
            }

            _completionState.WaitPartComplete(incompletePart, cancellationToken);
        }
    }

    internal override bool IsComplete(CompletionPart part)
    {
        return _completionState.IsComplete(part);
    }
}
