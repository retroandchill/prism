using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Prism.Compiler.Binding;
using Prism.Diagnostics;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Semantic.Symbols.Error;
using Prism.Syntax;
using Prism.Utils;

namespace Prism.Compiler.Symbols.Source;

internal sealed class SourceTypeAliasSymbol
    : SourceSymbol,
        IMemberContainerSymbolData,
        ITypeAliasSymbolData,
        ICompletableSymbolData
{
    private readonly TypeAliasDeclarationSyntax _syntax;
    private SymbolCompletionState _completionState;

    public SourceTypeAliasSymbol(
        Compilation compilation,
        MemberContainerSymbol container,
        TypeAliasDeclarationSyntax syntax
    )
        : base(compilation)
    {
        _syntax = syntax;
        ContainingMemberContainer = container;
        PublicSymbol = new TypeAliasSymbol(this);
    }

    public new TypeAliasSymbol PublicSymbol
    {
        get => Unsafe.As<TypeAliasSymbol>(base.PublicSymbol);
        private init => base.PublicSymbol = value;
    }

    public override string Name => _syntax.Identifier.IdentifierName;
    public override Symbol ContainingSymbol => ContainingMemberContainer;

    public override MemberContainerSymbol ContainingMemberContainer { get; }

    public override ImmutableArray<Location> Locations
    {
        get
        {
            return LazyComputation.LazyCompute(ref field, _syntax, static s =>
                [s.Identifier.Location]
            );
        }
    }

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
    {
        get
        {
            return LazyComputation.LazyCompute(ref field, _syntax, static s =>
                [new SyntaxReference(s)]
            );
        }
    }

    public TypeSymbol AliasedType
    {
        get
        {
            return LazySymbols.LazyCompute(
                ref field,
                this,
                static (@this, context) =>
                {
                    var factory = @this.DeclaringCompilation.GetBinderFactory(
                        @this._syntax.SyntaxTree
                    );
                    var binder = factory.GetBinder(@this._syntax);
                    if (@this._syntax.Type is not null)
                        return binder.ResolveType(@this._syntax.Type, context);

                    context.ReportDiagnostic(
                        Diagnostic.TypeAliasMustSpecifyType(
                            @this._syntax.Location,
                            @this.PublicSymbol.ToDisplayString()
                        )
                    );
                    return ErrorTypeSymbol.Unnamed.PublicSymbol;
                },
                static (@this, context) =>
                {
                    @this.AddDeclarationDiagnostics(context);
                    @this._completionState.MarkPartComplete(CompletionPart.Type);
                }
            );
        }
    }

    public bool IsDynamicallySized => AliasedType.IsDynamicallySized;
    public bool IsDefaultConstructible => AliasedType.IsDefaultConstructible;

    public ImmutableArray<Symbol> GetMembers() => AliasedType.GetMembers();

    public ImmutableArray<Symbol> GetMembers(ReadOnlySpan<char> name) =>
        AliasedType.GetMembers(name);

    public void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    )
    {
        if (filter?.Invoke(PublicSymbol) == false)
            return;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var incompletePart = _completionState.NextIncompletePart;
            switch (incompletePart)
            {
                case CompletionPart.Type:
                    _ = AliasedType;
                    break;
                case CompletionPart.None:
                    return;
                default:
                    _completionState.MarkPartComplete(
                        CompletionPart.All & ~CompletionPart.TypeAliasAll
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
