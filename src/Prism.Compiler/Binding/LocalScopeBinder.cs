using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Compiler.Symbols;
using Prism.Compiler.Symbols.Source;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using Prism.Syntax;
using ZLinq;

namespace Prism.Compiler.Binding;

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
        ReadOnlySpan<SyntaxNode> statements,
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
        SyntaxNode statement,
        ImmutableArray<VariableSymbol>.Builder variables
    )
    {
        switch (statement)
        {
            case VariableDeclarationStatementSyntax local:
            {
                var declarationBinder = enclosingBinder.GetBinder(local) ?? enclosingBinder;
                variables.Add(MakeLocal(local.Declaration, declarationBinder));
                break;
            }
            case LocalVariableDeclarationSyntax loopVariable:
            {
                var declarationBinder = enclosingBinder.GetBinder(loopVariable) ?? enclosingBinder;
                variables.Add(MakeLocal(loopVariable, declarationBinder));
                break;
            }
        }
    }

    private VariableSymbol MakeLocal(
        LocalVariableDeclarationSyntax declaration,
        Binder? initializerBinder = null
    )
    {
        var containing = ContainingSymbol as FunctionSymbol;
        Debug.Assert(containing is not null);
        return new SourceLocalVariableSymbol(
            Compilation,
            containing,
            declaration,
            this,
            initializerBinder
        ).PublicSymbol;
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
