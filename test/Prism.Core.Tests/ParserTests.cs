// @file ParserTests.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using Prism.Core.Parser;
using Prism.Core.Syntax;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Tests;

public sealed class ParserTests
{
    [Test]
    public void CanParseVariableDeclarations()
    {
        var parser = new LanguageParser("var value: i32 = 5;");
        var declaration = parser.ParseDeclaration();

        Assert.That(declaration.ContainsDiagnostics, Is.False);
        Assert.That(declaration is GreenVariableDeclaration, Is.True);

        var variable = (GreenVariableDeclaration)declaration;
        var identifier = new SyntaxToken(variable.Identifier, 0);
        var name = identifier.TryGetValue<IdentifierData>();

        Assert.That(name.HasValue, Is.True);
        Assert.That(name.Value.Text, Is.EqualTo("value"));

        var type = variable.Type;
        Assert.That(type, Is.Not.Null);
        Assert.That(type.Type is GreenPredefinedType, Is.True);

        var builtInType = ((GreenPredefinedType)type.Type).Keyword;
        Assert.That(builtInType.Kind, Is.EqualTo(SyntaxKind.I32Keyword));

        var initializer = variable.Initializer;
        Assert.That(initializer, Is.Not.Null);

        var expression = initializer.Value;
        Assert.That(expression is GreenLiteralExpression, Is.True);

        var literal = new SyntaxToken(((GreenLiteralExpression)expression).Value, 0);
        var value = literal.TryGetValue<IntegerLiteralData>();

        Assert.That(value.HasValue, Is.True);
        Assert.That(value.Value.Value, Is.EqualTo(new BigInteger(5)));

        var firstToken = declaration.FirstToken;
        Assert.That(firstToken, Is.Not.Null);
        Assert.That(firstToken.Kind, Is.EqualTo(SyntaxKind.VarKeyword));

        var lastToken = declaration.LastToken;
        Assert.That(lastToken, Is.Not.Null);
        Assert.That(lastToken.Kind, Is.EqualTo(SyntaxKind.SemicolonToken));
    }

    [Test]
    public void CanParseFunctionDeclarations()
    {
        var parser = new LanguageParser(
            "func add(x: i32, y: i32): i32 {\n" + "    return x + y;\n" + "}"
        );

        var declaration = parser.ParseDeclaration();

        Assert.That(declaration.ContainsDiagnostics, Is.False);
        Assert.That(declaration is GreenFunctionDeclaration, Is.True);

        var function = (GreenFunctionDeclaration)declaration;

        var identifier = new SyntaxToken(function.Identifier, 0);
        var name = identifier.TryGetValue<IdentifierData>();

        Assert.That(name.HasValue, Is.True);
        Assert.That(name.Value.Text, Is.EqualTo("add"));

        var type = function.ReturnType;
        Assert.That(type, Is.Not.Null);
        Assert.That(type.Type is GreenPredefinedType, Is.True);

        var builtInType = ((GreenPredefinedType)type.Type).Keyword;
        Assert.That(builtInType.Kind, Is.EqualTo(SyntaxKind.I32Keyword));

        var parameters = function.Parameters.Parameters;
        Assert.That(parameters.Count, Is.EqualTo(2));

        var param1Name = new SyntaxToken(parameters[0].Name, 0);
        var param1NameValue = param1Name.TryGetValue<IdentifierData>();
        Assert.That(param1NameValue.HasValue, Is.True);
        Assert.That(param1NameValue.Value.Text, Is.EqualTo("x"));

        var param2Name = new SyntaxToken(parameters[1].Name, 0);
        var param2NameValue = param2Name.TryGetValue<IdentifierData>();
        Assert.That(param2NameValue.HasValue, Is.True);
        Assert.That(param2NameValue.Value.Text, Is.EqualTo("y"));

        Assert.That(function.Body, Is.Not.Null);
        Assert.That(function.ExpressionBody, Is.Null);
        Assert.That(function.Semicolon, Is.Null);
    }

    [Test]
    public void ExpressionPrecedenceIsApplied()
    {
        var parser = new LanguageParser("1 + 2 * 3");
        var expression = parser.ParseExpression();

        Assert.That(expression is GreenBinaryExpression, Is.True);

        var binaryExpression = (GreenBinaryExpression)expression;
        Assert.That(binaryExpression.Op.Kind, Is.EqualTo(SyntaxKind.PlusToken));
        Assert.That(binaryExpression.Left is GreenLiteralExpression, Is.True);
        Assert.That(binaryExpression.Right is GreenBinaryExpression, Is.True);

        var rightExpression = (GreenBinaryExpression)binaryExpression.Right;
        Assert.That(rightExpression.Op.Kind, Is.EqualTo(SyntaxKind.StarToken));
        Assert.That(rightExpression.Left is GreenLiteralExpression, Is.True);
        Assert.That(rightExpression.Right is GreenLiteralExpression, Is.True);
    }

    [Test]
    public void ParenthesesAreApplied()
    {
        var parser = new LanguageParser("(1 + 2) * 3");
        var expression = parser.ParseExpression();

        Assert.That(expression is GreenBinaryExpression, Is.True);

        var binaryExpression = (GreenBinaryExpression)expression;
        Assert.That(binaryExpression.Op.Kind, Is.EqualTo(SyntaxKind.StarToken));
        Assert.That(binaryExpression.Right is GreenLiteralExpression, Is.True);
        Assert.That(binaryExpression.Left is GreenParenthesizedExpression, Is.True);

        var leftExpression = (GreenParenthesizedExpression)binaryExpression.Left;
        Assert.That(leftExpression.Expression is GreenBinaryExpression, Is.True);

        var inner = (GreenBinaryExpression)leftExpression.Expression;
        Assert.That(inner.Op.Kind, Is.EqualTo(SyntaxKind.PlusToken));
        Assert.That(inner.Left is GreenLiteralExpression, Is.True);
        Assert.That(inner.Right is GreenLiteralExpression, Is.True);
    }

    [Test]
    public void PrefixOperatorsAreApplied()
    {
        var parser = new LanguageParser("-a * b + !c");
        var expression = parser.ParseExpression();

        Assert.That(expression is GreenBinaryExpression, Is.True);

        var binaryExpression = (GreenBinaryExpression)expression;
        Assert.That(binaryExpression.Op.Kind, Is.EqualTo(SyntaxKind.PlusToken));
        Assert.That(binaryExpression.Left is GreenBinaryExpression, Is.True);
        Assert.That(binaryExpression.Right is GreenPrefixExpression, Is.True);

        var left = (GreenBinaryExpression)binaryExpression.Left;
        Assert.That(left.Op.Kind, Is.EqualTo(SyntaxKind.StarToken));
        Assert.That(left.Left is GreenPrefixExpression, Is.True);
        Assert.That(left.Right is GreenIdentifierExpression, Is.True);

        Assert.That(((GreenPrefixExpression)left.Left).Op.Kind, Is.EqualTo(SyntaxKind.MinusToken));
        Assert.That(
            ((GreenPrefixExpression)binaryExpression.Right).Op.Kind,
            Is.EqualTo(SyntaxKind.BangToken)
        );
    }

    [Test]
    public void CanMixPrefixAndPostfixOperators()
    {
        var parser = new LanguageParser("++x++");
        var expression = parser.ParseExpression();

        Assert.That(expression is GreenPrefixExpression, Is.True);

        var outer = (GreenPrefixExpression)expression;
        Assert.That(outer.Op.Kind, Is.EqualTo(SyntaxKind.PlusPlusToken));
        Assert.That(outer.Operand is GreenPostfixExpression, Is.True);

        var inner = (GreenPostfixExpression)outer.Operand;
        Assert.That(inner.Op.Kind, Is.EqualTo(SyntaxKind.PlusPlusToken));
    }

    [Test]
    public void AssignmentWorksAsExpected()
    {
        var parser = new LanguageParser("x = -a * b + !c");
        var expression = parser.ParseExpression();

        Assert.That(expression is GreenAssignmentExpression, Is.True);

        var assignment = (GreenAssignmentExpression)expression;
        Assert.That(assignment.Op.Kind, Is.EqualTo(SyntaxKind.EqualToken));
        Assert.That(assignment.Left is GreenIdentifierExpression, Is.True);
        Assert.That(assignment.Right is GreenBinaryExpression, Is.True);
    }

    [Test]
    public void CastWorksAsExpected()
    {
        var parser = new LanguageParser("x as i32");
        var expression = parser.ParseExpression();

        Assert.That(expression is GreenCastExpression, Is.True);

        var cast = (GreenCastExpression)expression;
        Assert.That(cast.Type is GreenPredefinedType, Is.True);
        Assert.That(cast.Operand is GreenIdentifierExpression, Is.True);
    }
}
