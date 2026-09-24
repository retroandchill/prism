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

    [Test]
    public void FunctionsCanHaveAttributes()
    {
        var tree = SyntaxTree.Parse(
            """
            public attribute Foo;

            [Foo]
            public func Bar() { }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var bars = compilation.Assembly.GlobalNamespace.GetMembers("Bar");
        Assert.That(bars, Has.Length.EqualTo(1));

        var bar = bars[0];
        Assert.That(bar, Is.InstanceOf<FunctionSymbol>());

        var barFunction = (FunctionSymbol)bar;
        var attributes = barFunction.GetAttributes();
        Assert.That(attributes, Has.Length.EqualTo(1));
        var attribute = attributes[0];
        Assert.That(attribute.Attribute.Name, Is.EqualTo("Foo"));
    }

    [Test]
    public void AttributesCanHaveArguments()
    {
        var tree = SyntaxTree.Parse(
            """
            public attribute Foo(name: str&);

            [Foo("baz")]
            public func Bar() { }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var bars = compilation.Assembly.GlobalNamespace.GetMembers("Bar");
        Assert.That(bars, Has.Length.EqualTo(1));

        var bar = bars[0];
        Assert.That(bar, Is.InstanceOf<FunctionSymbol>());

        var barFunction = (FunctionSymbol)bar;
        var attributes = barFunction.GetAttributes();
        Assert.That(attributes, Has.Length.EqualTo(1));
        var attribute = attributes[0];
        Assert.That(attribute.Attribute.Name, Is.EqualTo("Foo"));

        var parameters = attribute.Arguments;
        Assert.That(parameters, Has.Length.EqualTo(1));
        var parameter = parameters[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(parameter.Parameter.Name, Is.EqualTo("name"));
            Assert.That(parameter.Value.IsString);
        }
        Assert.That(parameter.Value.AsString(), Is.EqualTo("baz"));
    }
}
