// @file GenericParserTests.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Syntax.Green;
using Prism.Syntax.Parser;

namespace Prism.Tests;

public class GenericParserTests
{
    [Test]
    public void ParseGenericParameterSignatures()
    {
        var parser = new LanguageParser(
            """
            public func foo<T, const N: usize>(x: T[N])
            {

            }
            """
        );
        var declaration = parser.ParseTopLevelDeclaration();

        Assert.That(declaration, Is.InstanceOf<GreenFunctionDeclaration>());
        var functionDeclaration = (GreenFunctionDeclaration)declaration;

        Assert.That(functionDeclaration.GenericParameters, Is.Not.Null);
        Assert.That(functionDeclaration.GenericParameters.Parameters, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                functionDeclaration.GenericParameters.Parameters[0],
                Is.InstanceOf<GreenTypeParameter>()
            );
            Assert.That(
                functionDeclaration.GenericParameters.Parameters[1],
                Is.InstanceOf<GreenConstantParameter>()
            );
        }
    }

    [Test]
    public void ParseGenericFunctionInvocation()
    {
        var parser = new LanguageParser("foo<i32, 5>(arr)");
        var expression = parser.ParseExpression();

        Assert.That(expression, Is.InstanceOf<GreenInvocationExpression>());
        var invocation = (GreenInvocationExpression)expression;

        Assert.That(invocation.Callee, Is.InstanceOf<GreenIdentifierExpression>());
        var callee = (GreenIdentifierExpression)invocation.Callee;

        Assert.That(callee.Value, Is.InstanceOf<GreenSimpleName>());
        var name = (GreenSimpleName)callee.Value;

        Assert.That(name.GenericArguments, Is.Not.Null);
        Assert.That(name.GenericArguments.Arguments, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(name.GenericArguments.Arguments[0], Is.InstanceOf<GreenTypeArgument>());
            Assert.That(
                name.GenericArguments.Arguments[1],
                Is.InstanceOf<GreenExpressionArgument>()
            );
        }
    }

    [Test]
    public void ParseGenericFunctionInvocationWithComplexConstantExpression()
    {
        var parser = new LanguageParser("foo<i32, expr x + 5>(arr)");
        var expression = parser.ParseExpression();

        Assert.That(expression, Is.InstanceOf<GreenInvocationExpression>());
        var invocation = (GreenInvocationExpression)expression;

        Assert.That(invocation.Callee, Is.InstanceOf<GreenIdentifierExpression>());
        var callee = (GreenIdentifierExpression)invocation.Callee;

        Assert.That(callee.Value, Is.InstanceOf<GreenSimpleName>());
        var name = (GreenSimpleName)callee.Value;

        Assert.That(name.GenericArguments, Is.Not.Null);
        Assert.That(name.GenericArguments.Arguments, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(name.GenericArguments.Arguments[0], Is.InstanceOf<GreenTypeArgument>());
            Assert.That(
                name.GenericArguments.Arguments[1],
                Is.InstanceOf<GreenExpressionArgument>()
            );
        }
    }

    [Test]
    public void ParseGenericFunctionInvocationWithNamedConstant()
    {
        var parser = new LanguageParser("foo<i32, SIZE>(arr)");
        var expression = parser.ParseExpression();

        Assert.That(expression, Is.InstanceOf<GreenInvocationExpression>());
        var invocation = (GreenInvocationExpression)expression;

        Assert.That(invocation.Callee, Is.InstanceOf<GreenIdentifierExpression>());
        var callee = (GreenIdentifierExpression)invocation.Callee;

        Assert.That(callee.Value, Is.InstanceOf<GreenSimpleName>());
        var name = (GreenSimpleName)callee.Value;

        Assert.That(name.GenericArguments, Is.Not.Null);
        Assert.That(name.GenericArguments.Arguments, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(name.GenericArguments.Arguments[0], Is.InstanceOf<GreenTypeArgument>());
            Assert.That(
                name.GenericArguments.Arguments[1],
                Is.InstanceOf<GreenIdentifierGenericArgument>()
            );
        }
    }

    [Test]
    public void ParseGenericFunctionInvocationWithArrayTypes()
    {
        var parser = new LanguageParser("foo<Foo[4], expr x[3]>(arr)");
        var expression = parser.ParseExpression();

        Assert.That(expression, Is.InstanceOf<GreenInvocationExpression>());
        var invocation = (GreenInvocationExpression)expression;

        Assert.That(invocation.Callee, Is.InstanceOf<GreenIdentifierExpression>());
        var callee = (GreenIdentifierExpression)invocation.Callee;

        Assert.That(callee.Value, Is.InstanceOf<GreenSimpleName>());
        var name = (GreenSimpleName)callee.Value;

        Assert.That(name.GenericArguments, Is.Not.Null);
        Assert.That(name.GenericArguments.Arguments, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(name.GenericArguments.Arguments[0], Is.InstanceOf<GreenTypeArgument>());
            Assert.That(
                name.GenericArguments.Arguments[1],
                Is.InstanceOf<GreenExpressionArgument>()
            );
        }
    }

    [Test]
    public void ParseGenericFunctionInvocationWithNullableAndReferenceTypes()
    {
        var parser = new LanguageParser("foo<Foo?, Bar&, Baz mutable &>(arr)");
        var expression = parser.ParseExpression();

        Assert.That(expression, Is.InstanceOf<GreenInvocationExpression>());
        var invocation = (GreenInvocationExpression)expression;

        Assert.That(invocation.Callee, Is.InstanceOf<GreenIdentifierExpression>());
        var callee = (GreenIdentifierExpression)invocation.Callee;

        Assert.That(callee.Value, Is.InstanceOf<GreenSimpleName>());
        var name = (GreenSimpleName)callee.Value;

        Assert.That(name.GenericArguments, Is.Not.Null);
        Assert.That(name.GenericArguments.Arguments, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(name.GenericArguments.Arguments[0], Is.InstanceOf<GreenTypeArgument>());
            Assert.That(name.GenericArguments.Arguments[1], Is.InstanceOf<GreenTypeArgument>());
            Assert.That(name.GenericArguments.Arguments[2], Is.InstanceOf<GreenTypeArgument>());
        }
    }

    [Test]
    public void ParseNestedGenerics()
    {
        var parser = new LanguageParser("foo<List<i32>>(arr)");
        var expression = parser.ParseExpression();

        Assert.That(expression, Is.InstanceOf<GreenInvocationExpression>());
        var invocation = (GreenInvocationExpression)expression;

        Assert.That(invocation.Callee, Is.InstanceOf<GreenIdentifierExpression>());
        var callee = (GreenIdentifierExpression)invocation.Callee;

        Assert.That(callee.Value, Is.InstanceOf<GreenSimpleName>());
        var name = (GreenSimpleName)callee.Value;

        Assert.That(name.GenericArguments, Is.Not.Null);
        Assert.That(name.GenericArguments.Arguments, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(name.GenericArguments.Arguments[0], Is.InstanceOf<GreenTypeArgument>());
        }
    }
}
