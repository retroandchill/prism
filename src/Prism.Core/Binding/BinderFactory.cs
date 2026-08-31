using System.Collections.Concurrent;
using Prism.Core.Compiling;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Binding;

internal sealed class BinderFactory(Compilation compilation, SyntaxTree syntaxTree)
{
    private readonly TerminalBinder _terminalBinder = new(compilation);
    private readonly ConcurrentDictionary<SyntaxNode, Binder> _declarationBinders = new(
        ReferenceEqualityComparer.Instance
    );
    private readonly ConcurrentDictionary<SyntaxNode, ExecutableCodeBinder> _executableBinders =
        new(ReferenceEqualityComparer.Instance);

    public Binder GetBinder(SyntaxNode node)
    {
        var executableRoot = FindContainingExecutableRoot(node);
        if (executableRoot is null)
            return GetEnclosingDeclarationBinder(node);

        var executableBinder = _executableBinders.GetOrAdd(executableRoot, CreateExecutableBinder);
        return executableBinder.GetRequiredBinder(node);
    }

    private ExecutableCodeBinder CreateExecutableBinder(SyntaxNode executableRoot)
    {
        var owningDeclaration = GetExecutableOwner(executableRoot);
        var signatureBinder = GetDeclarationBinder(owningDeclaration);
        return new ExecutableCodeBinder(signatureBinder, executableRoot);
    }

    private Binder GetEnclosingDeclarationBinder(SyntaxNode node)
    {
        for (var current = node; current is not null; current = current.Parent)
        {
            if (IsDeclarationScopeNode(current))
                return GetDeclarationBinder(current);
        }

        return _terminalBinder;
    }

    private Binder GetDeclarationBinder(SyntaxNode declarationNode)
    {
        return _declarationBinders.GetOrAdd(declarationNode, CreateDeclarationBinder);
    }

    private static SyntaxNode? FindContainingExecutableRoot(SyntaxNode node)
    {
        for (var current = node; current is not null; current = current.Parent)
        {
            if (IsExecutableRoot(current))
                return current;
        }

        return null;
    }

    private static bool IsExecutableRoot(SyntaxNode node)
    {
        return node switch
        {
            BlockSyntax block when IsFunctionLikeBody(block) => true,
            ExpressionBodySyntax => true,
            _ => false,
        };
    }

    private static bool IsFunctionLikeBody(BlockSyntax block)
    {
        // This should be the only case where a block is executable
        return block.Parent is FunctionDeclarationSyntax;
    }

    private static SyntaxNode GetExecutableOwner(SyntaxNode node)
    {
        return node.Parent ?? throw new InvalidOperationException("No executable owner found");
    }

    private static bool IsDeclarationScopeNode(SyntaxNode node)
    {
        return node
            is CompilationUnitSyntax
                or NamespaceDeclarationSyntax
                or FunctionDeclarationSyntax;
    }

    private Binder CreateDeclarationBinder(SyntaxNode node)
    {
        return node switch
        {
            CompilationUnitSyntax compilationUnit => new CompilationUnitBinder(
                _terminalBinder,
                compilationUnit
            ),
            NamespaceDeclarationSyntax namespaceDecl => new MemberContainerBinder(
                GetEnclosingDeclarationBinder(namespaceDecl.Parent!),
                GetDeclaredNamespaceSymbol(namespaceDecl),
                namespaceDecl
            ),
            FunctionDeclarationSyntax functionDecl => new FunctionSignatureBinder(
                GetEnclosingDeclarationBinder(functionDecl.Parent!),
                GetDeclaredFunctionSymbol(functionDecl),
                functionDecl
            ),
            _ => throw new InvalidOperationException("Unexpected declaration node"),
        };
    }

    private NamespaceSymbol GetDeclaredNamespaceSymbol(NamespaceDeclarationSyntax namespaceDecl)
    {
        return compilation.GetSemanticModel(syntaxTree).GetDeclaredSymbol(namespaceDecl)
            ?? throw new InvalidOperationException("Namespace is not declared");
    }

    private FunctionSymbol GetDeclaredFunctionSymbol(FunctionDeclarationSyntax functionDecl)
    {
        return compilation.GetSemanticModel(syntaxTree).GetDeclaredSymbol(functionDecl)
            ?? throw new InvalidOperationException("Function is not declared");
    }
}
