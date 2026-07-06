// @file SemanticBinderTest.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Parse;
using Prism.Core.Semantic;
using Prism.Core.Semantic.Symbols;

namespace Prism.Core.Tests;

public class SemanticBinderTest
{
    /*
    private static (CompilationUnitSyntax Syntax, CompilationContext Context) CreateCompilationUnit(
        string code
    )
    {
        var context = new CompilationContext(new SourceFile(code));
        var parser = new Parser(context);
        return (parser.ParseCompilationUnit(), context);
    }

    [Test]
    public void BindVariableExplicitType()
    {
        var (syntax, context) = CreateCompilationUnit("var x: i32 = 5;");

        Assert.That(context.Diagnostics, Is.Empty);

        var scope = syntax.ScanDeclarations();
        var x = scope.FindDeclarations("x");
        Assert.That(x, Has.Length.EqualTo(1));

        var binder = new SemanticBinder();
        var symbol = binder.GetSymbol(x[0], context);
        Assert.That(symbol, Is.InstanceOf<VariableSymbol>());
        var variableSymbol = (VariableSymbol)symbol;
        Assert.That(variableSymbol.Type, Is.InstanceOf<NamedTypeSymbol>());

        var type = (NamedTypeSymbol)variableSymbol.Type;
        Assert.That(type.Name.ToString(), Is.EqualTo("i32"));
        Assert.That(type.BuiltInType, Is.EqualTo(BuiltInType.I32));
    }

    [Test]
    public void BindVariableInferredType()
    {
        var (syntax, context) = CreateCompilationUnit("var x = 5;");

        Assert.That(context.Diagnostics, Is.Empty);

        var scope = syntax.ScanDeclarations();
        var x = scope.FindDeclarations("x");
        Assert.That(x, Has.Length.EqualTo(1));

        var binder = new SemanticBinder();
        var symbol = binder.GetSymbol(x[0], context);
        Assert.That(symbol, Is.InstanceOf<VariableSymbol>());
        var variableSymbol = (VariableSymbol)symbol;
        Assert.That(variableSymbol.Type, Is.InstanceOf<NamedTypeSymbol>());

        var type = (NamedTypeSymbol)variableSymbol.Type;
        Assert.That(type.Name.ToString(), Is.EqualTo("i32"));
        Assert.That(type.BuiltInType, Is.EqualTo(BuiltInType.I32));
    }

    [Test]
    public void BindVariableFromOtherVariable()
    {
        var (syntax, context) = CreateCompilationUnit(
            """
            var x = y;
            var y = 4;
            """
        );

        Assert.That(context.Diagnostics, Is.Empty);

        var scope = syntax.ScanDeclarations();
        var x = scope.FindDeclarations("x");
        Assert.That(x, Has.Length.EqualTo(1));

        var binder = new SemanticBinder();
        var symbol = binder.GetSymbol(x[0], context);
        Assert.That(symbol, Is.InstanceOf<VariableSymbol>());
        var variableSymbol = (VariableSymbol)symbol;
        Assert.That(variableSymbol.Type, Is.InstanceOf<NamedTypeSymbol>());

        var type = (NamedTypeSymbol)variableSymbol.Type;
        Assert.That(type.Name.ToString(), Is.EqualTo("i32"));
        Assert.That(type.BuiltInType, Is.EqualTo(BuiltInType.I32));

        var y = scope.FindDeclarations("y");
        Assert.That(x, Has.Length.EqualTo(1));

        symbol = binder.GetSymbol(y[0], context);
        Assert.That(symbol, Is.InstanceOf<VariableSymbol>());
        variableSymbol = (VariableSymbol)symbol;
        Assert.That(variableSymbol.Type, Is.InstanceOf<NamedTypeSymbol>());

        type = (NamedTypeSymbol)variableSymbol.Type;
        Assert.That(type.Name.ToString(), Is.EqualTo("i32"));
        Assert.That(type.BuiltInType, Is.EqualTo(BuiltInType.I32));
    }

    [Test]
    public void BindVariableFromUnaryOperator()
    {
        var (syntax, context) = CreateCompilationUnit(
            """
            var x = !y;
            var y = true;
            """
        );

        Assert.That(context.Diagnostics, Is.Empty);

        var scope = syntax.ScanDeclarations();
        var x = scope.FindDeclarations("x");
        Assert.That(x, Has.Length.EqualTo(1));

        var binder = new SemanticBinder();
        var symbol = binder.GetSymbol(x[0], context);
        Assert.That(symbol, Is.InstanceOf<VariableSymbol>());
        var variableSymbol = (VariableSymbol)symbol;
        Assert.That(variableSymbol.Type, Is.InstanceOf<NamedTypeSymbol>());

        var type = (NamedTypeSymbol)variableSymbol.Type;
        Assert.That(type.Name.ToString(), Is.EqualTo("bool"));
        Assert.That(type.BuiltInType, Is.EqualTo(BuiltInType.Bool));

        var y = scope.FindDeclarations("y");
        Assert.That(x, Has.Length.EqualTo(1));

        symbol = binder.GetSymbol(y[0], context);
        Assert.That(symbol, Is.InstanceOf<VariableSymbol>());
        variableSymbol = (VariableSymbol)symbol;
        Assert.That(variableSymbol.Type, Is.InstanceOf<NamedTypeSymbol>());

        type = (NamedTypeSymbol)variableSymbol.Type;
        Assert.That(type.Name.ToString(), Is.EqualTo("bool"));
        Assert.That(type.BuiltInType, Is.EqualTo(BuiltInType.Bool));
    }
    */
}
