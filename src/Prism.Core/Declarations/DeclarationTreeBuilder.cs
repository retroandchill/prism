using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Binding;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Declarations;

internal sealed class DeclarationTreeBuilder
{
    private readonly SyntaxTree _syntaxTree;
    private int _currentTypeIndex;

    private DeclarationTreeBuilder(SyntaxTree syntaxTree)
    {
        _syntaxTree = syntaxTree;
    }

    public static SingleRootNamespaceDeclaration ForTree(SyntaxTree tree)
    {
        var builder = new DeclarationTreeBuilder(tree);
        return builder.VisitTree();
    }

    private SingleRootNamespaceDeclaration VisitTree()
    {
        return _syntaxTree.Root is CompilationUnitSyntax compilationUnitSyntax
            ? VisitCompilationUnit(compilationUnitSyntax)
            : throw new InvalidOperationException("Compilation unit syntax expected");
    }

    private ImmutableArray<SingleDeclaration> VisitNamespaceChildren(
        SyntaxNode node,
        SyntaxList<DeclarationSyntax> members
    )
    {
        Debug.Assert(node is CompilationUnitSyntax or NamespaceDeclarationSyntax);

        if (members.Count == 0)
            return [];

        var builder = ImmutableArray.CreateBuilder<SingleDeclaration>();
        foreach (var member in members)
        {
            var namespaceOrType = member switch
            {
                NamespaceDeclarationSyntax ns => VisitNamespaceDeclaration(ns),
                _ => null,
            };

            if (namespaceOrType is not null)
                builder.Add(namespaceOrType);
        }

        return builder.DrainToImmutable();
    }

    private SingleRootNamespaceDeclaration VisitCompilationUnit(CompilationUnitSyntax syntax)
    {
        var children = VisitNamespaceChildren(syntax, syntax.Members);
        return CreateSingleRootNamespaceDeclaration(syntax, children);
    }

    private SingleDeclaration VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        var children = VisitNamespaceChildren(node, node.Members);

        var hasUsings = node.Usings.Count > 0;
        var name = node.Name;
        SyntaxNode? currentNode = node;
        var memberNames = GetNonTypeMemberNames(node.Members);
        while (name is QualifiedNameSyntax qualified)
        {
            var rightName = qualified.UnqualifiedName;
            var ns = new SingleNamedNamespaceDeclaration(
                rightName,
                new SyntaxReference(currentNode),
                new SourceLocation(qualified.Right),
                memberNames,
                children
            )
            {
                HasUsings = hasUsings,
            };

            memberNames = ImmutableHashSet<string>.Empty;
            children = [ns];
            name = qualified.Left;
            currentNode = name;
            hasUsings = false;
        }

        var diagnostics = DiagnosticBag.Create();
        switch (node)
        {
            case FileScopedNamespaceDeclarationSyntax:
                switch (node.Parent)
                {
                    case FileScopedNamespaceDeclarationSyntax:
                        diagnostics.Add(
                            Diagnostic.MultipleFileScopedNamespaces(node.Name.Location)
                        );
                        break;
                    case NamespaceDeclarationSyntax:
                        diagnostics.Add(
                            Diagnostic.FileScopedAndNormalNamespace(node.Name.Location)
                        );
                        break;
                    case CompilationUnitSyntax compilationUnit:
                    {
                        if (!ReferenceEquals(node, compilationUnit.Members[0]))
                        {
                            diagnostics.Add(
                                Diagnostic.FileScopedNamespaceNotBeforeAllMembers(
                                    node.Name.Location
                                )
                            );
                        }

                        break;
                    }
                    default:
                        throw new InvalidOperationException("This isn't a valid state");
                }

                break;
            case BlockNamespaceDeclarationSyntax:
            {
                if (node.Parent is FileScopedNamespaceDeclarationSyntax)
                {
                    diagnostics.Add(Diagnostic.FileScopedAndNormalNamespace(node.Name.Location));
                }

                break;
            }
            default:
                throw new InvalidOperationException("This isn't a valid state");
        }

        if (node.Modifiers.Count > 0)
        {
            diagnostics.Add(Diagnostic.BadModifiersOnNamespace(node.Modifiers[0].Location));
        }

        Debug.Assert(name is SimpleNameSyntax);
        var unqualifiedName = name.UnqualifiedName;
        return new SingleNamedNamespaceDeclaration(
            unqualifiedName,
            new SyntaxReference(currentNode),
            new SourceLocation(name),
            memberNames,
            children
        )
        {
            HasUsings = hasUsings,
            Diagnostics = diagnostics.ToImmutableAndClear(),
        };
    }

    private SingleRootNamespaceDeclaration CreateSingleRootNamespaceDeclaration(
        CompilationUnitSyntax syntax,
        ImmutableArray<SingleDeclaration> children
    )
    {
        return new SingleRootNamespaceDeclaration(
            new SyntaxReference(syntax),
            new SourceLocation(syntax),
            GetNonTypeMemberNames(syntax.Members),
            children
        )
        {
            HasUsings = syntax.Usings.Count > 0,
        };
    }

    private static ImmutableHashSet<string> GetNonTypeMemberNames(
        SyntaxList<DeclarationSyntax> members
    )
    {
        var memberNames = ImmutableHashSet.CreateBuilder<string>();
        foreach (var member in members)
        {
            switch (member)
            {
                case VariableDeclarationSyntax variable:
                    memberNames.Add(variable.Identifier.IdentifierName);
                    break;
                case FunctionDeclarationSyntax function:
                    memberNames.Add(function.Identifier.IdentifierName);
                    break;
            }
        }

        return memberNames.ToImmutable();
    }
}
