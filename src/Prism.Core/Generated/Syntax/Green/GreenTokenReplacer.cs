// Generated file, do not edit
namespace Prism.Core.Syntax.Green;

internal abstract partial class GreenTokenReplacer
{
    public GreenNode Visit(GreenNode node)
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

    public GreenStructuredTrivia Visit(GreenStructuredTrivia node)
    {
        return node switch
        {
            GreenSkippedTokensTrivia skippedTokensTrivia => Visit(skippedTokensTrivia),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    public GreenType Visit(GreenType node)
    {
        return node switch
        {
            GreenPredefinedType predefinedType => Visit(predefinedType),
            GreenNamedType namedType => Visit(namedType),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    public GreenName Visit(GreenName node)
    {
        return node switch
        {
            GreenSimpleName simpleName => Visit(simpleName),
            GreenQualifiedName qualifiedName => Visit(qualifiedName),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    public GreenExpression Visit(GreenExpression node)
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

    public GreenStatement Visit(GreenStatement node)
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

    public GreenDeclaration Visit(GreenDeclaration node)
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

    public GreenNamespaceDeclaration Visit(GreenNamespaceDeclaration node)
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

    public GreenSkippedTokensTrivia Visit(GreenSkippedTokensTrivia node)
    {
        return node.Update(Visit(node.Tokens));
    }

    public GreenCompilationUnit Visit(GreenCompilationUnit node)
    {
        return node.Update(Visit(node.Usings), Visit(node.Members));
    }

    public GreenPredefinedType Visit(GreenPredefinedType node)
    {
        return node.Update(Visit(node.Keyword));
    }

    public GreenNamedType Visit(GreenNamedType node)
    {
        return node.Update(Visit(node.Identifier));
    }

    public GreenSimpleName Visit(GreenSimpleName node)
    {
        return node.Update(Visit(node.Identifier));
    }

    public GreenQualifiedName Visit(GreenQualifiedName node)
    {
        return node.Update(Visit(node.Left), Visit(node.Separator), Visit(node.Right));
    }

    public GreenInitializer Visit(GreenInitializer node)
    {
        return node.Update(Visit(node.EqualSign), Visit(node.Value));
    }

    public GreenTypeSpecifier Visit(GreenTypeSpecifier node)
    {
        return node.Update(Visit(node.Colon), Visit(node.Type));
    }

    public GreenNamedParameter Visit(GreenNamedParameter node)
    {
        return node.Update(Visit(node.Name), Visit(node.Colon));
    }

    public GreenArgumentList Visit(GreenArgumentList node)
    {
        return node.Update(Visit(node.OpenParen), Visit(node.Arguments), Visit(node.CloseParen));
    }

    public GreenArgument Visit(GreenArgument node)
    {
        return node.Update(node.Name is not null ? Visit(node.Name) : null, Visit(node.Value));
    }

    public GreenParameterList Visit(GreenParameterList node)
    {
        return node.Update(Visit(node.OpenParen), Visit(node.Parameters), Visit(node.CloseParen));
    }

    public GreenParameter Visit(GreenParameter node)
    {
        return node.Update(
            node.MutableKeyword is not null ? Visit(node.MutableKeyword) : null,
            Visit(node.Name),
            node.TypeSpecifier is not null ? Visit(node.TypeSpecifier) : null,
            node.DefaultValue is not null ? Visit(node.DefaultValue) : null
        );
    }

    public GreenExpressionBody Visit(GreenExpressionBody node)
    {
        return node.Update(Visit(node.Arrow), Visit(node.Expression));
    }

    public GreenElseClause Visit(GreenElseClause node)
    {
        return node.Update(Visit(node.ElseKeyword), Visit(node.Statement));
    }

    public GreenLiteralExpression Visit(GreenLiteralExpression node)
    {
        return node.Update(Visit(node.Value));
    }

    public GreenIdentifierExpression Visit(GreenIdentifierExpression node)
    {
        return node.Update(Visit(node.Value));
    }

    public GreenParenthesizedExpression Visit(GreenParenthesizedExpression node)
    {
        return node.Update(Visit(node.Open), Visit(node.Expression), Visit(node.Close));
    }

    public GreenBinaryExpression Visit(GreenBinaryExpression node)
    {
        return node.Update(Visit(node.Left), Visit(node.Op), Visit(node.Right));
    }

    public GreenAssignmentExpression Visit(GreenAssignmentExpression node)
    {
        return node.Update(Visit(node.Left), Visit(node.Op), Visit(node.Right));
    }

    public GreenPrefixExpression Visit(GreenPrefixExpression node)
    {
        return node.Update(Visit(node.Op), Visit(node.Operand));
    }

    public GreenPostfixExpression Visit(GreenPostfixExpression node)
    {
        return node.Update(Visit(node.Operand), Visit(node.Op));
    }

    public GreenTernaryExpression Visit(GreenTernaryExpression node)
    {
        return node.Update(
            Visit(node.Condition),
            Visit(node.QuestionMark),
            Visit(node.WhenTrue),
            Visit(node.Colon),
            Visit(node.WhenFalse)
        );
    }

    public GreenInvocationExpression Visit(GreenInvocationExpression node)
    {
        return node.Update(Visit(node.Callee), Visit(node.Arguments));
    }

    public GreenCastExpression Visit(GreenCastExpression node)
    {
        return node.Update(Visit(node.Operand), Visit(node.AsKeyword), Visit(node.Type));
    }

    public GreenEmptyStatement Visit(GreenEmptyStatement node)
    {
        return node.Update(Visit(node.Semicolon));
    }

    public GreenVariableDeclarationStatement Visit(GreenVariableDeclarationStatement node)
    {
        return node.Update(Visit(node.Declaration));
    }

    public GreenBlock Visit(GreenBlock node)
    {
        return node.Update(Visit(node.OpenBrace), Visit(node.Statements), Visit(node.CloseBrace));
    }

    public GreenReturnStatement Visit(GreenReturnStatement node)
    {
        return node.Update(
            Visit(node.ReturnKeyword),
            node.Expression is not null ? Visit(node.Expression) : null,
            Visit(node.Semicolon)
        );
    }

    public GreenExpressionStatement Visit(GreenExpressionStatement node)
    {
        return node.Update(Visit(node.Expression), Visit(node.Semicolon));
    }

    public GreenIfStatement Visit(GreenIfStatement node)
    {
        return node.Update(
            Visit(node.IfKeyword),
            Visit(node.OpenParen),
            Visit(node.Condition),
            Visit(node.CloseParen),
            Visit(node.Block),
            node.ElseClause is not null ? Visit(node.ElseClause) : null
        );
    }

    public GreenWhileStatement Visit(GreenWhileStatement node)
    {
        return node.Update(
            Visit(node.WhileKeyword),
            Visit(node.OpenParen),
            Visit(node.Condition),
            Visit(node.CloseParen),
            Visit(node.Block)
        );
    }

    public GreenLoopStatement Visit(GreenLoopStatement node)
    {
        return node.Update(Visit(node.LoopKeyword), Visit(node.Block));
    }

    public GreenForStatement Visit(GreenForStatement node)
    {
        return node.Update(
            Visit(node.ForKeyword),
            Visit(node.OpenParen),
            node.Declaration is not null ? Visit(node.Declaration) : null,
            Visit(node.Initializers),
            Visit(node.FirstSemicolon),
            node.Condition is not null ? Visit(node.Condition) : null,
            Visit(node.SecondSemicolon),
            Visit(node.Incrementors),
            Visit(node.CloseParen),
            Visit(node.Block)
        );
    }

    public GreenBreakStatement Visit(GreenBreakStatement node)
    {
        return node.Update(Visit(node.Keyword), Visit(node.Semicolon));
    }

    public GreenContinueStatement Visit(GreenContinueStatement node)
    {
        return node.Update(Visit(node.Keyword), Visit(node.Semicolon));
    }

    public GreenIncompleteDeclaration Visit(GreenIncompleteDeclaration node)
    {
        return node.Update(Visit(node.Modifiers));
    }

    public GreenBlockNamespaceDeclaration Visit(GreenBlockNamespaceDeclaration node)
    {
        return node.Update(
            Visit(node.Modifiers),
            Visit(node.NamespaceToken),
            Visit(node.Name),
            Visit(node.OpenBrace),
            Visit(node.Usings),
            Visit(node.Members),
            Visit(node.CloseBrace)
        );
    }

    public GreenFileScopedNamespaceDeclaration Visit(GreenFileScopedNamespaceDeclaration node)
    {
        return node.Update(
            Visit(node.Modifiers),
            Visit(node.NamespaceToken),
            Visit(node.Name),
            Visit(node.Semicolon),
            Visit(node.Usings),
            Visit(node.Members)
        );
    }

    public GreenVariableDeclaration Visit(GreenVariableDeclaration node)
    {
        return node.Update(
            Visit(node.Modifiers),
            Visit(node.VarKeyword),
            Visit(node.Identifier),
            node.Type is not null ? Visit(node.Type) : null,
            node.Initializer is not null ? Visit(node.Initializer) : null,
            Visit(node.Semicolon)
        );
    }

    public GreenFunctionDeclaration Visit(GreenFunctionDeclaration node)
    {
        return node.Update(
            Visit(node.Modifiers),
            Visit(node.FuncKeyword),
            Visit(node.Identifier),
            Visit(node.Parameters),
            node.ReturnType is not null ? Visit(node.ReturnType) : null,
            node.Body is not null ? Visit(node.Body) : null,
            node.ExpressionBody is not null ? Visit(node.ExpressionBody) : null,
            node.Semicolon is not null ? Visit(node.Semicolon) : null
        );
    }

    public GreenUsingDirective Visit(GreenUsingDirective node)
    {
        return node.Update(Visit(node.UsingKeyword), Visit(node.Name), Visit(node.Semicolon));
    }
}
