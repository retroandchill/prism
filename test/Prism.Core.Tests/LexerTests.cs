// @file LexerTests.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Parse;
using Prism.Core.Syntax;

namespace Prism.Core.Tests;

public class LexerTests
{
    private static List<SyntaxKind> LexAll(string source)
    {
        var lexer = new Lexer(new SourceFile(source));
        var tokens = new List<SyntaxKind>();
        while (true)
        {
            var token = lexer.Next();
            tokens.Add(token.Kind);
            if (token.Kind == SyntaxKind.EndOfFileToken)
                break;
        }

        return tokens;
    }

    [Test]
    public void SimpleGrammar()
    {
        const string source = """
            func do_thing(x: i32) {
              return x + 1;
            }
            """;
        var tokens = LexAll(source);
        Assert.That(
            tokens,
            Is.EquivalentTo([
                SyntaxKind.FuncKeyword,
                SyntaxKind.IdentifierToken,
                SyntaxKind.LParenToken,
                SyntaxKind.IdentifierToken,
                SyntaxKind.ColonToken,
                SyntaxKind.I32Keyword,
                SyntaxKind.RParenToken,
                SyntaxKind.LBraceToken,
                SyntaxKind.ReturnKeyword,
                SyntaxKind.IdentifierToken,
                SyntaxKind.PlusToken,
                SyntaxKind.IntegerLiteralToken,
                SyntaxKind.SemicolonToken,
                SyntaxKind.RBraceToken,
                SyntaxKind.EndOfFileToken,
            ])
        );
    }

    [Test]
    public void SkipsLineComments()
    {
        const string source = """
            // This is a line comment
            var x = 5; // This is another line comment
            """;
        var tokens = LexAll(source);
        Assert.That(
            tokens,
            Is.EquivalentTo([
                SyntaxKind.VarKeyword,
                SyntaxKind.IdentifierToken,
                SyntaxKind.EqualToken,
                SyntaxKind.IntegerLiteralToken,
                SyntaxKind.SemicolonToken,
                SyntaxKind.EndOfFileToken,
            ])
        );
    }

    [Test]
    public void BlockCommentsSplitLine()
    {
        const string source = "var x = /* This is a block comment */ 5;";
        var tokens = LexAll(source);
        Assert.That(
            tokens,
            Is.EquivalentTo([
                SyntaxKind.VarKeyword,
                SyntaxKind.IdentifierToken,
                SyntaxKind.EqualToken,
                SyntaxKind.IntegerLiteralToken,
                SyntaxKind.SemicolonToken,
                SyntaxKind.EndOfFileToken,
            ])
        );
    }

    [Test]
    public void BlockCommentSpansMultipleLines()
    {
        const string source = """
            /*
            This is a block comment
            that spans multiple lines
            */
            var x = 5;
            """;
        var tokens = LexAll(source);
        Assert.That(
            tokens,
            Is.EquivalentTo([
                SyntaxKind.VarKeyword,
                SyntaxKind.IdentifierToken,
                SyntaxKind.EqualToken,
                SyntaxKind.IntegerLiteralToken,
                SyntaxKind.SemicolonToken,
                SyntaxKind.EndOfFileToken,
            ])
        );
    }
}
