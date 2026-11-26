using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Syntax;
using Prism.Text;
using TargetInfo = Prism.Configuration.TargetInfo;

namespace Prism.Compiler.Symbols.Source;

internal sealed class SourceAssemblySymbol
    : SourceSymbol,
        IAssemblySymbolData,
        ICompletableSymbolData
{
    private SymbolCompletionState _completionState;
    private readonly DeclaredSymbolLocator _declaredSymbolLocator;

    public SourceAssemblySymbol(Compilation compilation, AssemblyReferences? references = null)
        : base(compilation)
    {
        References = references ?? AssemblyReferences.Empty;
        PublicSymbol = new AssemblySymbol(this);
        _declaredSymbolLocator = new DeclaredSymbolLocator(PublicSymbol);

        if (compilation.Settings.BuildingCoreLibrary)
        {
            StandardLibrary = PublicSymbol;
        }
        else
        {
            StandardLibrary = References
                .UnifiedAssemblies.Select(a => a.TargetAssembly)
                .FirstOrDefault(a => a.Name == CommonNames.StdLib)
                ?? new MissingAssemblySymbol(
                    new AssemblyIdentity(CommonNames.StdLib),
                    TargetInfo
                ).PublicSymbol;
        }
    }

    public new AssemblySymbol PublicSymbol
    {
        get => Unsafe.As<AssemblySymbol>(base.PublicSymbol);
        private init => base.PublicSymbol = value;
    }

    public override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedCompareExchange(
                ref field,
                [
                    .. DeclaringCompilation.MergedRootDeclaration.Declarations.Select(
                        Location (d) => d.Location
                    ),
                ],
                default
            );
            return field;
        }
    }

    public override AssemblySymbol? ContainingAssembly => null;

    public override Symbol? ContainingSymbol => null;
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override bool IsDefinedInSourceTree(ISourceDocument tree, TextSpan? definedWithin)
    {
        return false;
    }

    public AssemblyIdentity Identity
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                new AssemblyIdentity(DeclaringCompilation.AssemblyName),
                null
            );
            return field;
        }
    }

    public TargetInfo TargetInfo => DeclaringCompilation.TargetInfo;
    public AssemblySymbol StandardLibrary { get; }
    public AssemblyReferences References { get; }

    public NamespaceSymbol GlobalNamespace
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                new SourceNamespaceSymbol(
                    DeclaringCompilation,
                    DeclaringCompilation.MergedRootDeclaration,
                    PublicSymbol
                ).PublicSymbol,
                null
            );
            return field;
        }
    }

    public NamedTypeSymbol GetDeclaredSpecialType(SpecialType specialType)
    {
        return _declaredSymbolLocator.GetDeclaredSpecialType(specialType);
    }

    public override string Name => Identity.Name;

    public void ForceComplete(
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
                    if (!IsComplete(CompletionPart.Members))
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

    public bool IsComplete(CompletionPart part)
    {
        return _completionState.IsComplete(part);
    }
}
