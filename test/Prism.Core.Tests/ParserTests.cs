// @file ParserTests.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Parse;
using Prism.Core.Tests.Constraints;

namespace Prism.Core.Tests;

public class ParserTests
{
    [Test]
    public void VariableDeclaration()
    {
        var context = new CompilationContext(new SourceFile("var value: i32 = 5;"));
        var parser = new Parser(context);
        var declaration = parser.ParseDeclaration();

        Assert.That(context.Diagnostics, Is.Empty);
        Assert.That(declaration, Is.InstanceOf<VariableDeclarationSyntax>());

        var variable = (VariableDeclarationSyntax)declaration;
        Assert.That(variable.IsMutable, Is.False);

        Assert.That(variable.Identifier.Name.ToString(), Is.EqualTo("value"));
        Assert.That(variable.Type, Is.InstanceOf<BuiltInTypeSyntax>());
        var type = (BuiltInTypeSyntax)variable.Type;
        Assert.That(type.Type, Is.EqualTo(BuiltInType.I32));

        Assert.That(variable.Initializer, Is.InstanceOf<LiteralExpressionSyntax>());
        var initializer = (LiteralExpressionSyntax)variable.Initializer;
        Assert.That(initializer.Kind, Is.EqualTo(LiteralKind.Integer));
    }

    [Test]
    public void FunctionDeclaration()
    {
        var context = new CompilationContext(
            new SourceFile(
                """
                func add(x: i32, y: i32) -> i32 {
                    return x + y;
                }
                """
            )
        );
        var parser = new Parser(context);
        var declaration = parser.ParseDeclaration();

        Assert.That(context.Diagnostics, Is.Empty);
        Assert.That(declaration, Is.InstanceOf<FunctionDeclarationSyntax>());

        var function = (FunctionDeclarationSyntax)declaration;
        Assert.That(function.Identifier.Name.ToString(), Is.EqualTo("add"));
        Assert.That(function.ReturnType, Is.InstanceOf<BuiltInTypeSyntax>());
        var returnType = (BuiltInTypeSyntax)function.ReturnType;
        Assert.That(returnType.Type, Is.EqualTo(BuiltInType.I32));

        Assert.That(function.Parameters.Count, Is.EqualTo(2));
        Assert.That(function.Parameters[0].Identifier.Name.ToString(), Is.EqualTo("x"));
        Assert.That(function.Parameters[0].IsMutable, Is.False);
        Assert.That(function.Parameters[0].Type, Is.InstanceOf<BuiltInTypeSyntax>());
        var parameter1Type = (BuiltInTypeSyntax)function.Parameters[0].Type;
        Assert.That(parameter1Type.Type, Is.EqualTo(BuiltInType.I32));
        Assert.That(function.Parameters[1].Identifier.Name.ToString(), Is.EqualTo("y"));
        Assert.That(function.Parameters[1].IsMutable, Is.False);
        Assert.That(function.Parameters[1].Type, Is.InstanceOf<BuiltInTypeSyntax>());
        var parameter2Type = (BuiltInTypeSyntax)function.Parameters[1].Type;
        Assert.That(parameter2Type.Type, Is.EqualTo(BuiltInType.I32));

        Assert.That(function.Body, Has.Alternative<BlockSyntax>());
    }

    [Test]
    public void ExpressionPrecedence()
    {
        var context = new CompilationContext(new SourceFile("1 + 2 * 3"));
        var parser = new Parser(context);
        var expression = parser.ParseExpression();

        Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
        var binaryExpression = (BinaryExpressionSyntax)expression;
        Assert.That(binaryExpression.Operator, Is.EqualTo(BinaryOperator.Add));
        Assert.That(binaryExpression.Left, Is.InstanceOf<LiteralExpressionSyntax>());
        Assert.That(binaryExpression.Right, Is.InstanceOf<BinaryExpressionSyntax>());
        var rightBinaryExpression = (BinaryExpressionSyntax)binaryExpression.Right;
        Assert.That(rightBinaryExpression.Operator, Is.EqualTo(BinaryOperator.Mul));
        Assert.That(rightBinaryExpression.Left, Is.InstanceOf<LiteralExpressionSyntax>());
        Assert.That(rightBinaryExpression.Right, Is.InstanceOf<LiteralExpressionSyntax>());
    }

    [Test]
    public void ParenthesesChangesOrderOfOperations()
    {
        var context = new CompilationContext(new SourceFile("(1 + 2) * 3"));
        var parser = new Parser(context);
        var expression = parser.ParseExpression();

        Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
        var binaryExpression = (BinaryExpressionSyntax)expression;
        Assert.That(binaryExpression.Operator, Is.EqualTo(BinaryOperator.Mul));
        Assert.That(binaryExpression.Left, Is.InstanceOf<BinaryExpressionSyntax>());
        Assert.That(binaryExpression.Right, Is.InstanceOf<LiteralExpressionSyntax>());
        var rightBinaryExpression = (BinaryExpressionSyntax)binaryExpression.Left;
        Assert.That(rightBinaryExpression.Operator, Is.EqualTo(BinaryOperator.Add));
        Assert.That(rightBinaryExpression.Left, Is.InstanceOf<LiteralExpressionSyntax>());
        Assert.That(rightBinaryExpression.Right, Is.InstanceOf<LiteralExpressionSyntax>());
    }

    [Test]
    public void UnaryOperators()
    {
        var context = new CompilationContext(new SourceFile("-a * b + !c"));
        var parser = new Parser(context);
        var expression = parser.ParseExpression();

        Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
        var binaryExpression = (BinaryExpressionSyntax)expression;
        Assert.That(binaryExpression.Operator, Is.EqualTo(BinaryOperator.Add));
        Assert.That(binaryExpression.Left, Is.InstanceOf<BinaryExpressionSyntax>());
        Assert.That(binaryExpression.Right, Is.InstanceOf<UnaryExpressionSyntax>());
        var left = (BinaryExpressionSyntax)binaryExpression.Left;
        Assert.That(left.Operator, Is.EqualTo(BinaryOperator.Mul));
        Assert.That(left.Left, Is.InstanceOf<UnaryExpressionSyntax>());
        Assert.That(left.Right, Is.InstanceOf<IdentifierExpressionSyntax>());
        Assert.That(((UnaryExpressionSyntax)left.Left).Operator, Is.EqualTo(UnaryOperator.Negate));

        Assert.That(
            ((UnaryExpressionSyntax)binaryExpression.Right).Operator,
            Is.EqualTo(UnaryOperator.LogicalNot)
        );
    }

    [Test]
    public void PrefixAndPostfixOperators()
    {
        var context = new CompilationContext(new SourceFile("++x++"));
        var parser = new Parser(context);
        var expression = parser.ParseExpression();

        Assert.That(expression, Is.InstanceOf<UnaryExpressionSyntax>());
        var outer = (UnaryExpressionSyntax)expression;
        Assert.That(outer.Operator, Is.EqualTo(UnaryOperator.PreIncrement));
        Assert.That(outer.Operand, Is.InstanceOf<UnaryExpressionSyntax>());
        var inner = (UnaryExpressionSyntax)outer.Operand;
        Assert.That(inner.Operator, Is.EqualTo(UnaryOperator.PostIncrement));
        Assert.That(inner.Operand, Is.InstanceOf<IdentifierExpressionSyntax>());
    }

    [Test]
    public void Assignment()
    {
        var context = new CompilationContext(new SourceFile("x = -a * b + !c"));
        var parser = new Parser(context);
        var expression = parser.ParseExpression();

        Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
        var assignment = (BinaryExpressionSyntax)expression;
        Assert.That(assignment.Operator, Is.EqualTo(BinaryOperator.Assign));
        Assert.That(assignment.Left, Is.InstanceOf<IdentifierExpressionSyntax>());
        Assert.That(assignment.Right, Is.InstanceOf<BinaryExpressionSyntax>());
        var right = (BinaryExpressionSyntax)assignment.Right;
        Assert.That(right.Operator, Is.EqualTo(BinaryOperator.Add));
        Assert.That(right.Left, Is.InstanceOf<BinaryExpressionSyntax>());
        Assert.That(right.Right, Is.InstanceOf<UnaryExpressionSyntax>());
    }
}
