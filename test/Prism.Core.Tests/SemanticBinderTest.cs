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
    private static readonly TargetPlatform TargetPlatform = new()
    {
        PointerSize = 8
    };
    
    private static SourceUnit CreateCompilationUnit(
        string code
    )
    {
        var context = new SourceDocument(code);
        var parser = new Parser(context);
        return parser.ParseCompilationUnit();
    }

    [Test]
    public async Task BindVariableExplicitType()
    {
        var unit = CreateCompilationUnit("var x: i32 = 5;");

        Assert.That(unit.Diagnostics, Is.Empty);

        var compilation = new Compilation(TargetPlatform);
        compilation.SemanticModel.AddCompilationUnit(unit.Syntax);
        var scope = compilation.SemanticModel.GlobalScope;
        var x = scope.GetDeclaredHere("x");
        Assert.That(x, Has.Length.EqualTo(1));
        
        var symbol = x[0];
        Assert.That(symbol, Is.InstanceOf<VariableSymbol>());
        var variableSymbol = (VariableSymbol)symbol;
        var bindingContext = new BindingContext(unit.Diagnostics, ResolutionContext.Empty);
        var type = await compilation.SemanticModel.ResolveValueTypeAsync(variableSymbol, bindingContext);
        Assert.That(type, Is.InstanceOf<NamedTypeSymbol>());
        
        var namedType = (NamedTypeSymbol)type;
        Assert.That(type.Name.ToString(), Is.EqualTo("i32"));
        Assert.That(namedType.BuiltInType, Is.EqualTo(BuiltInType.I32));
    }

    [Test]
    public async Task BindVariableInferredType()
    {
        var unit = CreateCompilationUnit("var x: i32 = 5;");

        Assert.That(unit.Diagnostics, Is.Empty);

        var compilation = new Compilation(TargetPlatform);
        compilation.SemanticModel.AddCompilationUnit(unit.Syntax);
        var scope = compilation.SemanticModel.GlobalScope;
        var x = scope.GetDeclaredHere("x");
        Assert.That(x, Has.Length.EqualTo(1));
        
        var symbol = x[0];
        Assert.That(symbol, Is.InstanceOf<VariableSymbol>());
        var variableSymbol = (VariableSymbol)symbol;
        var bindingContext = new BindingContext(unit.Diagnostics, ResolutionContext.Empty);
        var type = await compilation.SemanticModel.ResolveValueTypeAsync(variableSymbol, bindingContext);
        Assert.That(type, Is.InstanceOf<NamedTypeSymbol>());
        
        var namedType = (NamedTypeSymbol)type;
        Assert.That(type.Name.ToString(), Is.EqualTo("i32"));
        Assert.That(namedType.BuiltInType, Is.EqualTo(BuiltInType.I32));
    }

    [Test]
    public async Task BindVariableFromOtherVariable()
    {
        var unit = CreateCompilationUnit(
            """
            var x = y;
            var y = 4;
            """
        );

        Assert.That(unit.Diagnostics, Is.Empty);

        var compilation = new Compilation(TargetPlatform);
        compilation.SemanticModel.AddCompilationUnit(unit.Syntax);
        var scope = compilation.SemanticModel.GlobalScope;
        var x = scope.GetDeclaredHere("x");
        Assert.That(x, Has.Length.EqualTo(1));
        
        var symbol = x[0];
        Assert.That(symbol, Is.InstanceOf<VariableSymbol>());
        var variableSymbol = (VariableSymbol)symbol;
        var bindingContext = new BindingContext(unit.Diagnostics, ResolutionContext.Empty);
        var type = await compilation.SemanticModel.ResolveValueTypeAsync(variableSymbol, bindingContext);
        Assert.That(type, Is.InstanceOf<NamedTypeSymbol>());
        
        var namedType = (NamedTypeSymbol)type;
        Assert.That(type.Name.ToString(), Is.EqualTo("i32"));
        Assert.That(namedType.BuiltInType, Is.EqualTo(BuiltInType.I32));

        var y = scope.GetDeclaredHere("y");
        Assert.That(x, Has.Length.EqualTo(1));

        symbol = y[0];
        Assert.That(symbol, Is.InstanceOf<VariableSymbol>());
        variableSymbol = (VariableSymbol)symbol;
        type = await compilation.SemanticModel.ResolveValueTypeAsync(variableSymbol, bindingContext);
        Assert.That(type, Is.InstanceOf<NamedTypeSymbol>());

        namedType = (NamedTypeSymbol)type;
        Assert.That(type.Name.ToString(), Is.EqualTo("i32"));
        Assert.That(namedType.BuiltInType, Is.EqualTo(BuiltInType.I32));
    }

    [Test]
    public async Task BindVariableFromUnaryOperator()
    {
        var unit = CreateCompilationUnit(
            """
            var x = !y;
            var y = true;
            """
        );

        Assert.That(unit.Diagnostics, Is.Empty);

        var compilation = new Compilation(TargetPlatform);
        compilation.SemanticModel.AddCompilationUnit(unit.Syntax);
        var scope = compilation.SemanticModel.GlobalScope;
        var x = scope.GetDeclaredHere("x");
        Assert.That(x, Has.Length.EqualTo(1));
        
        var symbol = x[0];
        Assert.That(symbol, Is.InstanceOf<VariableSymbol>());
        var variableSymbol = (VariableSymbol)symbol;
        var bindingContext = new BindingContext(unit.Diagnostics, ResolutionContext.Empty);
        var type = await compilation.SemanticModel.ResolveValueTypeAsync(variableSymbol, bindingContext);
        Assert.That(type, Is.InstanceOf<NamedTypeSymbol>());
        
        var namedType = (NamedTypeSymbol)type;
        Assert.That(type.Name.ToString(), Is.EqualTo("bool"));
        Assert.That(namedType.BuiltInType, Is.EqualTo(BuiltInType.Bool));

        var y = scope.GetDeclaredHere("y");
        Assert.That(x, Has.Length.EqualTo(1));

        symbol = y[0];
        Assert.That(symbol, Is.InstanceOf<VariableSymbol>());
        variableSymbol = (VariableSymbol)symbol;
        type = await compilation.SemanticModel.ResolveValueTypeAsync(variableSymbol, bindingContext);
        Assert.That(type, Is.InstanceOf<NamedTypeSymbol>());

        namedType = (NamedTypeSymbol)type;
        Assert.That(type.Name.ToString(), Is.EqualTo("bool"));
        Assert.That(namedType.BuiltInType, Is.EqualTo(BuiltInType.Bool));
    }
    
    [Test]
    public async Task UnableToBindCycle()
    {
        var unit = CreateCompilationUnit(
            """
            var x = y;
            var y = x;
            """
        );

        Assert.That(unit.Diagnostics, Is.Empty);

        var compilation = new Compilation(TargetPlatform);
        compilation.SemanticModel.AddCompilationUnit(unit.Syntax);
        var scope = compilation.SemanticModel.GlobalScope;
        var x = scope.GetDeclaredHere("x");
        Assert.That(x, Has.Length.EqualTo(1));
        
        var symbol = x[0];
        Assert.That(symbol, Is.InstanceOf<VariableSymbol>());
        var variableSymbol = (VariableSymbol)symbol;
        var bindingContext = new BindingContext(unit.Diagnostics, ResolutionContext.Empty);
        var type = await compilation.SemanticModel.ResolveValueTypeAsync(variableSymbol, bindingContext);
        Assert.That(type, Is.InstanceOf<ErrorTypeSymbol>());
        
        Assert.That(unit.Diagnostics, Has.Count.EqualTo(1));

        var y = scope.GetDeclaredHere("y");
        Assert.That(x, Has.Length.EqualTo(1));

        symbol = y[0];
        Assert.That(symbol, Is.InstanceOf<VariableSymbol>());
        variableSymbol = (VariableSymbol)symbol;
        type = await compilation.SemanticModel.ResolveValueTypeAsync(variableSymbol, bindingContext);
        Assert.That(type, Is.InstanceOf<ErrorTypeSymbol>());
    }

    [Test]
    public async Task FullSymbolResolution()
    {
        var unit = CreateCompilationUnit(
            """
            var x: bool = !y;
            var y = true;
            """
        );

        Assert.That(unit.Diagnostics, Is.Empty);

        var compilation = new Compilation(TargetPlatform);
        compilation.SemanticModel.AddCompilationUnit(unit.Syntax);

        await compilation.SemanticModel.BindSymbolsAsync();

        var scope = compilation.SemanticModel.GlobalScope;
        var x = scope.GetDeclaredHere("x");
        Assert.That(x, Has.Length.EqualTo(1));
        
        var symbol = x[0];
        Assert.That(symbol, Is.InstanceOf<VariableSymbol>());
        var variableSymbol = (VariableSymbol)symbol;
        Assert.That(variableSymbol.Type, Is.InstanceOf<NamedTypeSymbol>());
        Assert.That(variableSymbol.Initializer, Is.Not.Null);
        
        var namedType = (NamedTypeSymbol)variableSymbol.Type;
        Assert.That(namedType.Name.ToString(), Is.EqualTo("bool"));
        Assert.That(namedType.BuiltInType, Is.EqualTo(BuiltInType.Bool));
        
        
    }
}
