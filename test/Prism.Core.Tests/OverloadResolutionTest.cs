// @file OverloadResolutionTest.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Tests;

public class OverloadResolutionTest
{
    [Test]
    public void DefaultValuesAreBound()
    {
        var tree = SyntaxTree.Parse("func foo(x: i32, y: i32 = 0, z: i32 = 0) { }");

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var members = compilation.Assembly.GlobalNamespace.GetMembers("foo");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var function = (FunctionSymbol)members[0];

        Assert.That(function.Parameters, Has.Length.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(function.Parameters[0].HasDefaultValue, Is.False);
            Assert.That(function.Parameters[1].HasDefaultValue, Is.True);
            Assert.That(function.Parameters[2].HasDefaultValue, Is.True);
        }
    }

    [Test]
    public void CanBindToOverloadWithDefaultParameter()
    {
        var tree = SyntaxTree.Parse(
            """
            func foo(x: i32, y: i32 = 0, z: i32 = 0) { }

            func bar(x: i32) {
                foo(x);
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var members = compilation.Assembly.GlobalNamespace.GetMembers("foo");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var foo = (FunctionSymbol)members[0];

        members = compilation.Assembly.GlobalNamespace.GetMembers("bar");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var bar = (FunctionSymbol)members[0];

        var body = compilation.GetBoundBody(bar);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(body.Diagnostics, Is.Empty);
            Assert.That(body.Body, Is.Not.Null);
        }

        Assert.That(body.Body, Is.InstanceOf<BoundBlock>());

        var block = (BoundBlock)body.Body;
        Assert.That(block.Statements[0], Is.InstanceOf<BoundExpressionStatement>());
        var statement = (BoundExpressionStatement)block.Statements[0];

        Assert.That(statement.Expression, Is.InstanceOf<BoundInvocation>());
        var call = (BoundInvocation)statement.Expression;
        Assert.That(call.Function, Is.SameAs(foo));
    }

    [Test]
    public void TooManyArgumentsProducesAnError()
    {
        var tree = SyntaxTree.Parse(
            """
            func foo(x: i32, y: i32 = 0, z: i32 = 0) { }

            func bar(x: i32) {
                foo(x, 4, 5, 6);
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var members = compilation.Assembly.GlobalNamespace.GetMembers("bar");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var bar = (FunctionSymbol)members[0];

        var body = compilation.GetBoundBody(bar);
        Assert.That(body.Diagnostics, Is.Not.Empty);
    }

    [Test]
    public void NamedArgumentsCanBeOutOfOrder()
    {
        var tree = SyntaxTree.Parse(
            """
            func foo(x: i32, y: i32 = 0, z: i32 = 0) { }

            func bar(x: i32) {
                foo(x, z: 4, y: 5);
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var members = compilation.Assembly.GlobalNamespace.GetMembers("foo");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var foo = (FunctionSymbol)members[0];

        members = compilation.Assembly.GlobalNamespace.GetMembers("bar");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var bar = (FunctionSymbol)members[0];

        var body = compilation.GetBoundBody(bar);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(body.Diagnostics, Is.Empty);
            Assert.That(body.Body, Is.Not.Null);
        }

        Assert.That(body.Body, Is.InstanceOf<BoundBlock>());

        var block = (BoundBlock)body.Body;
        Assert.That(block.Statements[0], Is.InstanceOf<BoundExpressionStatement>());
        var statement = (BoundExpressionStatement)block.Statements[0];

        Assert.That(statement.Expression, Is.InstanceOf<BoundInvocation>());
        var call = (BoundInvocation)statement.Expression;
        Assert.That(call.Function, Is.SameAs(foo));
    }

    [Test]
    public void PositionalBeforeNamedOnlyGetsReportedOnce()
    {
        var tree = SyntaxTree.Parse(
            """
            func foo(x: i32, y: i32) { }
            func foo(x: i32, y: i32, z: i32) { }

            func bar(x: i32) {
                foo(y: 5, x);
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var members = compilation.Assembly.GlobalNamespace.GetMembers("bar");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var bar = (FunctionSymbol)members[0];

        var body = compilation.GetBoundBody(bar);
        Assert.That(body.Diagnostics, Has.Length.EqualTo(1));
        Assert.That(body.Diagnostics[0].Id, Is.EqualTo("PositionalArgumentsFirst"));
    }
}
