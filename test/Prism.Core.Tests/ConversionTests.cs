// @file ConversionTests.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Compiling;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Tests;

public class ConversionTests
{
    [Test]
    public void CanConvertFromCharArrayToStr()
    {
        var tree = SyntaxTree.Parse(
            """
            extern func print(msg: str&);

            func f() {
                print(&['H', 'e', 'l', 'l', 'o']);

                var world = ['W', 'o', 'r', 'l', 'd'];
                print(&world);
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
    public void CanConvertFromStrToCharArray()
    {
        var tree = SyntaxTree.Parse(
            """
            extern func print(msg: char[]&);

            func f() {
                print("Hello world");
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
