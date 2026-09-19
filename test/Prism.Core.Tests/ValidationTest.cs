// @file ValidationTest.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Compiling;
using Prism.Core.Configuration;
using Prism.Core.Syntax;

namespace Prism.Core.Tests;

public class ValidationTest
{
    private static readonly CompilationSettings Settings = new()
    {
        OutputKind = OutputKind.StaticLibrary,
    };

    [Test]
    public void CannotHaveDuplicateDeclarations()
    {
        var tree = SyntaxTree.Parse(
            """
            var x: i32 = 5;
            var x: i32 = 7;
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var diagnostics = compilation.GetDeclarationDiagnostics();
        Assert.That(diagnostics, Has.Length.EqualTo(1));
        Assert.That(diagnostics[0].GetMessage(), Is.EqualTo("Duplicate declaration of 'x'"));
    }

    [Test]
    public void DuplicateNamesAreFineInSeparateNamespaces()
    {
        var tree = SyntaxTree.Parse(
            """
            var x: i32 = 5;

            namespace foo {
                var x: i32 = 7;
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var diagnostics = compilation.GetDeclarationDiagnostics();
        Assert.That(diagnostics, Is.Empty);
    }

    [Test]
    public void FunctionOverloadingIsAllowed()
    {
        var tree = SyntaxTree.Parse(
            """
            func foo(x: i32) { }
            func foo(x: i32, y: i32) { }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var diagnostics = compilation.GetDeclarationDiagnostics();
        Assert.That(diagnostics, Is.Empty);
    }

    [Test]
    public void OverloadsMustDifferByParameters()
    {
        var tree = SyntaxTree.Parse(
            """
            func foo(x: i32) { }
            func foo(y: i32) { }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var diagnostics = compilation.GetDeclarationDiagnostics();
        Assert.That(diagnostics, Has.Length.EqualTo(1));
        Assert.That(diagnostics[0].GetMessage(), Is.EqualTo("Duplicate declaration of 'foo'"));
    }

    [Test]
    public void CannotMixFunctionOverloadsAndNonFunctionDeclarations()
    {
        var tree = SyntaxTree.Parse(
            """
            var foo: i32 = 5;
            func foo(x: i32) { }
            func foo(x: i32, y: i32) { }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var diagnostics = compilation.GetDeclarationDiagnostics();
        Assert.That(diagnostics, Has.Length.EqualTo(1));
        Assert.That(diagnostics[0].GetMessage(), Is.EqualTo("Duplicate declaration of 'foo'"));
    }

    [Test]
    public void FunctionParameterNamesMustBeUnique()
    {
        var tree = SyntaxTree.Parse("func foo(x: i32, x: i32) { }");

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var diagnostics = compilation.GetDeclarationDiagnostics();
        Assert.That(diagnostics, Has.Length.EqualTo(1));
        Assert.That(diagnostics[0].GetMessage(), Is.EqualTo("Duplicate declaration of 'x'"));
    }

    [Test]
    public void DefaultsMustBeUsedAfterFirstSpecifiedDefault()
    {
        var tree = SyntaxTree.Parse("func foo(x: i32, y: i32 = 0, z: i32) { }");

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var diagnostics = compilation.GetDeclarationDiagnostics();
        Assert.That(diagnostics, Has.Length.EqualTo(1));
        Assert.That(
            diagnostics[0].GetMessage(),
            Is.EqualTo("Parameter 'z' must specify a default value")
        );
    }

    [Test]
    public void CannotUseUndeclaredOrUninitializedVariables()
    {
        var tree = SyntaxTree.Parse(
            """
            func foo(): i32 {
                x = 5;
                var x: i32;
                var y: i32;

                return x + y;
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree], Settings);

        var diagnostics = compilation.GetFunctionBodyDiagnostics();
        Assert.That(diagnostics, Has.Length.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                diagnostics[0].GetMessage(),
                Is.EqualTo("Cannot use variable 'x' before it is declared")
            );
            Assert.That(diagnostics[1].GetMessage(), Is.EqualTo("Variable 'y' is uninitialized"));
        }
    }

    [Test]
    public void CannotShadowParameters()
    {
        var tree = SyntaxTree.Parse(
            """
            func foo(x: i32, y: i32 = 0, z: i32) {
                var x = 5;
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree], Settings);

        var diagnostics = compilation.GetFunctionBodyDiagnostics();
        Assert.That(diagnostics, Has.Length.EqualTo(1));
        Assert.That(
            diagnostics[0].GetMessage(),
            Is.EqualTo("Variable 'x' shadows a value from an outer scope")
        );
    }

    [Test]
    public void CanOnlyShadowLocalsThatAreNotYetDeclared()
    {
        var tree = SyntaxTree.Parse(
            """
            func foo(x: i32) {
                var y: i32;
                if (x >= 5) {
                    var y = x * 2;
                    var z = x + 2;
                }

                var z = 4;
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree], Settings);

        var diagnostics = compilation.GetFunctionBodyDiagnostics();
        Assert.That(diagnostics, Has.Length.EqualTo(1));
        Assert.That(
            diagnostics[0].GetMessage(),
            Is.EqualTo("Variable 'y' shadows a value from an outer scope")
        );
    }

    [Test]
    public void RedefinitionOfLocalVariablesInTheSameScope()
    {
        var tree = SyntaxTree.Parse(
            """
            func foo(x: i32) {
                var y = x;
                var y = 4;
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree], Settings);

        var diagnostics = compilation.GetFunctionBodyDiagnostics();
        Assert.That(diagnostics, Has.Length.EqualTo(1));
        Assert.That(
            diagnostics[0].GetMessage(),
            Is.EqualTo("Variable 'y' shadows a value from an outer scope")
        );
    }

    [Test]
    public void ExternFunctionsCannotHaveBody()
    {
        var tree = SyntaxTree.Parse(
            """
            extern func foo(x: i32) {

            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree], Settings);

        var diagnostics = compilation.GetDeclarationDiagnostics();
        Assert.That(diagnostics, Has.Length.EqualTo(1));
        Assert.That(
            diagnostics[0].GetMessage(),
            Is.EqualTo("Extern function 'foo' cannot have a body")
        );
    }

    [Test]
    public void FunctionMustHaveBodyIfNotExtern()
    {
        var tree = SyntaxTree.Parse("func foo(x: i32);");

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree], Settings);

        var diagnostics = compilation.GetDeclarationDiagnostics();
        Assert.That(diagnostics, Has.Length.EqualTo(1));
        Assert.That(
            diagnostics[0].GetMessage(),
            Is.EqualTo("Function 'foo' must either define a body or be marked 'extern'")
        );
    }
}
