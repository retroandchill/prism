// @file LexerTests.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Parse;

namespace Prism.Core.Tests;

public class LexerTests
{
    private static List<TokenKind> LexAll(string source)
    {
        var lexer = new Lexer(source);
        var tokens = new List<TokenKind>();
        while (true)
        {
            var token = lexer.NextToken();
            tokens.Add(token.Kind);
            if (token.Kind == TokenKind.Eof)
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
                TokenKind.Func,
                TokenKind.Identifier,
                TokenKind.LParen,
                TokenKind.Identifier,
                TokenKind.Colon,
                TokenKind.I32,
                TokenKind.RParen,
                TokenKind.LBrace,
                TokenKind.Return,
                TokenKind.Identifier,
                TokenKind.Plus,
                TokenKind.IntegerLiteral,
                TokenKind.Semicolon,
                TokenKind.RBrace,
                TokenKind.Eof,
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
                TokenKind.Var,
                TokenKind.Identifier,
                TokenKind.Equal,
                TokenKind.IntegerLiteral,
                TokenKind.Semicolon,
                TokenKind.Eof,
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
                TokenKind.Var,
                TokenKind.Identifier,
                TokenKind.Equal,
                TokenKind.IntegerLiteral,
                TokenKind.Semicolon,
                TokenKind.Eof,
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
                TokenKind.Var,
                TokenKind.Identifier,
                TokenKind.Equal,
                TokenKind.IntegerLiteral,
                TokenKind.Semicolon,
                TokenKind.Eof,
            ])
        );
    }

    [Test]
    public void DocCommentsAreRetained()
    {
        const string source = """
            /**
             * This is a doc comment
             */
             var x = 5;
            """;
        var tokens = LexAll(source);
        Assert.That(
            tokens,
            Is.EquivalentTo([
                TokenKind.DocComment,
                TokenKind.Var,
                TokenKind.Identifier,
                TokenKind.Equal,
                TokenKind.IntegerLiteral,
                TokenKind.Semicolon,
                TokenKind.Eof,
            ])
        );
    }
}
