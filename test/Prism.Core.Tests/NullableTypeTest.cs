// @file NullableTypeTest.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Compiling;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Tests;

public class NullableTypeTest
{
    [Test]
    public void CanAssignNullToNullableTypes()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(cond: bool) {
                var x: i32? = null;
                var y: i32? = cond ? 3 : null;
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var members = compilation.Assembly.GlobalNamespace.GetMembers("f");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var function = (FunctionSymbol)members[0];
        var bodyAnalysis = compilation.GetBoundBody(function);

        Assert.That(bodyAnalysis.Diagnostics, Is.Empty);
    }

    [Test]
    public void ReturningAnUncheckedNullIsAnError()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(x: i32?): i32 {
                return x;
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var members = compilation.Assembly.GlobalNamespace.GetMembers("f");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var function = (FunctionSymbol)members[0];
        var bodyAnalysis = compilation.GetBoundBody(function);

        Assert.That(bodyAnalysis.Diagnostics, Has.Length.EqualTo(1));
        Assert.That(bodyAnalysis.Diagnostics[0].Id, Is.EqualTo("MayBeNull"));
    }

    [Test]
    public void NullChecksAllowUnwraps()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(x: int32?): i32 {
                if (x != null) {
                    return x;
                }

                return 0;
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var members = compilation.Assembly.GlobalNamespace.GetMembers("f");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var function = (FunctionSymbol)members[0];
        var bodyAnalysis = compilation.GetBoundBody(function);

        Assert.That(bodyAnalysis.Diagnostics, Is.Empty);
    }

    [Test]
    public void NullChecksAllowUnwrapsOnOtherBranch()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(x: int32?): i32 {
                if (x == null) {
                    return 0;
                }

                return x;
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var members = compilation.Assembly.GlobalNamespace.GetMembers("f");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var function = (FunctionSymbol)members[0];
        var bodyAnalysis = compilation.GetBoundBody(function);

        Assert.That(bodyAnalysis.Diagnostics, Is.Empty);
    }

    [Test]
    public void BooleanAndsApplyNullChecksCorrectly()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(x: i32?, y: i32?): i32 {
                if (x != null && y != null) {
                    return x + y;
                }

                return 0;
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var members = compilation.Assembly.GlobalNamespace.GetMembers("f");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var function = (FunctionSymbol)members[0];
        var bodyAnalysis = compilation.GetBoundBody(function);

        Assert.That(bodyAnalysis.Diagnostics, Is.Empty);
    }

    [Test]
    public void BooleanAndsApplyNullChecksCorrectlyOnElse()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(x: i32?, y: i32?): i32 {
                if (x == null || y == null) {
                    return 0;
                }

                return x + y;
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var members = compilation.Assembly.GlobalNamespace.GetMembers("f");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var function = (FunctionSymbol)members[0];
        var bodyAnalysis = compilation.GetBoundBody(function);

        Assert.That(bodyAnalysis.Diagnostics, Is.Empty);
    }

    [Test]
    public void AppliesNullChecksToTernaries()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(x: i32?, y: i32?): i32 {
                return x != null && y != null ? x + y : 0;
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var members = compilation.Assembly.GlobalNamespace.GetMembers("f");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var function = (FunctionSymbol)members[0];
        var bodyAnalysis = compilation.GetBoundBody(function);

        Assert.That(bodyAnalysis.Diagnostics, Is.Empty);
    }
}
