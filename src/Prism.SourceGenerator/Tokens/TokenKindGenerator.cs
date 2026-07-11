// @file TokenKindGenerator.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Prism.Core.Parse;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;

namespace Prism.SourceGenerator.Tokens;

[Generator]
public sealed class TokenKindGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor InvalidTokenKindAttribute = new(
        "PRSM0001",
        "Invalid TokenKindAttribute",
        "Enum member {Member} on {Type} may be labeled with either KeywordTokenAttribute or PunctuationTokenAttribute, but not both",
        "Prism",
        DiagnosticSeverity.Error,
        true
    );

    private TokenKindEmitter _emitter = null!;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        _emitter = new TokenKindEmitter();
        var fullName = typeof(TokenKindAttribute).FullName!;
        var tokenKindEnums = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullName,
            static (node, _) => node is EnumDeclarationSyntax,
            static (node, _) => (INamedTypeSymbol)node.TargetSymbol
        );

        SyntaxToken span;
        context.RegisterSourceOutput(tokenKindEnums, Execute);
    }

    private void Execute(SourceProductionContext context, INamedTypeSymbol enumSymbol)
    {
        var fields = enumSymbol.GetMembers().OfType<IFieldSymbol>().ToImmutableArray();
        var keywordsBuilder = new List<TokenLabel>(fields.Length);
        var punctuationsBuilder = new List<TokenLabel>(fields.Length);
        foreach (var field in fields)
        {
            var isKeyword = field.TryGetKeywordTokenInfo(out var keywordInfo);
            var isPunctuation = field.TryGetPunctuationTokenInfo(out var punctuationInfo);

            if (!isKeyword && !isPunctuation)
                continue;

            if (isKeyword && isPunctuation)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        InvalidTokenKindAttribute,
                        field.Locations[0],
                        field.Name,
                        enumSymbol.Name
                    )
                );
            }

            SyntaxTriviaList triviaList = default;
            if (isKeyword)
            {
                keywordsBuilder.Add(new TokenLabel(field.Name, keywordInfo.Token));
            }

            if (isPunctuation)
            {
                punctuationsBuilder.Add(new TokenLabel(field.Name, punctuationInfo.Token));
            }
        }

        var payload = new TokenKindData
        {
            Namespace = enumSymbol.ContainingNamespace.ToDisplayString(),
            EnumName = enumSymbol.Name,
            Keywords = [.. keywordsBuilder],
            Punctuations = [.. punctuationsBuilder.OrderByDescending(x => x.Value.Length)],
        };

        context.AddSource($"{enumSymbol.Name}.g.cs", _emitter.Emit(payload));
    }
}
