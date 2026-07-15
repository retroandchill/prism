// @file SpecificationTransformer.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CaseConverter;
using Prism.SyntaxGenerator.Models;

namespace Prism.SyntaxGenerator.Resolution;

public static class SpecificationTransformer
{
    private const string GreenFragmentName = "syntax.green";
    private const string RedFragmentName = "syntax";

    private const int TriviaSyntaxKindStartValue = 50;
    private const int KeywordSyntaxKindStartValue = 100;
    private const int PunctuationSyntaxKindStartValue = 200;
    private const int OtherSyntaxKindStartValue = 300;
    private const int NodeSyntaxKindStartValue = 400;
    private const int NodeSyntaxKindStep = 100;

    public static GeneratedSyntaxModel Transform(ResolvedSyntaxModel spec)
    {
        return new GeneratedSyntaxModel
        {
            SyntaxKinds = CollectSyntaxKindGroups(spec),
            Modules = [.. spec.Modules.Values.Select(Transform)],
        };
    }

    private static GeneratedSyntaxKindList CollectSyntaxKindGroups(ResolvedSyntaxModel spec)
    {
        var triviaGroup = new GeneratedSyntaxKindGroup
        {
            Name = "Trivia",
            CppName = "trivia",
            Kinds =
            [
                .. spec.Trivia.Select(
                    (trivia, i) =>
                        new GeneratedSyntaxKind
                        {
                            Name = trivia.Name,
                            CppName = $"{trivia.Name.ToSnakeCase()}_trivia",
                            DisplayName = trivia.DisplayName ?? trivia.Name.ToDisplayCase(),
                            Value = TriviaSyntaxKindStartValue + i,
                        }
                ),
            ],
            StartValue = TriviaSyntaxKindStartValue,
            EndValue = TriviaSyntaxKindStartValue + spec.Trivia.Length - 1,
        };

        var keywordsGroup = new GeneratedSyntaxKindGroup
        {
            Name = "Keywords",
            CppName = "keyword",
            Kinds =
            [
                .. spec.Tokens.Keywords.Select(
                    (keyword, i) =>
                        new GeneratedSyntaxKind
                        {
                            Name = keyword,
                            CppName = $"{keyword}_keyword",
                            DisplayName = keyword,
                            Value = KeywordSyntaxKindStartValue + i,
                        }
                ),
            ],
            StartValue = KeywordSyntaxKindStartValue,
            EndValue = KeywordSyntaxKindStartValue + spec.Tokens.Keywords.Length - 1,
        };

        var punctuationGroup = new GeneratedSyntaxKindGroup
        {
            Name = "Punctuation",
            CppName = "punctuation",
            Kinds =
            [
                .. spec.Tokens.Punctuations.Select(
                    (punctuation, i) =>
                        new GeneratedSyntaxKind
                        {
                            Name = punctuation.Name,
                            CppName = $"{punctuation.Name.ToSnakeCase()}_token",
                            DisplayName = punctuation.Value,
                            Value = PunctuationSyntaxKindStartValue + i,
                        }
                ),
            ],
            StartValue = PunctuationSyntaxKindStartValue,
            EndValue = PunctuationSyntaxKindStartValue + spec.Tokens.Punctuations.Length - 1,
        };

        var otherTokens = new GeneratedSyntaxKindGroup
        {
            Name = "Other Tokens",
            CppName = "other_token",
            Kinds =
            [
                .. spec.Tokens.Other.Select(
                    (other, i) =>
                        new GeneratedSyntaxKind
                        {
                            Name = other.Name,
                            CppName = $"{other.Name.ToSnakeCase()}_token",
                            DisplayName = other.DisplayName ?? other.Name.ToDisplayCase(),
                            Value = OtherSyntaxKindStartValue + i,
                        }
                ),
            ],
            StartValue = OtherSyntaxKindStartValue,
            EndValue = OtherSyntaxKindStartValue + spec.Tokens.Other.Length - 1,
        };

        var index = 0;
        var endValue = 0;
        var builder = ImmutableArray.CreateBuilder<GeneratedSyntaxKindGroup>(spec.Modules.Count);
        foreach (var module in spec.Modules.Values)
        {
            int tokenStart;

            do
            {
                tokenStart = NodeSyntaxKindStartValue + index * NodeSyntaxKindStep;
                index++;
            } while (tokenStart < endValue);

            var kinds = module
                .Nodes.Where(x => !x.IsAbstract)
                .Select(
                    (node, i) =>
                        new GeneratedSyntaxKind
                        {
                            Name = node.Name,
                            CppName = node.Name.ToSnakeCase(),
                            DisplayName = node.Name.ToDisplayCase(),
                            Value = tokenStart + i,
                        }
                )
                .ToImmutableArray();

            endValue = tokenStart + kinds.Length - 1;
            builder.Add(
                new GeneratedSyntaxKindGroup
                {
                    Name = module.Name.ToDisplayCase(),
                    CppName = module.Name.ToSnakeCase(),
                    Kinds = kinds,
                    StartValue = tokenStart,
                    EndValue = endValue,
                }
            );
        }

        var punctuationTrie = ConstructPunctuationTrie(punctuationGroup.Kinds);

        return new GeneratedSyntaxKindList
        {
            Trivia = triviaGroup,
            Tokens = new GeneratedTokens
            {
                Keywords = keywordsGroup,
                Punctuations = punctuationGroup,
                Others = otherTokens,

                KeywordsByLength =
                [
                    .. keywordsGroup
                        .Kinds.GroupBy(k => k.Name.Length)
                        .OrderBy(g => g.Key)
                        .Select(g => new GroupedKeywords { Length = g.Key, Keywords = [.. g] }),
                ],
                PunctuationTrie = RenderPunctuationTrie(punctuationTrie),
            },
            Nodes = builder.DrainToImmutable(),
        };
    }

    private static TrieNode ConstructPunctuationTrie(
        ImmutableArray<GeneratedSyntaxKind> punctuations
    )
    {
        var node = new TrieNode();
        foreach (var production in punctuations)
        {
            var current = node;

            foreach (var c in production.DisplayName)
            {
                if (current.Children.TryGetValue(c, out var child))
                {
                    current = child;
                }
                else
                {
                    child = new TrieNode();
                    current.Children[c] = child;
                    current = child;
                }
            }

            current.Value = production;
        }

        return node;
    }

    private static string RenderPunctuationTrie(TrieNode root)
    {
        // The recursive nature of this algorithm makes it difficult to render using raw Mustache templates, so we're
        // just going to render it ahead of time a splice it in.
        using var writer = new StringWriter();
        using var indentWriter = new IndentedTextWriter(writer);
        indentWriter.Indent = 2;
        WriteTrie(indentWriter, root);
        return writer.ToString();
    }

    private static void WriteTrie(IndentedTextWriter writer, TrieNode node)
    {
        switch (node.Children.Count)
        {
            case 1:
            {
                var (character, child) = node.Children.Single();
                writer.WriteLine($"if (cursor.current() == '{character}')");
                using var block = writer.OpenBlock();
                writer.WriteLine("cursor.advance();");
                WriteTrie(writer, child);
                break;
            }
            case > 1:
            {
                writer.WriteLine("switch (cursor.current())");
                using var block = writer.OpenBlock();
                // Sort by the key to ensure consistent output
                foreach (var (character, child) in node.Children.OrderBy(x => x.Key))
                {
                    writer.WriteLine($"case '{character}':");
                    using var unmarkedBlock = writer.OpenCaseBlock();
                    writer.WriteLine("cursor.advance();");
                    WriteTrie(writer, child);
                    if (child.Value is null)
                        writer.WriteLine("break;");
                }

                break;
            }
        }

        if (node.Value is { } terminal)
        {
            writer.WriteLine($"return SyntaxKind::{terminal.CppName};");
        }
    }

    private static string ToDisplayCase(this string str)
    {
        return str.ToPascalCase().SplitCamelCase().FirstCharToUpperCase();
    }

    private static GeneratedModule Transform(ResolvedModule module)
    {
        var nodes = module.Nodes.Select(Transform).ToImmutableArray();
        var moduleName = module.Name.ToSnakeCase();

        return new GeneratedModule
        {
            Name = moduleName,
            GreenName = $"{GreenFragmentName}.{moduleName}",
            RedName = $"{RedFragmentName}.{moduleName}",
            ForwardDeclarations =
            [
                .. module
                    .ForwardDeclarations.OrderBy(x => x.Name)
                    .Select(x => new Dependency(GetGreenName(x.Name), GetRedName(x.Name))),
            ],
            Imports =
            [
                .. module
                    .Dependencies.OrderBy(x => x.Name)
                    .Select(x => new Dependency(
                        $"{GreenFragmentName}.{x.Name}",
                        $"{RedFragmentName}.{x.Name}"
                    )),
            ],
            Nodes = nodes,
        };
    }

    private static GeneratedNode Transform(SyntaxNode spec)
    {
        var children = new GeneratedChild[spec.Children.Count];
        foreach (var (i, child) in spec.Children.Index())
        {
            children[i] = Transform(child);
        }

        return new GeneratedNode
        {
            Name = spec.Name,
            KindName = !spec.IsAbstract ? spec.Name.ToSnakeCase() : null,
            GreenName = $"Green{spec.Name}",
            GreenBase = GetGreenName(spec.Base?.Name),
            RedName = $"{spec.Name}Syntax",
            RedBase = GetRedName(spec.Base?.Name),
            ChildKinds =
            [
                .. spec.GetAllDerivedTypes()
                    .Where(x => !x.IsAbstract)
                    .Select(x => x.Name.ToSnakeCase()),
            ],
            Children = ImmutableCollectionsMarshal.AsImmutableArray(children),
        };
    }

    private static IEnumerable<SyntaxNode> GetAllDerivedTypes(this SyntaxNode node)
    {
        var stack = new Stack<SyntaxNode>(node.DerivedTypes.AsEnumerable().Reverse());
        while (stack.Count > 0)
        {
            var item = stack.Pop();
            yield return item;

            foreach (var child in item.DerivedTypes.AsEnumerable().Reverse())
            {
                stack.Push(child);
            }
        }
    }

    private static GeneratedChild Transform(SyntaxChild child)
    {
        var baseName = child.Name.ToSnakeCase();
        var fieldName = $"{baseName}_";

        var greenType = GetGreenName(child.Type.Name);
        var redType = GetRedName(child.Type.Name);

        string greenGetterType;
        string greenFieldType;
        string redGetterType;
        string greenGetterBody;
        string getChildrenExpression;
        string adjustExpression;
        switch (child.Shape)
        {
            case ChildShape.Single:
                greenGetterType = $"const {greenType}&";
                redGetterType = $"const {redType}&";
                greenFieldType = $"GreenPtr<{greenType}>";
                greenGetterBody = $"*{fieldName}";
                adjustExpression = greenGetterBody;
                getChildrenExpression = greenGetterBody;
                break;
            case ChildShape.Optional:
                greenGetterType = $"Optional<const {greenType}&>";
                redGetterType = $"Optional<const {redType}&>";
                greenFieldType = $"GreenPtr<{greenType}>";
                greenGetterBody = $"{fieldName}.get()";
                adjustExpression = $"*{fieldName}";
                getChildrenExpression = greenGetterBody;
                break;
            case ChildShape.List:
                greenGetterType = $"const GreenSyntaxList<{greenType}>&";
                redGetterType = $"SyntaxList<{redType}>";
                greenFieldType = $"GreenSyntaxList<{greenType}>";
                greenGetterBody = fieldName;
                getChildrenExpression = $"{fieldName}.node()";
                adjustExpression = fieldName;
                break;
            case ChildShape.SeparatedList:
                greenGetterType = $"const GreenSeparatedList<{greenType}>&";
                redGetterType = $"SeparatedSyntaxList<{redType}>";
                greenFieldType = $"GreenSeparatedList<{greenType}>";
                greenGetterBody = fieldName;
                getChildrenExpression = $"{fieldName}.node()";
                adjustExpression = fieldName;
                break;
            default:
                throw new InvalidOperationException("Unknown shape");
        }

        return new GeneratedChild
        {
            Name = baseName,
            FieldName = fieldName,
            GetterName = baseName,
            GreenGetterType = greenGetterType,
            GreenFieldType = greenFieldType,
            GreenGetterBody = greenGetterBody,
            GetChildExpression = getChildrenExpression,
            AdjustExpression = adjustExpression,
            RedGetterType = redGetterType,
            RedFieldType = $"RedPtr<{redType}>",
            IsOptional = child.Shape == ChildShape.Optional,
            IsOverride = child.IsOverride,
        };
    }

    private static string GetGreenName(string? baseName)
    {
        return baseName is not null ? $"Green{baseName}" : "GreenNode";
    }

    private static string GetRedName(string? baseName)
    {
        return baseName switch
        {
            "Token" => "SyntaxToken",
            "TokenList" => "SyntaxTokenList",
            "Node" or null => "SyntaxNode",
            _ => $"{baseName}Syntax",
        };
    }
}
