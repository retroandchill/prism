using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Symbols.Source;
using Prism.Core.Syntax;
using ZLinq;

namespace Prism.Core.Binding;

internal abstract class LocalScopeBinder(Binder next) : Binder(next)
{
    public ImmutableArray<VariableSymbol> LocalVariables
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedCompareExchange(
                ref field,
                BuildLocalVariables(),
                default
            );
            return field;
        }
    }

    protected sealed override void EnsureLocals()
    {
        _ = LocalVariables;
        base.EnsureLocals();
    }

    protected abstract ImmutableArray<VariableSymbol> BuildLocalVariables();

    protected ImmutableArray<VariableSymbol> BuildLocalVariables(
        SyntaxList<StatementSyntax> statements,
        Binder enclosingBinder
    )
    {
        var builder = ImmutableArray.CreateBuilder<VariableSymbol>();
        foreach (var statement in statements)
        {
            BuildLocalVariables(enclosingBinder, statement, builder);
        }

        return builder.DrainToImmutable();
    }

    protected ImmutableArray<VariableSymbol> BuildLocalVariables(
        ReadOnlySpan<StatementSyntax> statements,
        Binder enclosingBinder
    )
    {
        var builder = ImmutableArray.CreateBuilder<VariableSymbol>();
        foreach (var statement in statements)
        {
            BuildLocalVariables(enclosingBinder, statement, builder);
        }

        return builder.DrainToImmutable();
    }

    private void BuildLocalVariables(
        Binder enclosingBinder,
        StatementSyntax statement,
        ImmutableArray<VariableSymbol>.Builder variables
    )
    {
        if (statement is not VariableDeclarationStatementSyntax local)
            return;

        var declarationBinder = enclosingBinder.GetBinder(local) ?? enclosingBinder;
        variables.Add(MakeLocal(local.Declaration, declarationBinder));
    }

    private SourceLocalVariableSymbol MakeLocal(
        VariableDeclarationSyntax declaration,
        Binder? initializerBinder = null
    )
    {
        var name = declaration.Identifier.IdentifierName;
        var containing = ContainingSymbol;
        Debug.Assert(containing is not null);
        return new SourceLocalVariableSymbol(
            name,
            containing,
            declaration,
            this,
            initializerBinder
        );
    }

    protected sealed override LookupResult LookupLocal(
        string name,
        LookupOptions options,
        BindingContext context
    )
    {
        if (!options.HasFlag(LookupOptions.Value))
            return LookupResult.NotFound();

        var found = LocalVariables
            .AsValueEnumerable()
            .Where(s => s.Name == name)
            .ToImmutableArray();
        return MakeLookupResult(ImmutableArray<Symbol>.CastUp(found), LookupOptions.Value);
    }

    public override LabelSymbol? LookupLoopLabel(string name, BindingContext context)
    {
        Debug.Assert(Next is not null);
        return Next.LookupLoopLabel(name, context);
    }
}
