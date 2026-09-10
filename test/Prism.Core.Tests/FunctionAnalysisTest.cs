// @file FunctionAnalysisTest.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Compiling;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Tests;

public class FunctionAnalysisTest
{
    [Test]
    public void MustReturnIfNotVoid()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(p: i32): f64 {

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
        Assert.That(bodyAnalysis.Diagnostics[0].Id, Is.EqualTo("AllPathsMustReturnValue"));
    }

    [Test]
    public void MustReturnValueOnAllPaths()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(p: i32): f64 {
                if (p > 0) {
                    return p * 2;
                }
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
        Assert.That(bodyAnalysis.Diagnostics[0].Id, Is.EqualTo("AllPathsMustReturnValue"));
    }

    [Test]
    public void MustReturnValueIfNotVoid()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(p: i32): f64 {
                return;
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
        Assert.That(bodyAnalysis.Diagnostics[0].Id, Is.EqualTo("StatementMustReturnValue"));
    }

    [Test]
    public void VoidFunctionCannotReturnValue()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(p: i32) {
                return 4;
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
        Assert.That(bodyAnalysis.Diagnostics[0].Id, Is.EqualTo("CannotReturnValue"));
    }

    [Test]
    public void InfiniteLoopsCanOmitTailReturn()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(p: i32, mutable q: i32): i32 {
                loop
                {
                    if (p == q) {
                        return p;
                    }

                    q++;
                }
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
    public void InfiniteFromForCanOmitTailReturn()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(p: i32, mutable q: i32): i32 {
                for (;;)
                {
                    if (p == q) {
                        return p;
                    }

                    q++;
                }
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
    public void InfiniteFromWhileCanOmitTailReturn()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(p: i32, mutable q: i32): i32 {
                while (true)
                {
                    if (p == q) {
                        return p;
                    }

                    q++;
                }
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
    public void UnreachableCodeIsAnError()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(p: i32 mutable&) {
                *p += 3;
                return;
                *p *= 43;
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
        Assert.That(bodyAnalysis.Diagnostics[0].Id, Is.EqualTo("UnreachableCode"));
    }

    [Test]
    public void InfiniteLoopsWithBreaksCanReachOutside()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(p: i32) {
                outer: loop  {
                    for (mutable var i = 0; i < p; i++) {
                        if (i == p) {
                            break outer;
                        }
                    }
                }

                var v = 0;
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
    public void InfiniteLoopsWithoutBreaksCannotReachOutside()
    {
        var tree = SyntaxTree.Parse(
            """
            func f(p: i32) {
                outer: loop  {
                    for (mutable var i = 0; i < p; i++) {
                        if (i == p) {
                            return;
                        }
                    }
                }

                var v = 0;
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
        Assert.That(bodyAnalysis.Diagnostics[0].Id, Is.EqualTo("UnreachableCode"));
    }
}
