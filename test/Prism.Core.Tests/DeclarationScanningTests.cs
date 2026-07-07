// @file DeclarationScanningTests.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Ast;
using Prism.Core.Parse;
using Prism.Core.Semantic;

namespace Prism.Core.Tests;

public class DeclarationScanningTests
{
    private readonly TargetPlatform _targetPlatform = new()
    {
        PointerSize = 8
    };
    
    private static SourceUnit CreateCompilationUnit(string code)
    {
        var context = new SourceDocument(code);
        var parser = new Parser(context);
        return parser.ParseCompilationUnit();
    }

    [Test]
    public void ScanForDeclarations()
    {
        var unit = CreateCompilationUnit(
            """
            var x: i32 = 5;
            func f() {
              print("Hello world");
            }
            extern func f(x: i32);
            func g(x: i32, y: i32) -> i32 => x + y;
            """
        );

        Assert.That(unit.Diagnostics, Is.Empty);

        var compilation = new Compilation(_targetPlatform);
        var semanticModel = compilation.SemanticModel;
        semanticModel.AddCompilationUnit(unit.Syntax);
        var scope = semanticModel.GlobalScope;
        var x = scope.FindDeclarations("x");
        Assert.That(x, Has.Length.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(x[0].Name.ToString(), Is.EqualTo("x"));
            Assert.That(x[0].Declaration, Is.InstanceOf<VariableDeclarationSyntax>());
        }

        var f = scope.FindDeclarations("f");
        Assert.That(f, Has.Length.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(f[0].Name.ToString(), Is.EqualTo("f"));
            Assert.That(f[0].Declaration, Is.InstanceOf<FunctionDeclarationSyntax>());
            Assert.That(f[1].Name.ToString(), Is.EqualTo("f"));
            Assert.That(f[1].Declaration, Is.InstanceOf<FunctionDeclarationSyntax>());
        }

        var g = scope.FindDeclarations("g");
        Assert.That(g, Has.Length.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(g[0].Name.ToString(), Is.EqualTo("g"));
            Assert.That(g[0].Declaration, Is.InstanceOf<FunctionDeclarationSyntax>());
        }

        var childScope = semanticModel.GetOwnedScope(g[0].Declaration!);
        Assert.That(childScope, Is.Not.Null);

        var parameters = childScope.FindDeclarations("x");
        Assert.That(parameters, Has.Length.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(parameters[0].Name.ToString(), Is.EqualTo("x"));
            Assert.That(parameters[0].Declaration, Is.InstanceOf<ParameterDeclarationSyntax>());
        }

        var allParameters = childScope.FindAllDeclarations("x").ToImmutableArray();
        Assert.That(allParameters, Has.Length.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(allParameters[0].Name.ToString(), Is.EqualTo("x"));
            Assert.That(allParameters[0].Declaration, Is.InstanceOf<ParameterDeclarationSyntax>());
            Assert.That(allParameters[1].Name.ToString(), Is.EqualTo("x"));
            Assert.That(allParameters[1].Declaration, Is.InstanceOf<VariableDeclarationSyntax>());
        }
    }
}
