using System.Collections.Frozen;
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
        var mappings = new Dictionary<SyntaxNode, Binder>(ReferenceEqualityComparer.Instance)
        {
            [syntax] = this,
        };
        CollectScopes(syntax, this, mappings);
        return mappings.ToFrozenDictionary(ReferenceEqualityComparer.Instance);
    }

    // ReSharper disable TailRecursiveCall
    private static void CollectScopes(
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
                CollectScopes(whileStatement.Block, enclosingBinder, mappings);
                break;
            }
            case LoopStatementSyntax loopStatement:
            {
                CollectScopes(loopStatement.Block, enclosingBinder, mappings);
                break;
            }
            case ForStatementSyntax forStatement:
            {
                var binder = new ForLoopBinder(enclosingBinder, forStatement);
                CollectScopes(forStatement, binder, mappings);
                break;
            }
        }
    }
    // ReSharper restore TailRecursiveCall
}
