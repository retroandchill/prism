// @file OverloadResolutionTest.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
        var bodyAnalysis = compilation.GetBoundBody(function);

        Assert.That(bodyAnalysis.ParameterDefaults, Has.Length.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(bodyAnalysis.ParameterDefaults[0], Is.Null);
            Assert.That(bodyAnalysis.ParameterDefaults[1], Is.Not.Null);
            Assert.That(bodyAnalysis.ParameterDefaults[2], Is.Not.Null);
        }
    }
}
