// @file EnumParsingTests.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Syntax.Green;
using Prism.Syntax.Parser;

namespace Prism.Tests;

public class EnumParsingTests
{
    [Test]
    public void MinimalEnumSyntax()
    {
        var parser = new LanguageParser(
            """
            public enum Color {
                Red,
                Blue,
                Green
            }
            """
        );
        var declaration = parser.ParseTopLevelDeclaration();

        Assert.That(declaration, Is.InstanceOf<GreenEnumDeclaration>());
        Assert.That(declaration.Diagnostics, Is.Empty);
        var enumDeclaration = (GreenEnumDeclaration)declaration;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(enumDeclaration.Ordinals, Has.Count.EqualTo(3));
            Assert.That(enumDeclaration.Members, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public void MinimalEnumWithTrailingComma()
    {
        var parser = new LanguageParser(
            """
            public enum Color {
                Red,
                Blue,
                Green,
            }
            """
        );
        var declaration = parser.ParseTopLevelDeclaration();

        Assert.That(declaration, Is.InstanceOf<GreenEnumDeclaration>());
        Assert.That(declaration.Diagnostics, Is.Empty);
        var enumDeclaration = (GreenEnumDeclaration)declaration;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(enumDeclaration.Ordinals, Has.Count.EqualTo(3));
            Assert.That(enumDeclaration.Members, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public void MinimalEnumWithSemicolon()
    {
        var parser = new LanguageParser(
            """
            public enum Color {
                Red,
                Blue,
                Green;
            }
            """
        );
        var declaration = parser.ParseTopLevelDeclaration();

        Assert.That(declaration, Is.InstanceOf<GreenEnumDeclaration>());
        Assert.That(declaration.Diagnostics, Is.Empty);
        var enumDeclaration = (GreenEnumDeclaration)declaration;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(enumDeclaration.Ordinals, Has.Count.EqualTo(3));
            Assert.That(enumDeclaration.Members, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public void MinimalEnumWithTrailingCommaAndSemicolon()
    {
        var parser = new LanguageParser(
            """
            public enum Color {
                Red,
                Blue,
                Green,
                ;
            }
            """
        );
        var declaration = parser.ParseTopLevelDeclaration();

        Assert.That(declaration, Is.InstanceOf<GreenEnumDeclaration>());
        Assert.That(declaration.Diagnostics, Is.Empty);
        var enumDeclaration = (GreenEnumDeclaration)declaration;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(enumDeclaration.Ordinals, Has.Count.EqualTo(3));
            Assert.That(enumDeclaration.Members, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public void MinimalEnumWithSemicolonAndMembers()
    {
        var parser = new LanguageParser(
            """
            public enum Color {
                Red,
                Blue,
                Green;

                public func is_red(): bool => this == Red;
            }
            """
        );
        var declaration = parser.ParseTopLevelDeclaration();

        Assert.That(declaration, Is.InstanceOf<GreenEnumDeclaration>());
        Assert.That(declaration.Diagnostics, Is.Empty);
        var enumDeclaration = (GreenEnumDeclaration)declaration;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(enumDeclaration.Ordinals, Has.Count.EqualTo(3));
            Assert.That(enumDeclaration.Members, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public void EnumWithoutOrdinals()
    {
        var parser = new LanguageParser(
            """
            public enum Color {
                ;

                public func is_red(): bool => this == Red;
            }
            """
        );
        var declaration = parser.ParseTopLevelDeclaration();

        Assert.That(declaration, Is.InstanceOf<GreenEnumDeclaration>());
        Assert.That(declaration.Diagnostics, Is.Empty);
        var enumDeclaration = (GreenEnumDeclaration)declaration;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(enumDeclaration.Ordinals, Has.Count.EqualTo(0));
            Assert.That(enumDeclaration.Members, Has.Count.EqualTo(1));
        }
    }
}
