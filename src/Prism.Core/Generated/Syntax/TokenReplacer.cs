// Generated file, do not edit
using System.Diagnostics.CodeAnalysis;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public static partial class TokenReplacer
{
    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenNode? ReplaceFirstToken(GreenNode? node, GreenToken newToken)
    {
        return node switch
        {
            null => null,
            GreenListNode list => ReplaceFirstToken(list, newToken),
            GreenToken token => ReplaceFirstToken(token, newToken),
            GreenTrivia trivia => ReplaceFirstToken(trivia, newToken),
            GreenSkippedTokensTrivia skippedTokensTrivia => ReplaceFirstToken(
                skippedTokensTrivia,
                newToken
            ),
            GreenCompilationUnit compilationUnit => ReplaceFirstToken(compilationUnit, newToken),
            GreenPredefinedType predefinedType => ReplaceFirstToken(predefinedType, newToken),
            GreenNamedType namedType => ReplaceFirstToken(namedType, newToken),
            GreenSimpleName simpleName => ReplaceFirstToken(simpleName, newToken),
            GreenQualifiedName qualifiedName => ReplaceFirstToken(qualifiedName, newToken),
            GreenInitializer initializer => ReplaceFirstToken(initializer, newToken),
            GreenTypeSpecifier typeSpecifier => ReplaceFirstToken(typeSpecifier, newToken),
            GreenNamedParameter namedParameter => ReplaceFirstToken(namedParameter, newToken),
            GreenArgumentList argumentList => ReplaceFirstToken(argumentList, newToken),
            GreenArgument argument => ReplaceFirstToken(argument, newToken),
            GreenParameterList parameterList => ReplaceFirstToken(parameterList, newToken),
            GreenParameter parameter => ReplaceFirstToken(parameter, newToken),
            GreenExpressionBody expressionBody => ReplaceFirstToken(expressionBody, newToken),
            GreenElseClause elseClause => ReplaceFirstToken(elseClause, newToken),
            GreenLiteralExpression literalExpression => ReplaceFirstToken(
                literalExpression,
                newToken
            ),
            GreenIdentifierExpression identifierExpression => ReplaceFirstToken(
                identifierExpression,
                newToken
            ),
            GreenParenthesizedExpression parenthesizedExpression => ReplaceFirstToken(
                parenthesizedExpression,
                newToken
            ),
            GreenBinaryExpression binaryExpression => ReplaceFirstToken(binaryExpression, newToken),
            GreenAssignmentExpression assignmentExpression => ReplaceFirstToken(
                assignmentExpression,
                newToken
            ),
            GreenPrefixExpression prefixExpression => ReplaceFirstToken(prefixExpression, newToken),
            GreenPostfixExpression postfixExpression => ReplaceFirstToken(
                postfixExpression,
                newToken
            ),
            GreenTernaryExpression ternaryExpression => ReplaceFirstToken(
                ternaryExpression,
                newToken
            ),
            GreenInvocationExpression invocationExpression => ReplaceFirstToken(
                invocationExpression,
                newToken
            ),
            GreenCastExpression castExpression => ReplaceFirstToken(castExpression, newToken),
            GreenEmptyStatement emptyStatement => ReplaceFirstToken(emptyStatement, newToken),
            GreenVariableDeclarationStatement variableDeclarationStatement => ReplaceFirstToken(
                variableDeclarationStatement,
                newToken
            ),
            GreenBlock block => ReplaceFirstToken(block, newToken),
            GreenReturnStatement returnStatement => ReplaceFirstToken(returnStatement, newToken),
            GreenExpressionStatement expressionStatement => ReplaceFirstToken(
                expressionStatement,
                newToken
            ),
            GreenIfStatement ifStatement => ReplaceFirstToken(ifStatement, newToken),
            GreenWhileStatement whileStatement => ReplaceFirstToken(whileStatement, newToken),
            GreenLoopStatement loopStatement => ReplaceFirstToken(loopStatement, newToken),
            GreenForStatement forStatement => ReplaceFirstToken(forStatement, newToken),
            GreenBreakStatement breakStatement => ReplaceFirstToken(breakStatement, newToken),
            GreenContinueStatement continueStatement => ReplaceFirstToken(
                continueStatement,
                newToken
            ),
            GreenLabeledStatement labeledStatement => ReplaceFirstToken(labeledStatement, newToken),
            GreenIncompleteDeclaration incompleteDeclaration => ReplaceFirstToken(
                incompleteDeclaration,
                newToken
            ),
            GreenBlockNamespaceDeclaration blockNamespaceDeclaration => ReplaceFirstToken(
                blockNamespaceDeclaration,
                newToken
            ),
            GreenFileScopedNamespaceDeclaration fileScopedNamespaceDeclaration => ReplaceFirstToken(
                fileScopedNamespaceDeclaration,
                newToken
            ),
            GreenVariableDeclaration variableDeclaration => ReplaceFirstToken(
                variableDeclaration,
                newToken
            ),
            GreenFunctionDeclaration functionDeclaration => ReplaceFirstToken(
                functionDeclaration,
                newToken
            ),
            GreenUsingDirective usingDirective => ReplaceFirstToken(usingDirective, newToken),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenStructuredTrivia? ReplaceFirstToken(
        GreenStructuredTrivia? node,
        GreenToken newToken
    )
    {
        return node switch
        {
            null => null,
            GreenSkippedTokensTrivia skippedTokensTrivia => ReplaceFirstToken(
                skippedTokensTrivia,
                newToken
            ),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenType? ReplaceFirstToken(GreenType? node, GreenToken newToken)
    {
        return node switch
        {
            null => null,
            GreenPredefinedType predefinedType => ReplaceFirstToken(predefinedType, newToken),
            GreenNamedType namedType => ReplaceFirstToken(namedType, newToken),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenName? ReplaceFirstToken(GreenName? node, GreenToken newToken)
    {
        return node switch
        {
            null => null,
            GreenSimpleName simpleName => ReplaceFirstToken(simpleName, newToken),
            GreenQualifiedName qualifiedName => ReplaceFirstToken(qualifiedName, newToken),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenExpression? ReplaceFirstToken(GreenExpression? node, GreenToken newToken)
    {
        return node switch
        {
            null => null,
            GreenLiteralExpression literalExpression => ReplaceFirstToken(
                literalExpression,
                newToken
            ),
            GreenIdentifierExpression identifierExpression => ReplaceFirstToken(
                identifierExpression,
                newToken
            ),
            GreenParenthesizedExpression parenthesizedExpression => ReplaceFirstToken(
                parenthesizedExpression,
                newToken
            ),
            GreenBinaryExpression binaryExpression => ReplaceFirstToken(binaryExpression, newToken),
            GreenAssignmentExpression assignmentExpression => ReplaceFirstToken(
                assignmentExpression,
                newToken
            ),
            GreenPrefixExpression prefixExpression => ReplaceFirstToken(prefixExpression, newToken),
            GreenPostfixExpression postfixExpression => ReplaceFirstToken(
                postfixExpression,
                newToken
            ),
            GreenTernaryExpression ternaryExpression => ReplaceFirstToken(
                ternaryExpression,
                newToken
            ),
            GreenInvocationExpression invocationExpression => ReplaceFirstToken(
                invocationExpression,
                newToken
            ),
            GreenCastExpression castExpression => ReplaceFirstToken(castExpression, newToken),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenStatement? ReplaceFirstToken(GreenStatement? node, GreenToken newToken)
    {
        return node switch
        {
            null => null,
            GreenEmptyStatement emptyStatement => ReplaceFirstToken(emptyStatement, newToken),
            GreenVariableDeclarationStatement variableDeclarationStatement => ReplaceFirstToken(
                variableDeclarationStatement,
                newToken
            ),
            GreenBlock block => ReplaceFirstToken(block, newToken),
            GreenReturnStatement returnStatement => ReplaceFirstToken(returnStatement, newToken),
            GreenExpressionStatement expressionStatement => ReplaceFirstToken(
                expressionStatement,
                newToken
            ),
            GreenIfStatement ifStatement => ReplaceFirstToken(ifStatement, newToken),
            GreenWhileStatement whileStatement => ReplaceFirstToken(whileStatement, newToken),
            GreenLoopStatement loopStatement => ReplaceFirstToken(loopStatement, newToken),
            GreenForStatement forStatement => ReplaceFirstToken(forStatement, newToken),
            GreenBreakStatement breakStatement => ReplaceFirstToken(breakStatement, newToken),
            GreenContinueStatement continueStatement => ReplaceFirstToken(
                continueStatement,
                newToken
            ),
            GreenLabeledStatement labeledStatement => ReplaceFirstToken(labeledStatement, newToken),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenDeclaration? ReplaceFirstToken(GreenDeclaration? node, GreenToken newToken)
    {
        return node switch
        {
            null => null,
            GreenIncompleteDeclaration incompleteDeclaration => ReplaceFirstToken(
                incompleteDeclaration,
                newToken
            ),
            GreenBlockNamespaceDeclaration blockNamespaceDeclaration => ReplaceFirstToken(
                blockNamespaceDeclaration,
                newToken
            ),
            GreenFileScopedNamespaceDeclaration fileScopedNamespaceDeclaration => ReplaceFirstToken(
                fileScopedNamespaceDeclaration,
                newToken
            ),
            GreenVariableDeclaration variableDeclaration => ReplaceFirstToken(
                variableDeclaration,
                newToken
            ),
            GreenFunctionDeclaration functionDeclaration => ReplaceFirstToken(
                functionDeclaration,
                newToken
            ),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenNamespaceDeclaration? ReplaceFirstToken(
        GreenNamespaceDeclaration? node,
        GreenToken newToken
    )
    {
        return node switch
        {
            null => null,
            GreenBlockNamespaceDeclaration blockNamespaceDeclaration => ReplaceFirstToken(
                blockNamespaceDeclaration,
                newToken
            ),
            GreenFileScopedNamespaceDeclaration fileScopedNamespaceDeclaration => ReplaceFirstToken(
                fileScopedNamespaceDeclaration,
                newToken
            ),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static partial GreenToken? ReplaceFirstToken(GreenToken? node, GreenToken newToken);

    [return: NotNullIfNotNull(nameof(node))]
    internal static partial GreenTrivia? ReplaceFirstToken(GreenTrivia? node, GreenToken newToken);

    [return: NotNullIfNotNull(nameof(node))]
    internal static partial GreenListNode? ReplaceFirstToken(
        GreenListNode? node,
        GreenToken newToken
    );

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenSkippedTokensTrivia? ReplaceFirstToken(
        GreenSkippedTokensTrivia? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldTokens = node.Tokens;
        var newTokens = ReplaceFirstToken(oldTokens, newToken);
        if (newTokens != oldTokens)
        {
            return node.WithTokens(newTokens);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenCompilationUnit? ReplaceFirstToken(
        GreenCompilationUnit? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldUsings = node.Usings;
        var newUsings = ReplaceFirstToken(oldUsings, newToken);
        if (newUsings != oldUsings)
        {
            return node.WithUsings(newUsings);
        }

        var oldMembers = node.Members;
        var newMembers = ReplaceFirstToken(oldMembers, newToken);
        if (newMembers != oldMembers)
        {
            return node.WithMembers(newMembers);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenPredefinedType? ReplaceFirstToken(
        GreenPredefinedType? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldKeyword = node.Keyword;
        var newKeyword = ReplaceFirstToken(oldKeyword, newToken);
        if (newKeyword != oldKeyword)
        {
            return node.WithKeyword(newKeyword);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenNamedType? ReplaceFirstToken(GreenNamedType? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldIdentifier = node.Identifier;
        var newIdentifier = ReplaceFirstToken(oldIdentifier, newToken);
        if (newIdentifier != oldIdentifier)
        {
            return node.WithIdentifier(newIdentifier);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenSimpleName? ReplaceFirstToken(GreenSimpleName? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldIdentifier = node.Identifier;
        var newIdentifier = ReplaceFirstToken(oldIdentifier, newToken);
        if (newIdentifier != oldIdentifier)
        {
            return node.WithIdentifier(newIdentifier);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenQualifiedName? ReplaceFirstToken(
        GreenQualifiedName? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldLeft = node.Left;
        var newLeft = ReplaceFirstToken(oldLeft, newToken);
        if (newLeft != oldLeft)
        {
            return node.WithLeft(newLeft);
        }

        var oldSeparator = node.Separator;
        var newSeparator = ReplaceFirstToken(oldSeparator, newToken);
        if (newSeparator != oldSeparator)
        {
            return node.WithSeparator(newSeparator);
        }

        var oldRight = node.Right;
        var newRight = ReplaceFirstToken(oldRight, newToken);
        if (newRight != oldRight)
        {
            return node.WithRight(newRight);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenInitializer? ReplaceFirstToken(GreenInitializer? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldEqualSign = node.EqualSign;
        var newEqualSign = ReplaceFirstToken(oldEqualSign, newToken);
        if (newEqualSign != oldEqualSign)
        {
            return node.WithEqualSign(newEqualSign);
        }

        var oldValue = node.Value;
        var newValue = ReplaceFirstToken(oldValue, newToken);
        if (newValue != oldValue)
        {
            return node.WithValue(newValue);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenTypeSpecifier? ReplaceFirstToken(
        GreenTypeSpecifier? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldColon = node.Colon;
        var newColon = ReplaceFirstToken(oldColon, newToken);
        if (newColon != oldColon)
        {
            return node.WithColon(newColon);
        }

        var oldType = node.Type;
        var newType = ReplaceFirstToken(oldType, newToken);
        if (newType != oldType)
        {
            return node.WithType(newType);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenNamedParameter? ReplaceFirstToken(
        GreenNamedParameter? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldName = node.Name;
        var newName = ReplaceFirstToken(oldName, newToken);
        if (newName != oldName)
        {
            return node.WithName(newName);
        }

        var oldColon = node.Colon;
        var newColon = ReplaceFirstToken(oldColon, newToken);
        if (newColon != oldColon)
        {
            return node.WithColon(newColon);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenArgumentList? ReplaceFirstToken(
        GreenArgumentList? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldOpenParen = node.OpenParen;
        var newOpenParen = ReplaceFirstToken(oldOpenParen, newToken);
        if (newOpenParen != oldOpenParen)
        {
            return node.WithOpenParen(newOpenParen);
        }

        var oldArguments = node.Arguments;
        var newArguments = ReplaceFirstToken(oldArguments, newToken);
        if (newArguments != oldArguments)
        {
            return node.WithArguments(newArguments);
        }

        var oldCloseParen = node.CloseParen;
        var newCloseParen = ReplaceFirstToken(oldCloseParen, newToken);
        if (newCloseParen != oldCloseParen)
        {
            return node.WithCloseParen(newCloseParen);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenArgument? ReplaceFirstToken(GreenArgument? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldName = node.Name;
        var newName = ReplaceFirstToken(oldName, newToken);
        if (newName != oldName)
        {
            return node.WithName(newName);
        }

        var oldValue = node.Value;
        var newValue = ReplaceFirstToken(oldValue, newToken);
        if (newValue != oldValue)
        {
            return node.WithValue(newValue);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenParameterList? ReplaceFirstToken(
        GreenParameterList? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldOpenParen = node.OpenParen;
        var newOpenParen = ReplaceFirstToken(oldOpenParen, newToken);
        if (newOpenParen != oldOpenParen)
        {
            return node.WithOpenParen(newOpenParen);
        }

        var oldParameters = node.Parameters;
        var newParameters = ReplaceFirstToken(oldParameters, newToken);
        if (newParameters != oldParameters)
        {
            return node.WithParameters(newParameters);
        }

        var oldCloseParen = node.CloseParen;
        var newCloseParen = ReplaceFirstToken(oldCloseParen, newToken);
        if (newCloseParen != oldCloseParen)
        {
            return node.WithCloseParen(newCloseParen);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenParameter? ReplaceFirstToken(GreenParameter? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldMutableKeyword = node.MutableKeyword;
        var newMutableKeyword = ReplaceFirstToken(oldMutableKeyword, newToken);
        if (newMutableKeyword != oldMutableKeyword)
        {
            return node.WithMutableKeyword(newMutableKeyword);
        }

        var oldName = node.Name;
        var newName = ReplaceFirstToken(oldName, newToken);
        if (newName != oldName)
        {
            return node.WithName(newName);
        }

        var oldTypeSpecifier = node.TypeSpecifier;
        var newTypeSpecifier = ReplaceFirstToken(oldTypeSpecifier, newToken);
        if (newTypeSpecifier != oldTypeSpecifier)
        {
            return node.WithTypeSpecifier(newTypeSpecifier);
        }

        var oldDefaultValue = node.DefaultValue;
        var newDefaultValue = ReplaceFirstToken(oldDefaultValue, newToken);
        if (newDefaultValue != oldDefaultValue)
        {
            return node.WithDefaultValue(newDefaultValue);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenExpressionBody? ReplaceFirstToken(
        GreenExpressionBody? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldArrow = node.Arrow;
        var newArrow = ReplaceFirstToken(oldArrow, newToken);
        if (newArrow != oldArrow)
        {
            return node.WithArrow(newArrow);
        }

        var oldExpression = node.Expression;
        var newExpression = ReplaceFirstToken(oldExpression, newToken);
        if (newExpression != oldExpression)
        {
            return node.WithExpression(newExpression);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenElseClause? ReplaceFirstToken(GreenElseClause? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldElseKeyword = node.ElseKeyword;
        var newElseKeyword = ReplaceFirstToken(oldElseKeyword, newToken);
        if (newElseKeyword != oldElseKeyword)
        {
            return node.WithElseKeyword(newElseKeyword);
        }

        var oldStatement = node.Statement;
        var newStatement = ReplaceFirstToken(oldStatement, newToken);
        if (newStatement != oldStatement)
        {
            return node.WithStatement(newStatement);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenLiteralExpression? ReplaceFirstToken(
        GreenLiteralExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldValue = node.Value;
        var newValue = ReplaceFirstToken(oldValue, newToken);
        if (newValue != oldValue)
        {
            return node.WithValue(newValue);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenIdentifierExpression? ReplaceFirstToken(
        GreenIdentifierExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldValue = node.Value;
        var newValue = ReplaceFirstToken(oldValue, newToken);
        if (newValue != oldValue)
        {
            return node.WithValue(newValue);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenParenthesizedExpression? ReplaceFirstToken(
        GreenParenthesizedExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldOpen = node.Open;
        var newOpen = ReplaceFirstToken(oldOpen, newToken);
        if (newOpen != oldOpen)
        {
            return node.WithOpen(newOpen);
        }

        var oldExpression = node.Expression;
        var newExpression = ReplaceFirstToken(oldExpression, newToken);
        if (newExpression != oldExpression)
        {
            return node.WithExpression(newExpression);
        }

        var oldClose = node.Close;
        var newClose = ReplaceFirstToken(oldClose, newToken);
        if (newClose != oldClose)
        {
            return node.WithClose(newClose);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenBinaryExpression? ReplaceFirstToken(
        GreenBinaryExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldLeft = node.Left;
        var newLeft = ReplaceFirstToken(oldLeft, newToken);
        if (newLeft != oldLeft)
        {
            return node.WithLeft(newLeft);
        }

        var oldOp = node.Op;
        var newOp = ReplaceFirstToken(oldOp, newToken);
        if (newOp != oldOp)
        {
            return node.WithOp(newOp);
        }

        var oldRight = node.Right;
        var newRight = ReplaceFirstToken(oldRight, newToken);
        if (newRight != oldRight)
        {
            return node.WithRight(newRight);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenAssignmentExpression? ReplaceFirstToken(
        GreenAssignmentExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldLeft = node.Left;
        var newLeft = ReplaceFirstToken(oldLeft, newToken);
        if (newLeft != oldLeft)
        {
            return node.WithLeft(newLeft);
        }

        var oldOp = node.Op;
        var newOp = ReplaceFirstToken(oldOp, newToken);
        if (newOp != oldOp)
        {
            return node.WithOp(newOp);
        }

        var oldRight = node.Right;
        var newRight = ReplaceFirstToken(oldRight, newToken);
        if (newRight != oldRight)
        {
            return node.WithRight(newRight);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenPrefixExpression? ReplaceFirstToken(
        GreenPrefixExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldOp = node.Op;
        var newOp = ReplaceFirstToken(oldOp, newToken);
        if (newOp != oldOp)
        {
            return node.WithOp(newOp);
        }

        var oldOperand = node.Operand;
        var newOperand = ReplaceFirstToken(oldOperand, newToken);
        if (newOperand != oldOperand)
        {
            return node.WithOperand(newOperand);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenPostfixExpression? ReplaceFirstToken(
        GreenPostfixExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldOperand = node.Operand;
        var newOperand = ReplaceFirstToken(oldOperand, newToken);
        if (newOperand != oldOperand)
        {
            return node.WithOperand(newOperand);
        }

        var oldOp = node.Op;
        var newOp = ReplaceFirstToken(oldOp, newToken);
        if (newOp != oldOp)
        {
            return node.WithOp(newOp);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenTernaryExpression? ReplaceFirstToken(
        GreenTernaryExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldCondition = node.Condition;
        var newCondition = ReplaceFirstToken(oldCondition, newToken);
        if (newCondition != oldCondition)
        {
            return node.WithCondition(newCondition);
        }

        var oldQuestionMark = node.QuestionMark;
        var newQuestionMark = ReplaceFirstToken(oldQuestionMark, newToken);
        if (newQuestionMark != oldQuestionMark)
        {
            return node.WithQuestionMark(newQuestionMark);
        }

        var oldWhenTrue = node.WhenTrue;
        var newWhenTrue = ReplaceFirstToken(oldWhenTrue, newToken);
        if (newWhenTrue != oldWhenTrue)
        {
            return node.WithWhenTrue(newWhenTrue);
        }

        var oldColon = node.Colon;
        var newColon = ReplaceFirstToken(oldColon, newToken);
        if (newColon != oldColon)
        {
            return node.WithColon(newColon);
        }

        var oldWhenFalse = node.WhenFalse;
        var newWhenFalse = ReplaceFirstToken(oldWhenFalse, newToken);
        if (newWhenFalse != oldWhenFalse)
        {
            return node.WithWhenFalse(newWhenFalse);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenInvocationExpression? ReplaceFirstToken(
        GreenInvocationExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldCallee = node.Callee;
        var newCallee = ReplaceFirstToken(oldCallee, newToken);
        if (newCallee != oldCallee)
        {
            return node.WithCallee(newCallee);
        }

        var oldArguments = node.Arguments;
        var newArguments = ReplaceFirstToken(oldArguments, newToken);
        if (newArguments != oldArguments)
        {
            return node.WithArguments(newArguments);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenCastExpression? ReplaceFirstToken(
        GreenCastExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldOperand = node.Operand;
        var newOperand = ReplaceFirstToken(oldOperand, newToken);
        if (newOperand != oldOperand)
        {
            return node.WithOperand(newOperand);
        }

        var oldAsKeyword = node.AsKeyword;
        var newAsKeyword = ReplaceFirstToken(oldAsKeyword, newToken);
        if (newAsKeyword != oldAsKeyword)
        {
            return node.WithAsKeyword(newAsKeyword);
        }

        var oldType = node.Type;
        var newType = ReplaceFirstToken(oldType, newToken);
        if (newType != oldType)
        {
            return node.WithType(newType);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenEmptyStatement? ReplaceFirstToken(
        GreenEmptyStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceFirstToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenVariableDeclarationStatement? ReplaceFirstToken(
        GreenVariableDeclarationStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldDeclaration = node.Declaration;
        var newDeclaration = ReplaceFirstToken(oldDeclaration, newToken);
        if (newDeclaration != oldDeclaration)
        {
            return node.WithDeclaration(newDeclaration);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenBlock? ReplaceFirstToken(GreenBlock? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldOpenBrace = node.OpenBrace;
        var newOpenBrace = ReplaceFirstToken(oldOpenBrace, newToken);
        if (newOpenBrace != oldOpenBrace)
        {
            return node.WithOpenBrace(newOpenBrace);
        }

        var oldStatements = node.Statements;
        var newStatements = ReplaceFirstToken(oldStatements, newToken);
        if (newStatements != oldStatements)
        {
            return node.WithStatements(newStatements);
        }

        var oldCloseBrace = node.CloseBrace;
        var newCloseBrace = ReplaceFirstToken(oldCloseBrace, newToken);
        if (newCloseBrace != oldCloseBrace)
        {
            return node.WithCloseBrace(newCloseBrace);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenReturnStatement? ReplaceFirstToken(
        GreenReturnStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldReturnKeyword = node.ReturnKeyword;
        var newReturnKeyword = ReplaceFirstToken(oldReturnKeyword, newToken);
        if (newReturnKeyword != oldReturnKeyword)
        {
            return node.WithReturnKeyword(newReturnKeyword);
        }

        var oldExpression = node.Expression;
        var newExpression = ReplaceFirstToken(oldExpression, newToken);
        if (newExpression != oldExpression)
        {
            return node.WithExpression(newExpression);
        }

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceFirstToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenExpressionStatement? ReplaceFirstToken(
        GreenExpressionStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldExpression = node.Expression;
        var newExpression = ReplaceFirstToken(oldExpression, newToken);
        if (newExpression != oldExpression)
        {
            return node.WithExpression(newExpression);
        }

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceFirstToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenIfStatement? ReplaceFirstToken(GreenIfStatement? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldIfKeyword = node.IfKeyword;
        var newIfKeyword = ReplaceFirstToken(oldIfKeyword, newToken);
        if (newIfKeyword != oldIfKeyword)
        {
            return node.WithIfKeyword(newIfKeyword);
        }

        var oldOpenParen = node.OpenParen;
        var newOpenParen = ReplaceFirstToken(oldOpenParen, newToken);
        if (newOpenParen != oldOpenParen)
        {
            return node.WithOpenParen(newOpenParen);
        }

        var oldCondition = node.Condition;
        var newCondition = ReplaceFirstToken(oldCondition, newToken);
        if (newCondition != oldCondition)
        {
            return node.WithCondition(newCondition);
        }

        var oldCloseParen = node.CloseParen;
        var newCloseParen = ReplaceFirstToken(oldCloseParen, newToken);
        if (newCloseParen != oldCloseParen)
        {
            return node.WithCloseParen(newCloseParen);
        }

        var oldBlock = node.Block;
        var newBlock = ReplaceFirstToken(oldBlock, newToken);
        if (newBlock != oldBlock)
        {
            return node.WithBlock(newBlock);
        }

        var oldElseClause = node.ElseClause;
        var newElseClause = ReplaceFirstToken(oldElseClause, newToken);
        if (newElseClause != oldElseClause)
        {
            return node.WithElseClause(newElseClause);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenWhileStatement? ReplaceFirstToken(
        GreenWhileStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldWhileKeyword = node.WhileKeyword;
        var newWhileKeyword = ReplaceFirstToken(oldWhileKeyword, newToken);
        if (newWhileKeyword != oldWhileKeyword)
        {
            return node.WithWhileKeyword(newWhileKeyword);
        }

        var oldOpenParen = node.OpenParen;
        var newOpenParen = ReplaceFirstToken(oldOpenParen, newToken);
        if (newOpenParen != oldOpenParen)
        {
            return node.WithOpenParen(newOpenParen);
        }

        var oldCondition = node.Condition;
        var newCondition = ReplaceFirstToken(oldCondition, newToken);
        if (newCondition != oldCondition)
        {
            return node.WithCondition(newCondition);
        }

        var oldCloseParen = node.CloseParen;
        var newCloseParen = ReplaceFirstToken(oldCloseParen, newToken);
        if (newCloseParen != oldCloseParen)
        {
            return node.WithCloseParen(newCloseParen);
        }

        var oldBlock = node.Block;
        var newBlock = ReplaceFirstToken(oldBlock, newToken);
        if (newBlock != oldBlock)
        {
            return node.WithBlock(newBlock);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenLoopStatement? ReplaceFirstToken(
        GreenLoopStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldLoopKeyword = node.LoopKeyword;
        var newLoopKeyword = ReplaceFirstToken(oldLoopKeyword, newToken);
        if (newLoopKeyword != oldLoopKeyword)
        {
            return node.WithLoopKeyword(newLoopKeyword);
        }

        var oldBlock = node.Block;
        var newBlock = ReplaceFirstToken(oldBlock, newToken);
        if (newBlock != oldBlock)
        {
            return node.WithBlock(newBlock);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenForStatement? ReplaceFirstToken(
        GreenForStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldForKeyword = node.ForKeyword;
        var newForKeyword = ReplaceFirstToken(oldForKeyword, newToken);
        if (newForKeyword != oldForKeyword)
        {
            return node.WithForKeyword(newForKeyword);
        }

        var oldOpenParen = node.OpenParen;
        var newOpenParen = ReplaceFirstToken(oldOpenParen, newToken);
        if (newOpenParen != oldOpenParen)
        {
            return node.WithOpenParen(newOpenParen);
        }

        var oldDeclaration = node.Declaration;
        var newDeclaration = ReplaceFirstToken(oldDeclaration, newToken);
        if (newDeclaration != oldDeclaration)
        {
            return node.WithDeclaration(newDeclaration);
        }

        var oldInitializers = node.Initializers;
        var newInitializers = ReplaceFirstToken(oldInitializers, newToken);
        if (newInitializers != oldInitializers)
        {
            return node.WithInitializers(newInitializers);
        }

        var oldFirstSemicolon = node.FirstSemicolon;
        var newFirstSemicolon = ReplaceFirstToken(oldFirstSemicolon, newToken);
        if (newFirstSemicolon != oldFirstSemicolon)
        {
            return node.WithFirstSemicolon(newFirstSemicolon);
        }

        var oldCondition = node.Condition;
        var newCondition = ReplaceFirstToken(oldCondition, newToken);
        if (newCondition != oldCondition)
        {
            return node.WithCondition(newCondition);
        }

        var oldSecondSemicolon = node.SecondSemicolon;
        var newSecondSemicolon = ReplaceFirstToken(oldSecondSemicolon, newToken);
        if (newSecondSemicolon != oldSecondSemicolon)
        {
            return node.WithSecondSemicolon(newSecondSemicolon);
        }

        var oldIncrementors = node.Incrementors;
        var newIncrementors = ReplaceFirstToken(oldIncrementors, newToken);
        if (newIncrementors != oldIncrementors)
        {
            return node.WithIncrementors(newIncrementors);
        }

        var oldCloseParen = node.CloseParen;
        var newCloseParen = ReplaceFirstToken(oldCloseParen, newToken);
        if (newCloseParen != oldCloseParen)
        {
            return node.WithCloseParen(newCloseParen);
        }

        var oldBlock = node.Block;
        var newBlock = ReplaceFirstToken(oldBlock, newToken);
        if (newBlock != oldBlock)
        {
            return node.WithBlock(newBlock);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenBreakStatement? ReplaceFirstToken(
        GreenBreakStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldKeyword = node.Keyword;
        var newKeyword = ReplaceFirstToken(oldKeyword, newToken);
        if (newKeyword != oldKeyword)
        {
            return node.WithKeyword(newKeyword);
        }

        var oldLabel = node.Label;
        var newLabel = ReplaceFirstToken(oldLabel, newToken);
        if (newLabel != oldLabel)
        {
            return node.WithLabel(newLabel);
        }

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceFirstToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenContinueStatement? ReplaceFirstToken(
        GreenContinueStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldKeyword = node.Keyword;
        var newKeyword = ReplaceFirstToken(oldKeyword, newToken);
        if (newKeyword != oldKeyword)
        {
            return node.WithKeyword(newKeyword);
        }

        var oldLabel = node.Label;
        var newLabel = ReplaceFirstToken(oldLabel, newToken);
        if (newLabel != oldLabel)
        {
            return node.WithLabel(newLabel);
        }

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceFirstToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenLabeledStatement? ReplaceFirstToken(
        GreenLabeledStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldIdentifier = node.Identifier;
        var newIdentifier = ReplaceFirstToken(oldIdentifier, newToken);
        if (newIdentifier != oldIdentifier)
        {
            return node.WithIdentifier(newIdentifier);
        }

        var oldColon = node.Colon;
        var newColon = ReplaceFirstToken(oldColon, newToken);
        if (newColon != oldColon)
        {
            return node.WithColon(newColon);
        }

        var oldStatement = node.Statement;
        var newStatement = ReplaceFirstToken(oldStatement, newToken);
        if (newStatement != oldStatement)
        {
            return node.WithStatement(newStatement);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenIncompleteDeclaration? ReplaceFirstToken(
        GreenIncompleteDeclaration? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldModifiers = node.Modifiers;
        var newModifiers = ReplaceFirstToken(oldModifiers, newToken);
        if (newModifiers != oldModifiers)
        {
            return node.WithModifiers(newModifiers);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenBlockNamespaceDeclaration? ReplaceFirstToken(
        GreenBlockNamespaceDeclaration? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldModifiers = node.Modifiers;
        var newModifiers = ReplaceFirstToken(oldModifiers, newToken);
        if (newModifiers != oldModifiers)
        {
            return node.WithModifiers(newModifiers);
        }

        var oldNamespaceToken = node.NamespaceToken;
        var newNamespaceToken = ReplaceFirstToken(oldNamespaceToken, newToken);
        if (newNamespaceToken != oldNamespaceToken)
        {
            return node.WithNamespaceToken(newNamespaceToken);
        }

        var oldName = node.Name;
        var newName = ReplaceFirstToken(oldName, newToken);
        if (newName != oldName)
        {
            return node.WithName(newName);
        }

        var oldOpenBrace = node.OpenBrace;
        var newOpenBrace = ReplaceFirstToken(oldOpenBrace, newToken);
        if (newOpenBrace != oldOpenBrace)
        {
            return node.WithOpenBrace(newOpenBrace);
        }

        var oldUsings = node.Usings;
        var newUsings = ReplaceFirstToken(oldUsings, newToken);
        if (newUsings != oldUsings)
        {
            return node.WithUsings(newUsings);
        }

        var oldMembers = node.Members;
        var newMembers = ReplaceFirstToken(oldMembers, newToken);
        if (newMembers != oldMembers)
        {
            return node.WithMembers(newMembers);
        }

        var oldCloseBrace = node.CloseBrace;
        var newCloseBrace = ReplaceFirstToken(oldCloseBrace, newToken);
        if (newCloseBrace != oldCloseBrace)
        {
            return node.WithCloseBrace(newCloseBrace);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenFileScopedNamespaceDeclaration? ReplaceFirstToken(
        GreenFileScopedNamespaceDeclaration? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldModifiers = node.Modifiers;
        var newModifiers = ReplaceFirstToken(oldModifiers, newToken);
        if (newModifiers != oldModifiers)
        {
            return node.WithModifiers(newModifiers);
        }

        var oldNamespaceToken = node.NamespaceToken;
        var newNamespaceToken = ReplaceFirstToken(oldNamespaceToken, newToken);
        if (newNamespaceToken != oldNamespaceToken)
        {
            return node.WithNamespaceToken(newNamespaceToken);
        }

        var oldName = node.Name;
        var newName = ReplaceFirstToken(oldName, newToken);
        if (newName != oldName)
        {
            return node.WithName(newName);
        }

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceFirstToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        var oldUsings = node.Usings;
        var newUsings = ReplaceFirstToken(oldUsings, newToken);
        if (newUsings != oldUsings)
        {
            return node.WithUsings(newUsings);
        }

        var oldMembers = node.Members;
        var newMembers = ReplaceFirstToken(oldMembers, newToken);
        if (newMembers != oldMembers)
        {
            return node.WithMembers(newMembers);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenVariableDeclaration? ReplaceFirstToken(
        GreenVariableDeclaration? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldModifiers = node.Modifiers;
        var newModifiers = ReplaceFirstToken(oldModifiers, newToken);
        if (newModifiers != oldModifiers)
        {
            return node.WithModifiers(newModifiers);
        }

        var oldVarKeyword = node.VarKeyword;
        var newVarKeyword = ReplaceFirstToken(oldVarKeyword, newToken);
        if (newVarKeyword != oldVarKeyword)
        {
            return node.WithVarKeyword(newVarKeyword);
        }

        var oldIdentifier = node.Identifier;
        var newIdentifier = ReplaceFirstToken(oldIdentifier, newToken);
        if (newIdentifier != oldIdentifier)
        {
            return node.WithIdentifier(newIdentifier);
        }

        var oldType = node.Type;
        var newType = ReplaceFirstToken(oldType, newToken);
        if (newType != oldType)
        {
            return node.WithType(newType);
        }

        var oldInitializer = node.Initializer;
        var newInitializer = ReplaceFirstToken(oldInitializer, newToken);
        if (newInitializer != oldInitializer)
        {
            return node.WithInitializer(newInitializer);
        }

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceFirstToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenFunctionDeclaration? ReplaceFirstToken(
        GreenFunctionDeclaration? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldModifiers = node.Modifiers;
        var newModifiers = ReplaceFirstToken(oldModifiers, newToken);
        if (newModifiers != oldModifiers)
        {
            return node.WithModifiers(newModifiers);
        }

        var oldFuncKeyword = node.FuncKeyword;
        var newFuncKeyword = ReplaceFirstToken(oldFuncKeyword, newToken);
        if (newFuncKeyword != oldFuncKeyword)
        {
            return node.WithFuncKeyword(newFuncKeyword);
        }

        var oldIdentifier = node.Identifier;
        var newIdentifier = ReplaceFirstToken(oldIdentifier, newToken);
        if (newIdentifier != oldIdentifier)
        {
            return node.WithIdentifier(newIdentifier);
        }

        var oldParameters = node.Parameters;
        var newParameters = ReplaceFirstToken(oldParameters, newToken);
        if (newParameters != oldParameters)
        {
            return node.WithParameters(newParameters);
        }

        var oldReturnType = node.ReturnType;
        var newReturnType = ReplaceFirstToken(oldReturnType, newToken);
        if (newReturnType != oldReturnType)
        {
            return node.WithReturnType(newReturnType);
        }

        var oldBody = node.Body;
        var newBody = ReplaceFirstToken(oldBody, newToken);
        if (newBody != oldBody)
        {
            return node.WithBody(newBody);
        }

        var oldExpressionBody = node.ExpressionBody;
        var newExpressionBody = ReplaceFirstToken(oldExpressionBody, newToken);
        if (newExpressionBody != oldExpressionBody)
        {
            return node.WithExpressionBody(newExpressionBody);
        }

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceFirstToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenUsingDirective? ReplaceFirstToken(
        GreenUsingDirective? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldUsingKeyword = node.UsingKeyword;
        var newUsingKeyword = ReplaceFirstToken(oldUsingKeyword, newToken);
        if (newUsingKeyword != oldUsingKeyword)
        {
            return node.WithUsingKeyword(newUsingKeyword);
        }

        var oldName = node.Name;
        var newName = ReplaceFirstToken(oldName, newToken);
        if (newName != oldName)
        {
            return node.WithName(newName);
        }

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceFirstToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenNode? ReplaceLastToken(GreenNode? node, GreenToken newToken)
    {
        return node switch
        {
            null => null,
            GreenListNode list => ReplaceLastToken(list, newToken),
            GreenToken token => ReplaceLastToken(token, newToken),
            GreenTrivia trivia => ReplaceLastToken(trivia, newToken),
            GreenSkippedTokensTrivia skippedTokensTrivia => ReplaceLastToken(
                skippedTokensTrivia,
                newToken
            ),
            GreenCompilationUnit compilationUnit => ReplaceLastToken(compilationUnit, newToken),
            GreenPredefinedType predefinedType => ReplaceLastToken(predefinedType, newToken),
            GreenNamedType namedType => ReplaceLastToken(namedType, newToken),
            GreenSimpleName simpleName => ReplaceLastToken(simpleName, newToken),
            GreenQualifiedName qualifiedName => ReplaceLastToken(qualifiedName, newToken),
            GreenInitializer initializer => ReplaceLastToken(initializer, newToken),
            GreenTypeSpecifier typeSpecifier => ReplaceLastToken(typeSpecifier, newToken),
            GreenNamedParameter namedParameter => ReplaceLastToken(namedParameter, newToken),
            GreenArgumentList argumentList => ReplaceLastToken(argumentList, newToken),
            GreenArgument argument => ReplaceLastToken(argument, newToken),
            GreenParameterList parameterList => ReplaceLastToken(parameterList, newToken),
            GreenParameter parameter => ReplaceLastToken(parameter, newToken),
            GreenExpressionBody expressionBody => ReplaceLastToken(expressionBody, newToken),
            GreenElseClause elseClause => ReplaceLastToken(elseClause, newToken),
            GreenLiteralExpression literalExpression => ReplaceLastToken(
                literalExpression,
                newToken
            ),
            GreenIdentifierExpression identifierExpression => ReplaceLastToken(
                identifierExpression,
                newToken
            ),
            GreenParenthesizedExpression parenthesizedExpression => ReplaceLastToken(
                parenthesizedExpression,
                newToken
            ),
            GreenBinaryExpression binaryExpression => ReplaceLastToken(binaryExpression, newToken),
            GreenAssignmentExpression assignmentExpression => ReplaceLastToken(
                assignmentExpression,
                newToken
            ),
            GreenPrefixExpression prefixExpression => ReplaceLastToken(prefixExpression, newToken),
            GreenPostfixExpression postfixExpression => ReplaceLastToken(
                postfixExpression,
                newToken
            ),
            GreenTernaryExpression ternaryExpression => ReplaceLastToken(
                ternaryExpression,
                newToken
            ),
            GreenInvocationExpression invocationExpression => ReplaceLastToken(
                invocationExpression,
                newToken
            ),
            GreenCastExpression castExpression => ReplaceLastToken(castExpression, newToken),
            GreenEmptyStatement emptyStatement => ReplaceLastToken(emptyStatement, newToken),
            GreenVariableDeclarationStatement variableDeclarationStatement => ReplaceLastToken(
                variableDeclarationStatement,
                newToken
            ),
            GreenBlock block => ReplaceLastToken(block, newToken),
            GreenReturnStatement returnStatement => ReplaceLastToken(returnStatement, newToken),
            GreenExpressionStatement expressionStatement => ReplaceLastToken(
                expressionStatement,
                newToken
            ),
            GreenIfStatement ifStatement => ReplaceLastToken(ifStatement, newToken),
            GreenWhileStatement whileStatement => ReplaceLastToken(whileStatement, newToken),
            GreenLoopStatement loopStatement => ReplaceLastToken(loopStatement, newToken),
            GreenForStatement forStatement => ReplaceLastToken(forStatement, newToken),
            GreenBreakStatement breakStatement => ReplaceLastToken(breakStatement, newToken),
            GreenContinueStatement continueStatement => ReplaceLastToken(
                continueStatement,
                newToken
            ),
            GreenLabeledStatement labeledStatement => ReplaceLastToken(labeledStatement, newToken),
            GreenIncompleteDeclaration incompleteDeclaration => ReplaceLastToken(
                incompleteDeclaration,
                newToken
            ),
            GreenBlockNamespaceDeclaration blockNamespaceDeclaration => ReplaceLastToken(
                blockNamespaceDeclaration,
                newToken
            ),
            GreenFileScopedNamespaceDeclaration fileScopedNamespaceDeclaration => ReplaceLastToken(
                fileScopedNamespaceDeclaration,
                newToken
            ),
            GreenVariableDeclaration variableDeclaration => ReplaceLastToken(
                variableDeclaration,
                newToken
            ),
            GreenFunctionDeclaration functionDeclaration => ReplaceLastToken(
                functionDeclaration,
                newToken
            ),
            GreenUsingDirective usingDirective => ReplaceLastToken(usingDirective, newToken),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenStructuredTrivia? ReplaceLastToken(
        GreenStructuredTrivia? node,
        GreenToken newToken
    )
    {
        return node switch
        {
            null => null,
            GreenSkippedTokensTrivia skippedTokensTrivia => ReplaceLastToken(
                skippedTokensTrivia,
                newToken
            ),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenType? ReplaceLastToken(GreenType? node, GreenToken newToken)
    {
        return node switch
        {
            null => null,
            GreenPredefinedType predefinedType => ReplaceLastToken(predefinedType, newToken),
            GreenNamedType namedType => ReplaceLastToken(namedType, newToken),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenName? ReplaceLastToken(GreenName? node, GreenToken newToken)
    {
        return node switch
        {
            null => null,
            GreenSimpleName simpleName => ReplaceLastToken(simpleName, newToken),
            GreenQualifiedName qualifiedName => ReplaceLastToken(qualifiedName, newToken),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenExpression? ReplaceLastToken(GreenExpression? node, GreenToken newToken)
    {
        return node switch
        {
            null => null,
            GreenLiteralExpression literalExpression => ReplaceLastToken(
                literalExpression,
                newToken
            ),
            GreenIdentifierExpression identifierExpression => ReplaceLastToken(
                identifierExpression,
                newToken
            ),
            GreenParenthesizedExpression parenthesizedExpression => ReplaceLastToken(
                parenthesizedExpression,
                newToken
            ),
            GreenBinaryExpression binaryExpression => ReplaceLastToken(binaryExpression, newToken),
            GreenAssignmentExpression assignmentExpression => ReplaceLastToken(
                assignmentExpression,
                newToken
            ),
            GreenPrefixExpression prefixExpression => ReplaceLastToken(prefixExpression, newToken),
            GreenPostfixExpression postfixExpression => ReplaceLastToken(
                postfixExpression,
                newToken
            ),
            GreenTernaryExpression ternaryExpression => ReplaceLastToken(
                ternaryExpression,
                newToken
            ),
            GreenInvocationExpression invocationExpression => ReplaceLastToken(
                invocationExpression,
                newToken
            ),
            GreenCastExpression castExpression => ReplaceLastToken(castExpression, newToken),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenStatement? ReplaceLastToken(GreenStatement? node, GreenToken newToken)
    {
        return node switch
        {
            null => null,
            GreenEmptyStatement emptyStatement => ReplaceLastToken(emptyStatement, newToken),
            GreenVariableDeclarationStatement variableDeclarationStatement => ReplaceLastToken(
                variableDeclarationStatement,
                newToken
            ),
            GreenBlock block => ReplaceLastToken(block, newToken),
            GreenReturnStatement returnStatement => ReplaceLastToken(returnStatement, newToken),
            GreenExpressionStatement expressionStatement => ReplaceLastToken(
                expressionStatement,
                newToken
            ),
            GreenIfStatement ifStatement => ReplaceLastToken(ifStatement, newToken),
            GreenWhileStatement whileStatement => ReplaceLastToken(whileStatement, newToken),
            GreenLoopStatement loopStatement => ReplaceLastToken(loopStatement, newToken),
            GreenForStatement forStatement => ReplaceLastToken(forStatement, newToken),
            GreenBreakStatement breakStatement => ReplaceLastToken(breakStatement, newToken),
            GreenContinueStatement continueStatement => ReplaceLastToken(
                continueStatement,
                newToken
            ),
            GreenLabeledStatement labeledStatement => ReplaceLastToken(labeledStatement, newToken),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenDeclaration? ReplaceLastToken(GreenDeclaration? node, GreenToken newToken)
    {
        return node switch
        {
            null => null,
            GreenIncompleteDeclaration incompleteDeclaration => ReplaceLastToken(
                incompleteDeclaration,
                newToken
            ),
            GreenBlockNamespaceDeclaration blockNamespaceDeclaration => ReplaceLastToken(
                blockNamespaceDeclaration,
                newToken
            ),
            GreenFileScopedNamespaceDeclaration fileScopedNamespaceDeclaration => ReplaceLastToken(
                fileScopedNamespaceDeclaration,
                newToken
            ),
            GreenVariableDeclaration variableDeclaration => ReplaceLastToken(
                variableDeclaration,
                newToken
            ),
            GreenFunctionDeclaration functionDeclaration => ReplaceLastToken(
                functionDeclaration,
                newToken
            ),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenNamespaceDeclaration? ReplaceLastToken(
        GreenNamespaceDeclaration? node,
        GreenToken newToken
    )
    {
        return node switch
        {
            null => null,
            GreenBlockNamespaceDeclaration blockNamespaceDeclaration => ReplaceLastToken(
                blockNamespaceDeclaration,
                newToken
            ),
            GreenFileScopedNamespaceDeclaration fileScopedNamespaceDeclaration => ReplaceLastToken(
                fileScopedNamespaceDeclaration,
                newToken
            ),
            _ => throw new InvalidOperationException("Invalid node type passed into visit"),
        };
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static partial GreenToken? ReplaceLastToken(GreenToken? node, GreenToken newToken);

    [return: NotNullIfNotNull(nameof(node))]
    internal static partial GreenTrivia? ReplaceLastToken(GreenTrivia? node, GreenToken newToken);

    [return: NotNullIfNotNull(nameof(node))]
    internal static partial GreenListNode? ReplaceLastToken(
        GreenListNode? node,
        GreenToken newToken
    );

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenSkippedTokensTrivia? ReplaceLastToken(
        GreenSkippedTokensTrivia? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldTokens = node.Tokens;
        var newTokens = ReplaceLastToken(oldTokens, newToken);
        if (newTokens != oldTokens)
        {
            return node.WithTokens(newTokens);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenCompilationUnit? ReplaceLastToken(
        GreenCompilationUnit? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldMembers = node.Members;
        var newMembers = ReplaceLastToken(oldMembers, newToken);
        if (newMembers != oldMembers)
        {
            return node.WithMembers(newMembers);
        }

        var oldUsings = node.Usings;
        var newUsings = ReplaceLastToken(oldUsings, newToken);
        if (newUsings != oldUsings)
        {
            return node.WithUsings(newUsings);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenPredefinedType? ReplaceLastToken(
        GreenPredefinedType? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldKeyword = node.Keyword;
        var newKeyword = ReplaceLastToken(oldKeyword, newToken);
        if (newKeyword != oldKeyword)
        {
            return node.WithKeyword(newKeyword);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenNamedType? ReplaceLastToken(GreenNamedType? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldIdentifier = node.Identifier;
        var newIdentifier = ReplaceLastToken(oldIdentifier, newToken);
        if (newIdentifier != oldIdentifier)
        {
            return node.WithIdentifier(newIdentifier);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenSimpleName? ReplaceLastToken(GreenSimpleName? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldIdentifier = node.Identifier;
        var newIdentifier = ReplaceLastToken(oldIdentifier, newToken);
        if (newIdentifier != oldIdentifier)
        {
            return node.WithIdentifier(newIdentifier);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenQualifiedName? ReplaceLastToken(
        GreenQualifiedName? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldRight = node.Right;
        var newRight = ReplaceLastToken(oldRight, newToken);
        if (newRight != oldRight)
        {
            return node.WithRight(newRight);
        }

        var oldSeparator = node.Separator;
        var newSeparator = ReplaceLastToken(oldSeparator, newToken);
        if (newSeparator != oldSeparator)
        {
            return node.WithSeparator(newSeparator);
        }

        var oldLeft = node.Left;
        var newLeft = ReplaceLastToken(oldLeft, newToken);
        if (newLeft != oldLeft)
        {
            return node.WithLeft(newLeft);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenInitializer? ReplaceLastToken(GreenInitializer? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldValue = node.Value;
        var newValue = ReplaceLastToken(oldValue, newToken);
        if (newValue != oldValue)
        {
            return node.WithValue(newValue);
        }

        var oldEqualSign = node.EqualSign;
        var newEqualSign = ReplaceLastToken(oldEqualSign, newToken);
        if (newEqualSign != oldEqualSign)
        {
            return node.WithEqualSign(newEqualSign);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenTypeSpecifier? ReplaceLastToken(
        GreenTypeSpecifier? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldType = node.Type;
        var newType = ReplaceLastToken(oldType, newToken);
        if (newType != oldType)
        {
            return node.WithType(newType);
        }

        var oldColon = node.Colon;
        var newColon = ReplaceLastToken(oldColon, newToken);
        if (newColon != oldColon)
        {
            return node.WithColon(newColon);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenNamedParameter? ReplaceLastToken(
        GreenNamedParameter? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldColon = node.Colon;
        var newColon = ReplaceLastToken(oldColon, newToken);
        if (newColon != oldColon)
        {
            return node.WithColon(newColon);
        }

        var oldName = node.Name;
        var newName = ReplaceLastToken(oldName, newToken);
        if (newName != oldName)
        {
            return node.WithName(newName);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenArgumentList? ReplaceLastToken(
        GreenArgumentList? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldCloseParen = node.CloseParen;
        var newCloseParen = ReplaceLastToken(oldCloseParen, newToken);
        if (newCloseParen != oldCloseParen)
        {
            return node.WithCloseParen(newCloseParen);
        }

        var oldArguments = node.Arguments;
        var newArguments = ReplaceLastToken(oldArguments, newToken);
        if (newArguments != oldArguments)
        {
            return node.WithArguments(newArguments);
        }

        var oldOpenParen = node.OpenParen;
        var newOpenParen = ReplaceLastToken(oldOpenParen, newToken);
        if (newOpenParen != oldOpenParen)
        {
            return node.WithOpenParen(newOpenParen);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenArgument? ReplaceLastToken(GreenArgument? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldValue = node.Value;
        var newValue = ReplaceLastToken(oldValue, newToken);
        if (newValue != oldValue)
        {
            return node.WithValue(newValue);
        }

        var oldName = node.Name;
        var newName = ReplaceLastToken(oldName, newToken);
        if (newName != oldName)
        {
            return node.WithName(newName);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenParameterList? ReplaceLastToken(
        GreenParameterList? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldCloseParen = node.CloseParen;
        var newCloseParen = ReplaceLastToken(oldCloseParen, newToken);
        if (newCloseParen != oldCloseParen)
        {
            return node.WithCloseParen(newCloseParen);
        }

        var oldParameters = node.Parameters;
        var newParameters = ReplaceLastToken(oldParameters, newToken);
        if (newParameters != oldParameters)
        {
            return node.WithParameters(newParameters);
        }

        var oldOpenParen = node.OpenParen;
        var newOpenParen = ReplaceLastToken(oldOpenParen, newToken);
        if (newOpenParen != oldOpenParen)
        {
            return node.WithOpenParen(newOpenParen);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenParameter? ReplaceLastToken(GreenParameter? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldDefaultValue = node.DefaultValue;
        var newDefaultValue = ReplaceLastToken(oldDefaultValue, newToken);
        if (newDefaultValue != oldDefaultValue)
        {
            return node.WithDefaultValue(newDefaultValue);
        }

        var oldTypeSpecifier = node.TypeSpecifier;
        var newTypeSpecifier = ReplaceLastToken(oldTypeSpecifier, newToken);
        if (newTypeSpecifier != oldTypeSpecifier)
        {
            return node.WithTypeSpecifier(newTypeSpecifier);
        }

        var oldName = node.Name;
        var newName = ReplaceLastToken(oldName, newToken);
        if (newName != oldName)
        {
            return node.WithName(newName);
        }

        var oldMutableKeyword = node.MutableKeyword;
        var newMutableKeyword = ReplaceLastToken(oldMutableKeyword, newToken);
        if (newMutableKeyword != oldMutableKeyword)
        {
            return node.WithMutableKeyword(newMutableKeyword);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenExpressionBody? ReplaceLastToken(
        GreenExpressionBody? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldExpression = node.Expression;
        var newExpression = ReplaceLastToken(oldExpression, newToken);
        if (newExpression != oldExpression)
        {
            return node.WithExpression(newExpression);
        }

        var oldArrow = node.Arrow;
        var newArrow = ReplaceLastToken(oldArrow, newToken);
        if (newArrow != oldArrow)
        {
            return node.WithArrow(newArrow);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenElseClause? ReplaceLastToken(GreenElseClause? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldStatement = node.Statement;
        var newStatement = ReplaceLastToken(oldStatement, newToken);
        if (newStatement != oldStatement)
        {
            return node.WithStatement(newStatement);
        }

        var oldElseKeyword = node.ElseKeyword;
        var newElseKeyword = ReplaceLastToken(oldElseKeyword, newToken);
        if (newElseKeyword != oldElseKeyword)
        {
            return node.WithElseKeyword(newElseKeyword);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenLiteralExpression? ReplaceLastToken(
        GreenLiteralExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldValue = node.Value;
        var newValue = ReplaceLastToken(oldValue, newToken);
        if (newValue != oldValue)
        {
            return node.WithValue(newValue);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenIdentifierExpression? ReplaceLastToken(
        GreenIdentifierExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldValue = node.Value;
        var newValue = ReplaceLastToken(oldValue, newToken);
        if (newValue != oldValue)
        {
            return node.WithValue(newValue);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenParenthesizedExpression? ReplaceLastToken(
        GreenParenthesizedExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldClose = node.Close;
        var newClose = ReplaceLastToken(oldClose, newToken);
        if (newClose != oldClose)
        {
            return node.WithClose(newClose);
        }

        var oldExpression = node.Expression;
        var newExpression = ReplaceLastToken(oldExpression, newToken);
        if (newExpression != oldExpression)
        {
            return node.WithExpression(newExpression);
        }

        var oldOpen = node.Open;
        var newOpen = ReplaceLastToken(oldOpen, newToken);
        if (newOpen != oldOpen)
        {
            return node.WithOpen(newOpen);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenBinaryExpression? ReplaceLastToken(
        GreenBinaryExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldRight = node.Right;
        var newRight = ReplaceLastToken(oldRight, newToken);
        if (newRight != oldRight)
        {
            return node.WithRight(newRight);
        }

        var oldOp = node.Op;
        var newOp = ReplaceLastToken(oldOp, newToken);
        if (newOp != oldOp)
        {
            return node.WithOp(newOp);
        }

        var oldLeft = node.Left;
        var newLeft = ReplaceLastToken(oldLeft, newToken);
        if (newLeft != oldLeft)
        {
            return node.WithLeft(newLeft);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenAssignmentExpression? ReplaceLastToken(
        GreenAssignmentExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldRight = node.Right;
        var newRight = ReplaceLastToken(oldRight, newToken);
        if (newRight != oldRight)
        {
            return node.WithRight(newRight);
        }

        var oldOp = node.Op;
        var newOp = ReplaceLastToken(oldOp, newToken);
        if (newOp != oldOp)
        {
            return node.WithOp(newOp);
        }

        var oldLeft = node.Left;
        var newLeft = ReplaceLastToken(oldLeft, newToken);
        if (newLeft != oldLeft)
        {
            return node.WithLeft(newLeft);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenPrefixExpression? ReplaceLastToken(
        GreenPrefixExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldOperand = node.Operand;
        var newOperand = ReplaceLastToken(oldOperand, newToken);
        if (newOperand != oldOperand)
        {
            return node.WithOperand(newOperand);
        }

        var oldOp = node.Op;
        var newOp = ReplaceLastToken(oldOp, newToken);
        if (newOp != oldOp)
        {
            return node.WithOp(newOp);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenPostfixExpression? ReplaceLastToken(
        GreenPostfixExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldOp = node.Op;
        var newOp = ReplaceLastToken(oldOp, newToken);
        if (newOp != oldOp)
        {
            return node.WithOp(newOp);
        }

        var oldOperand = node.Operand;
        var newOperand = ReplaceLastToken(oldOperand, newToken);
        if (newOperand != oldOperand)
        {
            return node.WithOperand(newOperand);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenTernaryExpression? ReplaceLastToken(
        GreenTernaryExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldWhenFalse = node.WhenFalse;
        var newWhenFalse = ReplaceLastToken(oldWhenFalse, newToken);
        if (newWhenFalse != oldWhenFalse)
        {
            return node.WithWhenFalse(newWhenFalse);
        }

        var oldColon = node.Colon;
        var newColon = ReplaceLastToken(oldColon, newToken);
        if (newColon != oldColon)
        {
            return node.WithColon(newColon);
        }

        var oldWhenTrue = node.WhenTrue;
        var newWhenTrue = ReplaceLastToken(oldWhenTrue, newToken);
        if (newWhenTrue != oldWhenTrue)
        {
            return node.WithWhenTrue(newWhenTrue);
        }

        var oldQuestionMark = node.QuestionMark;
        var newQuestionMark = ReplaceLastToken(oldQuestionMark, newToken);
        if (newQuestionMark != oldQuestionMark)
        {
            return node.WithQuestionMark(newQuestionMark);
        }

        var oldCondition = node.Condition;
        var newCondition = ReplaceLastToken(oldCondition, newToken);
        if (newCondition != oldCondition)
        {
            return node.WithCondition(newCondition);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenInvocationExpression? ReplaceLastToken(
        GreenInvocationExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldArguments = node.Arguments;
        var newArguments = ReplaceLastToken(oldArguments, newToken);
        if (newArguments != oldArguments)
        {
            return node.WithArguments(newArguments);
        }

        var oldCallee = node.Callee;
        var newCallee = ReplaceLastToken(oldCallee, newToken);
        if (newCallee != oldCallee)
        {
            return node.WithCallee(newCallee);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenCastExpression? ReplaceLastToken(
        GreenCastExpression? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldType = node.Type;
        var newType = ReplaceLastToken(oldType, newToken);
        if (newType != oldType)
        {
            return node.WithType(newType);
        }

        var oldAsKeyword = node.AsKeyword;
        var newAsKeyword = ReplaceLastToken(oldAsKeyword, newToken);
        if (newAsKeyword != oldAsKeyword)
        {
            return node.WithAsKeyword(newAsKeyword);
        }

        var oldOperand = node.Operand;
        var newOperand = ReplaceLastToken(oldOperand, newToken);
        if (newOperand != oldOperand)
        {
            return node.WithOperand(newOperand);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenEmptyStatement? ReplaceLastToken(
        GreenEmptyStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceLastToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenVariableDeclarationStatement? ReplaceLastToken(
        GreenVariableDeclarationStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldDeclaration = node.Declaration;
        var newDeclaration = ReplaceLastToken(oldDeclaration, newToken);
        if (newDeclaration != oldDeclaration)
        {
            return node.WithDeclaration(newDeclaration);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenBlock? ReplaceLastToken(GreenBlock? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldCloseBrace = node.CloseBrace;
        var newCloseBrace = ReplaceLastToken(oldCloseBrace, newToken);
        if (newCloseBrace != oldCloseBrace)
        {
            return node.WithCloseBrace(newCloseBrace);
        }

        var oldStatements = node.Statements;
        var newStatements = ReplaceLastToken(oldStatements, newToken);
        if (newStatements != oldStatements)
        {
            return node.WithStatements(newStatements);
        }

        var oldOpenBrace = node.OpenBrace;
        var newOpenBrace = ReplaceLastToken(oldOpenBrace, newToken);
        if (newOpenBrace != oldOpenBrace)
        {
            return node.WithOpenBrace(newOpenBrace);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenReturnStatement? ReplaceLastToken(
        GreenReturnStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceLastToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        var oldExpression = node.Expression;
        var newExpression = ReplaceLastToken(oldExpression, newToken);
        if (newExpression != oldExpression)
        {
            return node.WithExpression(newExpression);
        }

        var oldReturnKeyword = node.ReturnKeyword;
        var newReturnKeyword = ReplaceLastToken(oldReturnKeyword, newToken);
        if (newReturnKeyword != oldReturnKeyword)
        {
            return node.WithReturnKeyword(newReturnKeyword);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenExpressionStatement? ReplaceLastToken(
        GreenExpressionStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceLastToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        var oldExpression = node.Expression;
        var newExpression = ReplaceLastToken(oldExpression, newToken);
        if (newExpression != oldExpression)
        {
            return node.WithExpression(newExpression);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenIfStatement? ReplaceLastToken(GreenIfStatement? node, GreenToken newToken)
    {
        if (node is null)
            return null;

        var oldElseClause = node.ElseClause;
        var newElseClause = ReplaceLastToken(oldElseClause, newToken);
        if (newElseClause != oldElseClause)
        {
            return node.WithElseClause(newElseClause);
        }

        var oldBlock = node.Block;
        var newBlock = ReplaceLastToken(oldBlock, newToken);
        if (newBlock != oldBlock)
        {
            return node.WithBlock(newBlock);
        }

        var oldCloseParen = node.CloseParen;
        var newCloseParen = ReplaceLastToken(oldCloseParen, newToken);
        if (newCloseParen != oldCloseParen)
        {
            return node.WithCloseParen(newCloseParen);
        }

        var oldCondition = node.Condition;
        var newCondition = ReplaceLastToken(oldCondition, newToken);
        if (newCondition != oldCondition)
        {
            return node.WithCondition(newCondition);
        }

        var oldOpenParen = node.OpenParen;
        var newOpenParen = ReplaceLastToken(oldOpenParen, newToken);
        if (newOpenParen != oldOpenParen)
        {
            return node.WithOpenParen(newOpenParen);
        }

        var oldIfKeyword = node.IfKeyword;
        var newIfKeyword = ReplaceLastToken(oldIfKeyword, newToken);
        if (newIfKeyword != oldIfKeyword)
        {
            return node.WithIfKeyword(newIfKeyword);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenWhileStatement? ReplaceLastToken(
        GreenWhileStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldBlock = node.Block;
        var newBlock = ReplaceLastToken(oldBlock, newToken);
        if (newBlock != oldBlock)
        {
            return node.WithBlock(newBlock);
        }

        var oldCloseParen = node.CloseParen;
        var newCloseParen = ReplaceLastToken(oldCloseParen, newToken);
        if (newCloseParen != oldCloseParen)
        {
            return node.WithCloseParen(newCloseParen);
        }

        var oldCondition = node.Condition;
        var newCondition = ReplaceLastToken(oldCondition, newToken);
        if (newCondition != oldCondition)
        {
            return node.WithCondition(newCondition);
        }

        var oldOpenParen = node.OpenParen;
        var newOpenParen = ReplaceLastToken(oldOpenParen, newToken);
        if (newOpenParen != oldOpenParen)
        {
            return node.WithOpenParen(newOpenParen);
        }

        var oldWhileKeyword = node.WhileKeyword;
        var newWhileKeyword = ReplaceLastToken(oldWhileKeyword, newToken);
        if (newWhileKeyword != oldWhileKeyword)
        {
            return node.WithWhileKeyword(newWhileKeyword);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenLoopStatement? ReplaceLastToken(
        GreenLoopStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldBlock = node.Block;
        var newBlock = ReplaceLastToken(oldBlock, newToken);
        if (newBlock != oldBlock)
        {
            return node.WithBlock(newBlock);
        }

        var oldLoopKeyword = node.LoopKeyword;
        var newLoopKeyword = ReplaceLastToken(oldLoopKeyword, newToken);
        if (newLoopKeyword != oldLoopKeyword)
        {
            return node.WithLoopKeyword(newLoopKeyword);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenForStatement? ReplaceLastToken(
        GreenForStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldBlock = node.Block;
        var newBlock = ReplaceLastToken(oldBlock, newToken);
        if (newBlock != oldBlock)
        {
            return node.WithBlock(newBlock);
        }

        var oldCloseParen = node.CloseParen;
        var newCloseParen = ReplaceLastToken(oldCloseParen, newToken);
        if (newCloseParen != oldCloseParen)
        {
            return node.WithCloseParen(newCloseParen);
        }

        var oldIncrementors = node.Incrementors;
        var newIncrementors = ReplaceLastToken(oldIncrementors, newToken);
        if (newIncrementors != oldIncrementors)
        {
            return node.WithIncrementors(newIncrementors);
        }

        var oldSecondSemicolon = node.SecondSemicolon;
        var newSecondSemicolon = ReplaceLastToken(oldSecondSemicolon, newToken);
        if (newSecondSemicolon != oldSecondSemicolon)
        {
            return node.WithSecondSemicolon(newSecondSemicolon);
        }

        var oldCondition = node.Condition;
        var newCondition = ReplaceLastToken(oldCondition, newToken);
        if (newCondition != oldCondition)
        {
            return node.WithCondition(newCondition);
        }

        var oldFirstSemicolon = node.FirstSemicolon;
        var newFirstSemicolon = ReplaceLastToken(oldFirstSemicolon, newToken);
        if (newFirstSemicolon != oldFirstSemicolon)
        {
            return node.WithFirstSemicolon(newFirstSemicolon);
        }

        var oldInitializers = node.Initializers;
        var newInitializers = ReplaceLastToken(oldInitializers, newToken);
        if (newInitializers != oldInitializers)
        {
            return node.WithInitializers(newInitializers);
        }

        var oldDeclaration = node.Declaration;
        var newDeclaration = ReplaceLastToken(oldDeclaration, newToken);
        if (newDeclaration != oldDeclaration)
        {
            return node.WithDeclaration(newDeclaration);
        }

        var oldOpenParen = node.OpenParen;
        var newOpenParen = ReplaceLastToken(oldOpenParen, newToken);
        if (newOpenParen != oldOpenParen)
        {
            return node.WithOpenParen(newOpenParen);
        }

        var oldForKeyword = node.ForKeyword;
        var newForKeyword = ReplaceLastToken(oldForKeyword, newToken);
        if (newForKeyword != oldForKeyword)
        {
            return node.WithForKeyword(newForKeyword);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenBreakStatement? ReplaceLastToken(
        GreenBreakStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceLastToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        var oldLabel = node.Label;
        var newLabel = ReplaceLastToken(oldLabel, newToken);
        if (newLabel != oldLabel)
        {
            return node.WithLabel(newLabel);
        }

        var oldKeyword = node.Keyword;
        var newKeyword = ReplaceLastToken(oldKeyword, newToken);
        if (newKeyword != oldKeyword)
        {
            return node.WithKeyword(newKeyword);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenContinueStatement? ReplaceLastToken(
        GreenContinueStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceLastToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        var oldLabel = node.Label;
        var newLabel = ReplaceLastToken(oldLabel, newToken);
        if (newLabel != oldLabel)
        {
            return node.WithLabel(newLabel);
        }

        var oldKeyword = node.Keyword;
        var newKeyword = ReplaceLastToken(oldKeyword, newToken);
        if (newKeyword != oldKeyword)
        {
            return node.WithKeyword(newKeyword);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenLabeledStatement? ReplaceLastToken(
        GreenLabeledStatement? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldStatement = node.Statement;
        var newStatement = ReplaceLastToken(oldStatement, newToken);
        if (newStatement != oldStatement)
        {
            return node.WithStatement(newStatement);
        }

        var oldColon = node.Colon;
        var newColon = ReplaceLastToken(oldColon, newToken);
        if (newColon != oldColon)
        {
            return node.WithColon(newColon);
        }

        var oldIdentifier = node.Identifier;
        var newIdentifier = ReplaceLastToken(oldIdentifier, newToken);
        if (newIdentifier != oldIdentifier)
        {
            return node.WithIdentifier(newIdentifier);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenIncompleteDeclaration? ReplaceLastToken(
        GreenIncompleteDeclaration? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldModifiers = node.Modifiers;
        var newModifiers = ReplaceLastToken(oldModifiers, newToken);
        if (newModifiers != oldModifiers)
        {
            return node.WithModifiers(newModifiers);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenBlockNamespaceDeclaration? ReplaceLastToken(
        GreenBlockNamespaceDeclaration? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldCloseBrace = node.CloseBrace;
        var newCloseBrace = ReplaceLastToken(oldCloseBrace, newToken);
        if (newCloseBrace != oldCloseBrace)
        {
            return node.WithCloseBrace(newCloseBrace);
        }

        var oldMembers = node.Members;
        var newMembers = ReplaceLastToken(oldMembers, newToken);
        if (newMembers != oldMembers)
        {
            return node.WithMembers(newMembers);
        }

        var oldUsings = node.Usings;
        var newUsings = ReplaceLastToken(oldUsings, newToken);
        if (newUsings != oldUsings)
        {
            return node.WithUsings(newUsings);
        }

        var oldOpenBrace = node.OpenBrace;
        var newOpenBrace = ReplaceLastToken(oldOpenBrace, newToken);
        if (newOpenBrace != oldOpenBrace)
        {
            return node.WithOpenBrace(newOpenBrace);
        }

        var oldName = node.Name;
        var newName = ReplaceLastToken(oldName, newToken);
        if (newName != oldName)
        {
            return node.WithName(newName);
        }

        var oldNamespaceToken = node.NamespaceToken;
        var newNamespaceToken = ReplaceLastToken(oldNamespaceToken, newToken);
        if (newNamespaceToken != oldNamespaceToken)
        {
            return node.WithNamespaceToken(newNamespaceToken);
        }

        var oldModifiers = node.Modifiers;
        var newModifiers = ReplaceLastToken(oldModifiers, newToken);
        if (newModifiers != oldModifiers)
        {
            return node.WithModifiers(newModifiers);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenFileScopedNamespaceDeclaration? ReplaceLastToken(
        GreenFileScopedNamespaceDeclaration? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldMembers = node.Members;
        var newMembers = ReplaceLastToken(oldMembers, newToken);
        if (newMembers != oldMembers)
        {
            return node.WithMembers(newMembers);
        }

        var oldUsings = node.Usings;
        var newUsings = ReplaceLastToken(oldUsings, newToken);
        if (newUsings != oldUsings)
        {
            return node.WithUsings(newUsings);
        }

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceLastToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        var oldName = node.Name;
        var newName = ReplaceLastToken(oldName, newToken);
        if (newName != oldName)
        {
            return node.WithName(newName);
        }

        var oldNamespaceToken = node.NamespaceToken;
        var newNamespaceToken = ReplaceLastToken(oldNamespaceToken, newToken);
        if (newNamespaceToken != oldNamespaceToken)
        {
            return node.WithNamespaceToken(newNamespaceToken);
        }

        var oldModifiers = node.Modifiers;
        var newModifiers = ReplaceLastToken(oldModifiers, newToken);
        if (newModifiers != oldModifiers)
        {
            return node.WithModifiers(newModifiers);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenVariableDeclaration? ReplaceLastToken(
        GreenVariableDeclaration? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceLastToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        var oldInitializer = node.Initializer;
        var newInitializer = ReplaceLastToken(oldInitializer, newToken);
        if (newInitializer != oldInitializer)
        {
            return node.WithInitializer(newInitializer);
        }

        var oldType = node.Type;
        var newType = ReplaceLastToken(oldType, newToken);
        if (newType != oldType)
        {
            return node.WithType(newType);
        }

        var oldIdentifier = node.Identifier;
        var newIdentifier = ReplaceLastToken(oldIdentifier, newToken);
        if (newIdentifier != oldIdentifier)
        {
            return node.WithIdentifier(newIdentifier);
        }

        var oldVarKeyword = node.VarKeyword;
        var newVarKeyword = ReplaceLastToken(oldVarKeyword, newToken);
        if (newVarKeyword != oldVarKeyword)
        {
            return node.WithVarKeyword(newVarKeyword);
        }

        var oldModifiers = node.Modifiers;
        var newModifiers = ReplaceLastToken(oldModifiers, newToken);
        if (newModifiers != oldModifiers)
        {
            return node.WithModifiers(newModifiers);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenFunctionDeclaration? ReplaceLastToken(
        GreenFunctionDeclaration? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceLastToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        var oldExpressionBody = node.ExpressionBody;
        var newExpressionBody = ReplaceLastToken(oldExpressionBody, newToken);
        if (newExpressionBody != oldExpressionBody)
        {
            return node.WithExpressionBody(newExpressionBody);
        }

        var oldBody = node.Body;
        var newBody = ReplaceLastToken(oldBody, newToken);
        if (newBody != oldBody)
        {
            return node.WithBody(newBody);
        }

        var oldReturnType = node.ReturnType;
        var newReturnType = ReplaceLastToken(oldReturnType, newToken);
        if (newReturnType != oldReturnType)
        {
            return node.WithReturnType(newReturnType);
        }

        var oldParameters = node.Parameters;
        var newParameters = ReplaceLastToken(oldParameters, newToken);
        if (newParameters != oldParameters)
        {
            return node.WithParameters(newParameters);
        }

        var oldIdentifier = node.Identifier;
        var newIdentifier = ReplaceLastToken(oldIdentifier, newToken);
        if (newIdentifier != oldIdentifier)
        {
            return node.WithIdentifier(newIdentifier);
        }

        var oldFuncKeyword = node.FuncKeyword;
        var newFuncKeyword = ReplaceLastToken(oldFuncKeyword, newToken);
        if (newFuncKeyword != oldFuncKeyword)
        {
            return node.WithFuncKeyword(newFuncKeyword);
        }

        var oldModifiers = node.Modifiers;
        var newModifiers = ReplaceLastToken(oldModifiers, newToken);
        if (newModifiers != oldModifiers)
        {
            return node.WithModifiers(newModifiers);
        }

        return node;
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static GreenUsingDirective? ReplaceLastToken(
        GreenUsingDirective? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        var oldSemicolon = node.Semicolon;
        var newSemicolon = ReplaceLastToken(oldSemicolon, newToken);
        if (newSemicolon != oldSemicolon)
        {
            return node.WithSemicolon(newSemicolon);
        }

        var oldName = node.Name;
        var newName = ReplaceLastToken(oldName, newToken);
        if (newName != oldName)
        {
            return node.WithName(newName);
        }

        var oldUsingKeyword = node.UsingKeyword;
        var newUsingKeyword = ReplaceLastToken(oldUsingKeyword, newToken);
        if (newUsingKeyword != oldUsingKeyword)
        {
            return node.WithUsingKeyword(newUsingKeyword);
        }

        return node;
    }
}
