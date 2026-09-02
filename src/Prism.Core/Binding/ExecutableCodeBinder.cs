using System.Collections.Frozen;
using Prism.Core.Symbols;
using Prism.Core.Symbols.Source;
using Prism.Core.Symbols.Synthesized;
using Prism.Core.Syntax;

namespace Prism.Core.Binding;

internal sealed class ExecutableCodeBinder(Binder next, SyntaxNode syntax) : Binder(next)
{
    public override Binder? GetBinder(SyntaxNode node)
    {
        return BinderMappings.TryGetValue(node, out var binder) ? binder : base.GetBinder(node);
    }

    protected override LookupResult LookupLocal(
        string name,
        LookupOptions options,
        LookupContext context
    )
    {
        return LookupResult.NotFound();
    }

    public override LabelSymbol? LookupLoopLabel(string name, LookupContext context) => null;

    private FrozenDictionary<SyntaxNode, Binder> BinderMappings
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, ComputeBinderMappings(), null);
            return field;
        }
    }

    private FrozenDictionary<SyntaxNode, Binder> ComputeBinderMappings()
    {
        var mappings = new Dictionary<SyntaxNode, Binder>(ReferenceEqualityComparer.Instance);
        CollectScopes(syntax, this, mappings);
        return mappings.ToFrozenDictionary(ReferenceEqualityComparer.Instance);
    }

    // ReSharper disable TailRecursiveCall
    private void CollectScopes(
        SyntaxNode node,
        Binder enclosingBinder,
        Dictionary<SyntaxNode, Binder> mappings
    )
    {
        switch (node)
        {
            case BlockSyntax block:
            {
                var binder = new BlockBinder(enclosingBinder, block);
                mappings.Add(block, binder);

                foreach (var statement in block.Statements)
                {
                    CollectScopes(statement, binder, mappings);
                }

                break;
            }
            case IfStatementSyntax ifStatement:
            {
                CollectScopes(ifStatement.Block, enclosingBinder, mappings);
                if (ifStatement.ElseClause is not null)
                {
                    CollectScopes(ifStatement.ElseClause.Statement, enclosingBinder, mappings);
                }

                break;
            }
            case WhileStatementSyntax whileStatement:
            {
                var label = CreateLabel(whileStatement);
                var binder = new DefaultLoopBinder(enclosingBinder, label);
                mappings.Add(whileStatement, binder);
                CollectScopes(whileStatement.Block, binder, mappings);
                break;
            }
            case LoopStatementSyntax loopStatement:
            {
                var label = CreateLabel(loopStatement);
                var binder = new DefaultLoopBinder(enclosingBinder, label);
                mappings.Add(loopStatement, binder);
                CollectScopes(loopStatement.Block, binder, mappings);
                break;
            }
            case ForStatementSyntax forStatement:
            {
                var label = CreateLabel(forStatement);
                var binder = new ForLoopBinder(enclosingBinder, forStatement, label);
                mappings.Add(forStatement, binder);
                CollectScopes(forStatement.Block, binder, mappings);
                break;
            }
            case LabeledStatementSyntax labeled:
                CollectScopes(labeled.Statement, enclosingBinder, mappings);
                break;
        }
    }

    // ReSharper restore TailRecursiveCall

    private LabelSymbol CreateLabel(StatementSyntax statement)
    {
        if (ContainingSymbol is not FunctionSymbol function)
        {
            throw new InvalidOperationException(
                "Cannot create a label for a statement outside of a function."
            );
        }

        return statement.Parent is not LabeledStatementSyntax labeledStatement
            ? new SynthesizedLabelSymbol(function)
            : new SourceLabelSymbol(function, labeledStatement);
    }
}
