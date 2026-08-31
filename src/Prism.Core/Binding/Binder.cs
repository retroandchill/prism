using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NetEscapades.EnumGenerators;
using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Mappers;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Binding;

internal abstract class Binder
{
    protected Binder(Compilation compilation)
    {
        Compilation = compilation;
    }

    protected Binder(Binder next, ConversionClassifier? conversionClassifier = null)
    {
        Compilation = next.Compilation;
        Next = next;
        ConversionClassifier = conversionClassifier;
    }

    public Compilation Compilation { get; }

    public Binder? Next { get; }

    public Binder GetNextChecked()
    {
        return Next ?? throw new InvalidOperationException("No next binder");
    }

    public ConversionClassifier ConversionClassifier
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, new ConversionClassifier(this), null);
            return field;
        }
    }

    public virtual Binder? GetBinder(SyntaxNode node)
    {
        Debug.Assert(Next is not null);
        return Next.GetBinder(node);
    }

    public Binder GetBinderChecked(SyntaxNode node)
    {
        return GetBinder(node) ?? throw new InvalidOperationException("No binder");
    }

    public virtual Symbol? ContainingSymbol
    {
        get
        {
            Debug.Assert(Next is not null);
            return Next.ContainingSymbol;
        }
    }

    public virtual SyntaxNode? ScopeDesignator => null;

    public virtual ImmutableArray<VariableSymbol> GetDeclaredLocalVariablesForScope(
        SyntaxNode designator
    )
    {
        Debug.Assert(Next is not null);
        return Next.GetDeclaredLocalVariablesForScope(designator);
    }

    public BoundStatement BindStatement(
        StatementSyntax syntax,
        TypeSymbol returnType,
        LookupContext context
    )
    {
        throw new NotImplementedException();
    }

    public BoundStatement BindExpressionBody(
        ExpressionBodySyntax syntax,
        TypeSymbol returnType,
        LookupContext context
    )
    {
        throw new NotImplementedException();
    }

    public BoundExpression BindExpression(ExpressionSyntax syntax, LookupContext context)
    {
        return BindExpression(syntax, null, context);
    }

    public BoundExpression BindExpression(
        ExpressionSyntax syntax,
        TypeSymbol? targetType,
        LookupContext context
    )
    {
        throw new NotImplementedException();
    }

    public TypeSymbol ResolveType(TypeSyntax syntax, LookupContext context)
    {
        return syntax switch
        {
            NamedTypeSyntax namedTypeSyntax => RequireType(
                LookupFromSyntax(namedTypeSyntax.Identifier, LookupOptions.Type, context),
                namedTypeSyntax.Identifier,
                context
            ),
            PredefinedTypeSyntax predefinedTypeSyntax => Compilation.GetSpecialType(
                predefinedTypeSyntax.Keyword.Kind.ToSpecialType()
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(syntax)),
        };
    }

    private TypeSymbol RequireType(LookupResult result, NameSyntax syntax, LookupContext context)
    {
        if (result is { IsViable: true, Symbol: TypeSymbol typeSymbol })
        {
            return typeSymbol;
        }

        BindingHelpers.DiagnoseLookupFailure(result, syntax, LookupOptions.Type, context);
        var names = BindingHelpers.CollectNames(syntax);
        return BindingHelpers.CreateErrorTypeSymbol(
            ContainingSymbol,
            Compilation,
            CollectionsMarshal.AsSpan(names)
        );
    }

    public LookupResult LookupFromSyntax(
        NameSyntax syntax,
        LookupOptions options,
        LookupContext context
    )
    {
        return syntax switch
        {
            SimpleNameSyntax simple => LookupFromSimpleName(simple, options, context),
            QualifiedNameSyntax qualified => LookupFromQualifiedName(qualified, options, context),
            _ => throw new ArgumentOutOfRangeException(nameof(syntax)),
        };
    }

    public LookupResult LookupUnqualifiedName(
        string name,
        LookupOptions options,
        LookupContext context
    )
    {
        if (options.HasFlag(LookupOptions.Callable))
        {
            var symbols = ImmutableArray.CreateBuilder<Symbol>();
            for (var binder = this; binder is not null; binder = binder.Next)
            {
                if (binder.LookupLocal(name, options, context) is not { Found: true } localResult)
                    continue;

                if (!localResult.IsViable || localResult.Symbols.All(s => s is FunctionSymbol))
                    return localResult;

                symbols.AddRange(localResult.Symbols);
            }

            return MakeLookupResult(symbols.DrainToImmutable(), options);
        }

        for (var binder = this; binder is not null; binder = binder.Next)
        {
            if (binder.LookupLocal(name, options, context) is { Found: true } localResult)
                return localResult;
        }

        return LookupResult.NotFound();
    }

    public LookupResult LookupQualifiedName(
        string name,
        MemberContainerSymbol container,
        LookupOptions options
    )
    {
        var symbols = ImmutableArray.CreateBuilder<Symbol>();
        foreach (var symbol in container.GetMembers(name).Where(VisibleFrom))
        {
            switch (symbol)
            {
                case VariableSymbol variable:
                    if (options.HasFlag(LookupOptions.Value))
                    {
                        symbols.Add(variable);
                    }

                    break;
                case FunctionSymbol function:
                    if (options.HasFlag(LookupOptions.Callable))
                    {
                        symbols.Add(function);
                    }

                    break;
                case NamespaceSymbol namespaceSymbol:
                    if (options.HasFlag(LookupOptions.Namespace))
                    {
                        symbols.Add(namespaceSymbol);
                    }

                    break;
                case TypeSymbol type:
                    if (options.HasFlag(LookupOptions.Type))
                    {
                        symbols.Add(type);
                    }

                    break;
            }
        }

        return MakeLookupResult(symbols.DrainToImmutable(), options);
    }

    private LookupResult LookupFromSimpleName(
        SimpleNameSyntax syntax,
        LookupOptions options,
        LookupContext context
    )
    {
        return LookupUnqualifiedName(syntax.UnqualifiedName, options, context);
    }

    private LookupResult LookupFromQualifiedName(
        QualifiedNameSyntax syntax,
        LookupOptions options,
        LookupContext context
    )
    {
        var lookupResult = LookupFromSyntax(syntax.Left, LookupOptions.NamespaceOrType, context);
        if (!lookupResult.IsViable)
            return lookupResult;

        return lookupResult.Symbol is not MemberContainerSymbol memberContainer
            ? LookupResult.WrongKind(lookupResult.Symbols)
            : LookupQualifiedName(syntax.UnqualifiedName, memberContainer, options);
    }

    protected virtual void EnsureLocals()
    {
        Debug.Assert(Next is not null);
        Next.EnsureLocals();
    }

    protected abstract LookupResult LookupLocal(
        string name,
        LookupOptions options,
        LookupContext context
    );

    private static LookupResult MakeLookupResult(
        ImmutableArray<Symbol> symbols,
        LookupOptions options
    )
    {
        if (symbols.IsEmpty)
            return LookupResult.NotFound();

        if (options.HasFlag(LookupOptions.Callable) || symbols.Length == 1)
            return LookupResult.Viable(symbols);

        return LookupResult.Ambiguous(symbols);
    }

    protected ImmutableArray<NamespaceSymbol> BuildUsingNamespaces(
        SyntaxList<UsingDirectiveSyntax> usings
    )
    {
        var result = ImmutableArray.CreateBuilder<NamespaceSymbol>(usings.Count);
        var seen = new HashSet<NamespaceSymbol>(ReferenceEqualityComparer.Instance);
        seen.EnsureCapacity(result.Capacity);

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var directive in usings)
        {
            var resolved = ResolveUsingNamespace(directive.Name);
            if (!seen.Add(resolved))
                continue;

            result.Add(resolved);
        }

        return result.DrainToImmutable();
    }

    private NamespaceSymbol ResolveUsingNamespace(NameSyntax syntax)
    {
        var diagnostics = DiagnosticBag.Create();
        var names = BindingHelpers.CollectNames(syntax);
        var current = Compilation.GlobalNamespace;
        var isError = false;
        foreach (var simple in names)
        {
            var name = simple.UnqualifiedName;
            var result = LookupQualifiedName(name, current, LookupOptions.Namespace);
            if (result.IsViable)
            {
                current = (NamespaceSymbol)result.Symbol;
            }
            else
            {
                current = Compilation.CreateErrorNamespaceSymbol(current, name);
                if (isError)
                    continue;
                diagnostics.Add(Diagnostic.UnresolvedSymbol(simple.Location, name));
                isError = true;
            }
        }

        if (isError)
        {
            Compilation.DeclarationDiagnostics.AddRange(diagnostics);
        }

        return current;
    }

    protected bool VisibleFrom(Symbol symbol)
    {
        switch (symbol.DeclaredVisibility)
        {
            case DeclaredVisibility.NotApplicable:
            case DeclaredVisibility.Public:
                return true;
            case DeclaredVisibility.Internal:
                return symbol.ContainingAssembly == Compilation.Assembly;
            case DeclaredVisibility.File:
            {
                var designator = ScopeDesignator;
                return designator is not null
                    && symbol.DeclaringSyntaxReferences.All(r =>
                        ReferenceEquals(r.SyntaxTree, designator.SyntaxTree)
                    );
            }
            default:
                throw new ArgumentException("Unexpected declared visibility", nameof(symbol));
        }
    }
}
