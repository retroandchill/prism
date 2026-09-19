// @file AttributesTest.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Compiling;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Tests;

public class AttributesTest
{
    [Test]
    public void CanDeclareAttributes()
    {
        var tree = SyntaxTree.Parse(
            """
            public attribute Foo;
            internal attribute Bar;
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var foos = compilation.Assembly.GlobalNamespace.GetMembers("Foo");
        var bars = compilation.Assembly.GlobalNamespace.GetMembers("Bar");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(foos, Has.Length.EqualTo(1));
            Assert.That(bars, Has.Length.EqualTo(1));
        }

        var foo = foos[0];
        var bar = bars[0];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foo, Is.InstanceOf<NamedTypeSymbol>());
            Assert.That(bar, Is.InstanceOf<NamedTypeSymbol>());
        }

        var fooType = (NamedTypeSymbol)foo;
        var barType = (NamedTypeSymbol)bar;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(fooType.Kind, Is.EqualTo(NamedTypeKind.Attribute));
            Assert.That(fooType.DeclaredVisibility, Is.EqualTo(DeclaredVisibility.Public));
            Assert.That(barType.Kind, Is.EqualTo(NamedTypeKind.Attribute));
            Assert.That(barType.DeclaredVisibility, Is.EqualTo(DeclaredVisibility.Internal));
        }
    }
}
