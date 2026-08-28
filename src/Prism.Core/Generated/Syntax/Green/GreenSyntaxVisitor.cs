// Generated file, do not edit
namespace Prism.Core.Syntax.Green;

internal abstract partial class GreenSyntaxVisitor
{
    public void Visit(GreenNode node)
    {
        switch (node)
        {
            case GreenListNode list:
                Visit(list);
                break;
            case GreenToken token:
                Visit(token);
                break;
            case GreenTrivia trivia:
                Visit(trivia);
                break;
            case GreenSkippedTokensTrivia skippedTokensTrivia:
                Visit(skippedTokensTrivia);
                break;
            case GreenCompilationUnit compilationUnit:
                Visit(compilationUnit);
                break;
            case GreenPredefinedType predefinedType:
                Visit(predefinedType);
                break;
            case GreenNamedType namedType:
                Visit(namedType);
                break;
            case GreenSimpleName simpleName:
                Visit(simpleName);
                break;
            case GreenQualifiedName qualifiedName:
                Visit(qualifiedName);
                break;
            case GreenInitializer initializer:
                Visit(initializer);
                break;
            case GreenTypeSpecifier typeSpecifier:
                Visit(typeSpecifier);
                break;
            case GreenNamedParameter namedParameter:
                Visit(namedParameter);
                break;
            case GreenArgumentList argumentList:
                Visit(argumentList);
                break;
            case GreenArgument argument:
                Visit(argument);
                break;
            case GreenParameterList parameterList:
                Visit(parameterList);
                break;
            case GreenParameter parameter:
                Visit(parameter);
                break;
            case GreenExpressionBody expressionBody:
                Visit(expressionBody);
                break;
            case GreenElseClause elseClause:
                Visit(elseClause);
                break;
            case GreenLiteralExpression literalExpression:
                Visit(literalExpression);
                break;
            case GreenIdentifierExpression identifierExpression:
                Visit(identifierExpression);
                break;
            case GreenParenthesizedExpression parenthesizedExpression:
                Visit(parenthesizedExpression);
                break;
            case GreenBinaryExpression binaryExpression:
                Visit(binaryExpression);
                break;
            case GreenAssignmentExpression assignmentExpression:
                Visit(assignmentExpression);
                break;
            case GreenPrefixExpression prefixExpression:
                Visit(prefixExpression);
                break;
            case GreenPostfixExpression postfixExpression:
                Visit(postfixExpression);
                break;
            case GreenTernaryExpression ternaryExpression:
                Visit(ternaryExpression);
                break;
            case GreenInvocationExpression invocationExpression:
                Visit(invocationExpression);
                break;
            case GreenCastExpression castExpression:
                Visit(castExpression);
                break;
            case GreenEmptyStatement emptyStatement:
                Visit(emptyStatement);
                break;
            case GreenVariableDeclarationStatement variableDeclarationStatement:
                Visit(variableDeclarationStatement);
                break;
            case GreenBlock block:
                Visit(block);
                break;
            case GreenReturnStatement returnStatement:
                Visit(returnStatement);
                break;
            case GreenExpressionStatement expressionStatement:
                Visit(expressionStatement);
                break;
            case GreenIfStatement ifStatement:
                Visit(ifStatement);
                break;
            case GreenWhileStatement whileStatement:
                Visit(whileStatement);
                break;
            case GreenLoopStatement loopStatement:
                Visit(loopStatement);
                break;
            case GreenForStatement forStatement:
                Visit(forStatement);
                break;
            case GreenBreakStatement breakStatement:
                Visit(breakStatement);
                break;
            case GreenContinueStatement continueStatement:
                Visit(continueStatement);
                break;
            case GreenIncompleteDeclaration incompleteDeclaration:
                Visit(incompleteDeclaration);
                break;
            case GreenBlockNamespaceDeclaration blockNamespaceDeclaration:
                Visit(blockNamespaceDeclaration);
                break;
            case GreenFileScopedNamespaceDeclaration fileScopedNamespaceDeclaration:
                Visit(fileScopedNamespaceDeclaration);
                break;
            case GreenVariableDeclaration variableDeclaration:
                Visit(variableDeclaration);
                break;
            case GreenFunctionDeclaration functionDeclaration:
                Visit(functionDeclaration);
                break;
            case GreenUsingDirective usingDirective:
                Visit(usingDirective);
                break;
            default:
                throw new InvalidOperationException("Invalid node type passed into visit");
        }
    }

    public void Visit(GreenStructuredTrivia node)
    {
        switch (node)
        {
            case GreenSkippedTokensTrivia skippedTokensTrivia:
                Visit(skippedTokensTrivia);
                break;
            default:
                throw new InvalidOperationException("Invalid node type passed into visit");
        }
    }

    public void Visit(GreenType node)
    {
        switch (node)
        {
            case GreenPredefinedType predefinedType:
                Visit(predefinedType);
                break;
            case GreenNamedType namedType:
                Visit(namedType);
                break;
            default:
                throw new InvalidOperationException("Invalid node type passed into visit");
        }
    }

    public void Visit(GreenName node)
    {
        switch (node)
        {
            case GreenSimpleName simpleName:
                Visit(simpleName);
                break;
            case GreenQualifiedName qualifiedName:
                Visit(qualifiedName);
                break;
            default:
                throw new InvalidOperationException("Invalid node type passed into visit");
        }
    }

    public void Visit(GreenExpression node)
    {
        switch (node)
        {
            case GreenLiteralExpression literalExpression:
                Visit(literalExpression);
                break;
            case GreenIdentifierExpression identifierExpression:
                Visit(identifierExpression);
                break;
            case GreenParenthesizedExpression parenthesizedExpression:
                Visit(parenthesizedExpression);
                break;
            case GreenBinaryExpression binaryExpression:
                Visit(binaryExpression);
                break;
            case GreenAssignmentExpression assignmentExpression:
                Visit(assignmentExpression);
                break;
            case GreenPrefixExpression prefixExpression:
                Visit(prefixExpression);
                break;
            case GreenPostfixExpression postfixExpression:
                Visit(postfixExpression);
                break;
            case GreenTernaryExpression ternaryExpression:
                Visit(ternaryExpression);
                break;
            case GreenInvocationExpression invocationExpression:
                Visit(invocationExpression);
                break;
            case GreenCastExpression castExpression:
                Visit(castExpression);
                break;
            default:
                throw new InvalidOperationException("Invalid node type passed into visit");
        }
    }

    public void Visit(GreenStatement node)
    {
        switch (node)
        {
            case GreenEmptyStatement emptyStatement:
                Visit(emptyStatement);
                break;
            case GreenVariableDeclarationStatement variableDeclarationStatement:
                Visit(variableDeclarationStatement);
                break;
            case GreenBlock block:
                Visit(block);
                break;
            case GreenReturnStatement returnStatement:
                Visit(returnStatement);
                break;
            case GreenExpressionStatement expressionStatement:
                Visit(expressionStatement);
                break;
            case GreenIfStatement ifStatement:
                Visit(ifStatement);
                break;
            case GreenWhileStatement whileStatement:
                Visit(whileStatement);
                break;
            case GreenLoopStatement loopStatement:
                Visit(loopStatement);
                break;
            case GreenForStatement forStatement:
                Visit(forStatement);
                break;
            case GreenBreakStatement breakStatement:
                Visit(breakStatement);
                break;
            case GreenContinueStatement continueStatement:
                Visit(continueStatement);
                break;
            default:
                throw new InvalidOperationException("Invalid node type passed into visit");
        }
    }

    public void Visit(GreenDeclaration node)
    {
        switch (node)
        {
            case GreenIncompleteDeclaration incompleteDeclaration:
                Visit(incompleteDeclaration);
                break;
            case GreenBlockNamespaceDeclaration blockNamespaceDeclaration:
                Visit(blockNamespaceDeclaration);
                break;
            case GreenFileScopedNamespaceDeclaration fileScopedNamespaceDeclaration:
                Visit(fileScopedNamespaceDeclaration);
                break;
            case GreenVariableDeclaration variableDeclaration:
                Visit(variableDeclaration);
                break;
            case GreenFunctionDeclaration functionDeclaration:
                Visit(functionDeclaration);
                break;
            default:
                throw new InvalidOperationException("Invalid node type passed into visit");
        }
    }

    public void Visit(GreenNamespaceDeclaration node)
    {
        switch (node)
        {
            case GreenBlockNamespaceDeclaration blockNamespaceDeclaration:
                Visit(blockNamespaceDeclaration);
                break;
            case GreenFileScopedNamespaceDeclaration fileScopedNamespaceDeclaration:
                Visit(fileScopedNamespaceDeclaration);
                break;
            default:
                throw new InvalidOperationException("Invalid node type passed into visit");
        }
    }

    public abstract void Visit(GreenListNode node);
    public abstract void Visit(GreenToken node);
    public abstract void Visit(GreenTrivia node);
    public abstract void Visit(GreenSkippedTokensTrivia node);
    public abstract void Visit(GreenCompilationUnit node);
    public abstract void Visit(GreenPredefinedType node);
    public abstract void Visit(GreenNamedType node);
    public abstract void Visit(GreenSimpleName node);
    public abstract void Visit(GreenQualifiedName node);
    public abstract void Visit(GreenInitializer node);
    public abstract void Visit(GreenTypeSpecifier node);
    public abstract void Visit(GreenNamedParameter node);
    public abstract void Visit(GreenArgumentList node);
    public abstract void Visit(GreenArgument node);
    public abstract void Visit(GreenParameterList node);
    public abstract void Visit(GreenParameter node);
    public abstract void Visit(GreenExpressionBody node);
    public abstract void Visit(GreenElseClause node);
    public abstract void Visit(GreenLiteralExpression node);
    public abstract void Visit(GreenIdentifierExpression node);
    public abstract void Visit(GreenParenthesizedExpression node);
    public abstract void Visit(GreenBinaryExpression node);
    public abstract void Visit(GreenAssignmentExpression node);
    public abstract void Visit(GreenPrefixExpression node);
    public abstract void Visit(GreenPostfixExpression node);
    public abstract void Visit(GreenTernaryExpression node);
    public abstract void Visit(GreenInvocationExpression node);
    public abstract void Visit(GreenCastExpression node);
    public abstract void Visit(GreenEmptyStatement node);
    public abstract void Visit(GreenVariableDeclarationStatement node);
    public abstract void Visit(GreenBlock node);
    public abstract void Visit(GreenReturnStatement node);
    public abstract void Visit(GreenExpressionStatement node);
    public abstract void Visit(GreenIfStatement node);
    public abstract void Visit(GreenWhileStatement node);
    public abstract void Visit(GreenLoopStatement node);
    public abstract void Visit(GreenForStatement node);
    public abstract void Visit(GreenBreakStatement node);
    public abstract void Visit(GreenContinueStatement node);
    public abstract void Visit(GreenIncompleteDeclaration node);
    public abstract void Visit(GreenBlockNamespaceDeclaration node);
    public abstract void Visit(GreenFileScopedNamespaceDeclaration node);
    public abstract void Visit(GreenVariableDeclaration node);
    public abstract void Visit(GreenFunctionDeclaration node);
    public abstract void Visit(GreenUsingDirective node);
}

internal abstract partial class GreenSyntaxVisitor<TResult>
{
    public TResult Visit(GreenNode node)
    {
        return node switch
        {
            GreenListNode list => Visit(list),
            GreenToken token => Visit(token),
            GreenTrivia trivia => Visit(trivia),
            GreenSkippedTokensTrivia skippedTokensTrivia => Visit(skippedTokensTrivia),
            GreenCompilationUnit compilationUnit => Visit(compilationUnit),
            GreenPredefinedType predefinedType => Visit(predefinedType),
            GreenNamedType namedType => Visit(namedType),
            GreenSimpleName simpleName => Visit(simpleName),
            GreenQualifiedName qualifiedName => Visit(qualifiedName),
            GreenInitializer initializer => Visit(initializer),
            GreenTypeSpecifier typeSpecifier => Visit(typeSpecifier),
            GreenNamedParameter namedParameter => Visit(namedParameter),
            GreenArgumentList argumentList => Visit(argumentList),
            GreenArgument argument => Visit(argument),
            GreenParameterList parameterList => Visit(parameterList),
            GreenParameter parameter => Visit(parameter),
            GreenExpressionBody expressionBody => Visit(expressionBody),
            GreenElseClause elseClause => Visit(elseClause),
            GreenLiteralExpression literalExpression => Visit(literalExpression),
            GreenIdentifierExpression identifierExpression => Visit(identifierExpression),
            GreenParenthesizedExpression parenthesizedExpression => Visit(parenthesizedExpression),
            GreenBinaryExpression binaryExpression => Visit(binaryExpression),
            GreenAssignmentExpression assignmentExpression => Visit(assignmentExpression),
            GreenPrefixExpression prefixExpression => Visit(prefixExpression),
            GreenPostfixExpression postfixExpression => Visit(postfixExpression),
            GreenTernaryExpression ternaryExpression => Visit(ternaryExpression),
            GreenInvocationExpression invocationExpression => Visit(invocationExpression),
            GreenCastExpression castExpression => Visit(castExpression),
            GreenEmptyStatement emptyStatement => Visit(emptyStatement),
            GreenVariableDeclarationStatement variableDeclarationStatement => Visit(
                variableDeclarationStatement
            ),
            GreenBlock block => Visit(block),
            GreenReturnStatement returnStatement => Visit(returnStatement),
            GreenExpressionStatement expressionStatement => Visit(expressionStatement),
            GreenIfStatement ifStatement => Visit(ifStatement),
            GreenWhileStatement whileStatement => Visit(whileStatement),
            GreenLoopStatement loopStatement => Visit(loopStatement),
            GreenForStatement forStatement => Visit(forStatement),
            GreenBreakStatement breakStatement => Visit(breakStatement),
            GreenContinueStatement continueStatement => Visit(continueStatement),
            GreenIncompleteDeclaration incompleteDeclaration => Visit(incompleteDeclaration),
            GreenBlockNamespaceDeclaration blockNamespaceDeclaration => Visit(
                blockNamespaceDeclaration
            ),
            GreenFileScopedNamespaceDeclaration fileScopedNamespaceDeclaration => Visit(
                fileScopedNamespaceDeclaration
            ),
            GreenVariableDeclaration variableDeclaration => Visit(variableDeclaration),
            GreenFunctionDeclaration functionDeclaration => Visit(functionDeclaration),
            GreenUsingDirective usingDirective => Visit(usingDirective),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    public TResult Visit(GreenStructuredTrivia node)
    {
        return node switch
        {
            GreenSkippedTokensTrivia skippedTokensTrivia => Visit(skippedTokensTrivia),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    public TResult Visit(GreenType node)
    {
        return node switch
        {
            GreenPredefinedType predefinedType => Visit(predefinedType),
            GreenNamedType namedType => Visit(namedType),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    public TResult Visit(GreenName node)
    {
        return node switch
        {
            GreenSimpleName simpleName => Visit(simpleName),
            GreenQualifiedName qualifiedName => Visit(qualifiedName),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    public TResult Visit(GreenExpression node)
    {
        return node switch
        {
            GreenLiteralExpression literalExpression => Visit(literalExpression),
            GreenIdentifierExpression identifierExpression => Visit(identifierExpression),
            GreenParenthesizedExpression parenthesizedExpression => Visit(parenthesizedExpression),
            GreenBinaryExpression binaryExpression => Visit(binaryExpression),
            GreenAssignmentExpression assignmentExpression => Visit(assignmentExpression),
            GreenPrefixExpression prefixExpression => Visit(prefixExpression),
            GreenPostfixExpression postfixExpression => Visit(postfixExpression),
            GreenTernaryExpression ternaryExpression => Visit(ternaryExpression),
            GreenInvocationExpression invocationExpression => Visit(invocationExpression),
            GreenCastExpression castExpression => Visit(castExpression),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    public TResult Visit(GreenStatement node)
    {
        return node switch
        {
            GreenEmptyStatement emptyStatement => Visit(emptyStatement),
            GreenVariableDeclarationStatement variableDeclarationStatement => Visit(
                variableDeclarationStatement
            ),
            GreenBlock block => Visit(block),
            GreenReturnStatement returnStatement => Visit(returnStatement),
            GreenExpressionStatement expressionStatement => Visit(expressionStatement),
            GreenIfStatement ifStatement => Visit(ifStatement),
            GreenWhileStatement whileStatement => Visit(whileStatement),
            GreenLoopStatement loopStatement => Visit(loopStatement),
            GreenForStatement forStatement => Visit(forStatement),
            GreenBreakStatement breakStatement => Visit(breakStatement),
            GreenContinueStatement continueStatement => Visit(continueStatement),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    public TResult Visit(GreenDeclaration node)
    {
        return node switch
        {
            GreenIncompleteDeclaration incompleteDeclaration => Visit(incompleteDeclaration),
            GreenBlockNamespaceDeclaration blockNamespaceDeclaration => Visit(
                blockNamespaceDeclaration
            ),
            GreenFileScopedNamespaceDeclaration fileScopedNamespaceDeclaration => Visit(
                fileScopedNamespaceDeclaration
            ),
            GreenVariableDeclaration variableDeclaration => Visit(variableDeclaration),
            GreenFunctionDeclaration functionDeclaration => Visit(functionDeclaration),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    public TResult Visit(GreenNamespaceDeclaration node)
    {
        return node switch
        {
            GreenBlockNamespaceDeclaration blockNamespaceDeclaration => Visit(
                blockNamespaceDeclaration
            ),
            GreenFileScopedNamespaceDeclaration fileScopedNamespaceDeclaration => Visit(
                fileScopedNamespaceDeclaration
            ),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    public abstract TResult Visit(GreenListNode node);
    public abstract TResult Visit(GreenToken node);
    public abstract TResult Visit(GreenTrivia node);
    public abstract TResult Visit(GreenSkippedTokensTrivia node);
    public abstract TResult Visit(GreenCompilationUnit node);
    public abstract TResult Visit(GreenPredefinedType node);
    public abstract TResult Visit(GreenNamedType node);
    public abstract TResult Visit(GreenSimpleName node);
    public abstract TResult Visit(GreenQualifiedName node);
    public abstract TResult Visit(GreenInitializer node);
    public abstract TResult Visit(GreenTypeSpecifier node);
    public abstract TResult Visit(GreenNamedParameter node);
    public abstract TResult Visit(GreenArgumentList node);
    public abstract TResult Visit(GreenArgument node);
    public abstract TResult Visit(GreenParameterList node);
    public abstract TResult Visit(GreenParameter node);
    public abstract TResult Visit(GreenExpressionBody node);
    public abstract TResult Visit(GreenElseClause node);
    public abstract TResult Visit(GreenLiteralExpression node);
    public abstract TResult Visit(GreenIdentifierExpression node);
    public abstract TResult Visit(GreenParenthesizedExpression node);
    public abstract TResult Visit(GreenBinaryExpression node);
    public abstract TResult Visit(GreenAssignmentExpression node);
    public abstract TResult Visit(GreenPrefixExpression node);
    public abstract TResult Visit(GreenPostfixExpression node);
    public abstract TResult Visit(GreenTernaryExpression node);
    public abstract TResult Visit(GreenInvocationExpression node);
    public abstract TResult Visit(GreenCastExpression node);
    public abstract TResult Visit(GreenEmptyStatement node);
    public abstract TResult Visit(GreenVariableDeclarationStatement node);
    public abstract TResult Visit(GreenBlock node);
    public abstract TResult Visit(GreenReturnStatement node);
    public abstract TResult Visit(GreenExpressionStatement node);
    public abstract TResult Visit(GreenIfStatement node);
    public abstract TResult Visit(GreenWhileStatement node);
    public abstract TResult Visit(GreenLoopStatement node);
    public abstract TResult Visit(GreenForStatement node);
    public abstract TResult Visit(GreenBreakStatement node);
    public abstract TResult Visit(GreenContinueStatement node);
    public abstract TResult Visit(GreenIncompleteDeclaration node);
    public abstract TResult Visit(GreenBlockNamespaceDeclaration node);
    public abstract TResult Visit(GreenFileScopedNamespaceDeclaration node);
    public abstract TResult Visit(GreenVariableDeclaration node);
    public abstract TResult Visit(GreenFunctionDeclaration node);
    public abstract TResult Visit(GreenUsingDirective node);
}
